using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using BombermanGame.Models;
using BombermanGame.Events;
using BombermanGame.Factories;
using BombermanGame.PowerUps;
using BombermanGame.Hubs;

namespace BombermanGame.Services;

public class GameService : IGameService
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly Timer _gameTimer;
    private readonly IGameFactory _gameFactory;
    private readonly IEventPublisher _eventPublisher;
    private readonly IHubContext<GameHub> _hubContext;

    public GameService(IGameFactory gameFactory, IEventPublisher eventPublisher, IHubContext<GameHub> hubContext)
    {
        _gameFactory = gameFactory;
        _eventPublisher = eventPublisher;
        _hubContext = hubContext;
        _gameTimer = new Timer(UpdateGames, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
    }

    public GameRoom CreateRoom(string roomId)
    {
        var room = _gameFactory.CreateGameRoom(roomId);
        _rooms.TryAdd(roomId, room);
        return room;
    }

    public GameRoom? GetRoom(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    public async Task<bool> JoinRoomAsync(string roomId, Player player)
    {
        var room = GetRoom(roomId) ?? CreateRoom(roomId);

        if (!room.StateHandler.CanJoinPlayer(room))
            return false;

        var spawnPositions = new[] { (1, 1), (13, 1), (1, 11), (13, 11) };
        var spawn = spawnPositions[room.Players.Count];
        player.X = spawn.Item1;
        player.Y = spawn.Item2;
        player.Color = new[] { "#ff0000", "#0000ff", "#00ff00", "#ffff00" }[room.Players.Count];

        room.Players.Add(player);

        await _eventPublisher.PublishAsync(new PlayerJoinedEvent
        {
            RoomId = roomId,
            Player = player
        });

        return true;
    }

    public async Task StartGameAsync(string roomId)
    {
        var room = GetRoom(roomId);
        if (room != null && room.StateHandler.CanStartGame(room))
        {
            room.State = GameState.Playing;
            room.Board = _gameFactory.CreateGameBoard();
            room.UpdateStateHandler();

            await _eventPublisher.PublishAsync(new GameStartedEvent
            {
                RoomId = roomId
            });
        }
    }

    public async Task<bool> MovePlayerAsync(string roomId, string playerId, int deltaX, int deltaY)
    {
        var room = GetRoom(roomId);
        var player = room?.Players.FirstOrDefault(p => p.Id == playerId);

        if (room == null || player == null || !player.IsAlive || !room.StateHandler.CanMovePlayer(room))
            return false;

        var newX = player.X + deltaX;
        var newY = player.Y + deltaY;

        if (player.MovementStrategy.CanMove(room.Board, newX, newY))
        {
            player.X = newX;
            player.Y = newY;

            var powerUp = room.Board.PowerUps.FirstOrDefault(p => p.X == newX && p.Y == newY);
            if (powerUp != null)
            {
                var powerUpEffect = PowerUpFactory.CreatePowerUp(powerUp.Type);
                powerUpEffect.ApplyEffect(player);
                room.Board.PowerUps.Remove(powerUp);
            }

            await _eventPublisher.PublishAsync(new PlayerMovedEvent
            {
                RoomId = roomId,
                Player = player
            });

            return true;
        }
        return false;
    }

    public async Task<bool> PlaceBombAsync(string roomId, string playerId)
    {
        var room = GetRoom(roomId);
        var player = room?.Players.FirstOrDefault(p => p.Id == playerId);

        if (room == null || player == null || !player.IsAlive || !room.StateHandler.CanPlaceBomb(room))
            return false;

        var playerBombs = room.Board.Bombs.Count(b => b.PlayerId == playerId);
        if (playerBombs >= player.BombCount)
            return false;

        if (room.Board.Bombs.Any(b => b.X == player.X && b.Y == player.Y))
            return false;

        var bomb = new Bomb
        {
            X = player.X,
            Y = player.Y,
            PlayerId = playerId,
            Range = player.BombRange
        };

        room.Board.Bombs.Add(bomb);

        await _eventPublisher.PublishAsync(new BombPlacedEvent
        {
            RoomId = roomId,
            Bomb = bomb
        });

        return true;
    }

    private async void UpdateGames(object? state)
    {
        var now = DateTime.Now;

        foreach (var room in _rooms.Values.ToList())
        {
            if (room.State != GameState.Playing) continue;

            var updated = false;

            var bombsToExplode = room.Board.Bombs.Where(b => (now - b.PlacedAt).TotalSeconds >= 3).ToList();
            foreach (var bomb in bombsToExplode)
            {
                var explosions = ExplodeBomb(room, bomb);
                room.Board.Bombs.Remove(bomb);

                await _eventPublisher.PublishAsync(new BombExplodedEvent
                {
                    RoomId = room.Id,
                    Bomb = bomb,
                    Explosions = explosions
                });

                updated = true;
            }

            if (room.Board.Explosions.RemoveAll(e => (now - e.CreatedAt).TotalSeconds >= 1) > 0)
            {
                updated = true;
            }

            var alivePlayers = room.Players.Count(p => p.IsAlive);
            if (alivePlayers <= 1)
            {
                room.State = GameState.Finished;
                room.UpdateStateHandler();
                updated = true;
            }

            if (updated)
            {
                room.LastUpdate = now;
                await _hubContext.Clients.Group(room.Id).SendAsync("GameUpdated", room);
            }
        }
    }

    private List<Explosion> ExplodeBomb(GameRoom room, Bomb bomb)
    {
        var explosions = new List<Explosion>();
        var directions = new[] { (0, 0), (0, 1), (0, -1), (1, 0), (-1, 0) };

        foreach (var (dx, dy) in directions)
        {
            for (int i = 0; i < bomb.Range; i++)
            {
                var x = bomb.X + dx * i;
                var y = bomb.Y + dy * i;

                if (x < 0 || x >= GameBoard.Width || y < 0 || y >= GameBoard.Height)
                    break;

                var cellType = (CellType)room.Board.Grid[y][x];
                if (cellType == CellType.Wall)
                    break;

                explosions.Add(new Explosion { X = x, Y = y });

                var hitPlayer = room.Players.FirstOrDefault(p => p.X == x && p.Y == y && p.IsAlive);
                if (hitPlayer != null)
                {
                    hitPlayer.IsAlive = false;
                }

                if (cellType == CellType.DestructibleWall)
                {
                    room.Board.Grid[y][x] = (int)CellType.Empty;

                    if (Random.Shared.NextDouble() < 0.3)
                    {
                        var powerUpType = (PowerUpType)Random.Shared.Next(0, 3);
                        room.Board.PowerUps.Add(new PowerUp { X = x, Y = y, Type = powerUpType });
                    }

                    break;
                }
            }
        }

        room.Board.Explosions.AddRange(explosions);
        return explosions;
    }

    public List<GameRoom> GetRooms()
    {
        return _rooms.Values.ToList();
    }
}