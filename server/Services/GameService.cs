using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using BombermanGame.Models;
using BombermanGame.Events;
using BombermanGame.Factories;
using BombermanGame.PowerUps;
using BombermanGame.Hubs;
using BombermanGame.Decorators;
using BombermanGame.Prototypes;
using BombermanGame.Singletons;

namespace BombermanGame.Services;

public class GameService : IGameService
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly Dictionary<string, PlayerDecoratorManager> _roomDecorators = new();
    private readonly Timer _gameTimer;
    private readonly IGameFactory _gameFactory;
    private readonly IGameElementFactory _elementFactory;
    private readonly IEventPublisher _eventPublisher;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly PrototypeManager _prototypeManager;

    private readonly GameConfiguration _config = GameConfiguration.Instance;
    private readonly GameStatistics _statistics = GameStatistics.Instance;
    private readonly GameLogger _logger = GameLogger.Instance;

    public GameService(
        IGameFactory gameFactory,
        IGameElementFactory elementFactory,
        IEventPublisher eventPublisher,
        IHubContext<GameHub> hubContext,
        PrototypeManager prototypeManager)
    {
        _gameFactory = gameFactory;
        _elementFactory = elementFactory;
        _eventPublisher = eventPublisher;
        _hubContext = hubContext;
        _prototypeManager = prototypeManager;

        _gameTimer = new Timer(
            UpdateGames,
            null,
            TimeSpan.FromMilliseconds(_config.GameUpdateIntervalMs),
            TimeSpan.FromMilliseconds(_config.GameUpdateIntervalMs)
        );

        InitializePrototypes();

        _logger.LogInfo("GameService", "Service initialized with singleton configuration");
    }

    private void InitializePrototypes()
    {
        var defaultPlayer = new Player
        {
            Name = "Default",
            X = 1,
            Y = 1,
            BombCount = _config.DefaultBombCount,
            BombRange = _config.DefaultBombRange,
            IsAlive = true
        };
        _prototypeManager.RegisterPlayerPrototype("default", defaultPlayer);

        var powerPlayer = new Player
        {
            Name = "Power",
            X = 1,
            Y = 1,
            BombCount = _config.DefaultBombCount + 2,
            BombRange = _config.DefaultBombRange + 2,
            IsAlive = true
        };
        _prototypeManager.RegisterPlayerPrototype("power", powerPlayer);

        var standardBoard = new GameBoard();
        _prototypeManager.RegisterBoardPrototype("standard", standardBoard);

        _logger.LogInfo("Prototypes", "Player and board prototypes registered");
    }

    public GameRoom CreateRoom(string roomId)
    {
        var room = _gameFactory.CreateGameRoom(roomId);
        _rooms.TryAdd(roomId, room);
        _roomDecorators[roomId] = new PlayerDecoratorManager();

        _logger.LogInfo("Room", $"Room created: {roomId}");
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
        {
            _logger.LogWarning("Room", $"Player {player.Name} cannot join room {roomId} - room full or game started");
            return false;
        }

        var playerCount = room.Players.Count;
        if (playerCount >= _config.MaxPlayersPerRoom)
        {
            _logger.LogWarning("Room", $"Room {roomId} is full");
            return false;
        }

        var spawn = _config.GetSpawnPosition(playerCount);
        player.X = spawn.X;
        player.Y = spawn.Y;
        player.Color = _config.GetPlayerColor(playerCount);

        room.Players.Add(player);

        _roomDecorators[roomId].RegisterPlayer(player);

        _statistics.RecordGamePlayed(player.Id);

        await _eventPublisher.PublishAsync(new PlayerJoinedEvent
        {
            RoomId = roomId,
            Player = player
        });

        _logger.LogInfo("Room", $"Player {player.Name} joined room {roomId}");
        return true;
    }

    public async Task StartGameAsync(string roomId)
    {
        var room = GetRoom(roomId);

        if (room != null && room.StateHandler.CanStartGame(room))
        {
            room.State = GameState.Playing;

            var prototypeBoard = _prototypeManager.CreateBoardFromPrototype("standard");
            room.Board = prototypeBoard ?? _gameFactory.CreateGameBoard();

            room.UpdateStateHandler();

            await _eventPublisher.PublishAsync(new GameStartedEvent
            {
                RoomId = roomId
            });

            _logger.LogInfo("Game", $"Game started in room {roomId} with {room.Players.Count} players");
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

                if (_roomDecorators.TryGetValue(roomId, out var decoratorManager))
                {
                    switch (powerUp.Type)
                    {
                        case PowerUpType.BombUp:
                            decoratorManager.ApplyBombUpgrade(playerId);
                            _logger.LogInfo("PowerUp", $"Player {player.Name} collected BombUp");
                            break;
                        case PowerUpType.RangeUp:
                            decoratorManager.ApplyRangeUpgrade(playerId);
                            _logger.LogInfo("PowerUp", $"Player {player.Name} collected RangeUp");
                            break;
                        case PowerUpType.SpeedUp:
                            decoratorManager.ApplySpeedUpgrade(playerId);
                            _logger.LogInfo("PowerUp", $"Player {player.Name} collected SpeedUp");
                            break;
                    }
                }

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

        var effectiveBombCount = player.BombCount;
        var effectiveBombRange = player.BombRange;

        if (_roomDecorators.TryGetValue(roomId, out var decoratorManager))
        {
            effectiveBombCount = decoratorManager.GetEffectiveBombCount(playerId);
            effectiveBombRange = decoratorManager.GetEffectiveBombRange(playerId);
        }

        var playerBombs = room.Board.Bombs.Count(b => b.PlayerId == playerId);
        if (playerBombs >= effectiveBombCount)
            return false;

        if (room.Board.Bombs.Any(b => b.X == player.X && b.Y == player.Y))
            return false;

        var bomb = _elementFactory.CreateBomb(player.X, player.Y, playerId, effectiveBombRange);
        room.Board.Bombs.Add(bomb);

        _statistics.RecordBombPlaced(playerId);

        await _eventPublisher.PublishAsync(new BombPlacedEvent
        {
            RoomId = roomId,
            Bomb = bomb
        });

        _logger.LogInfo("Bomb", $"Player {player.Name} placed bomb at ({player.X}, {player.Y})");
        return true;
    }

    private async void UpdateGames(object? state)
    {
        var now = DateTime.Now;

        foreach (var room in _rooms.Values.ToList())
        {
            if (room.State != GameState.Playing) continue;

            var updated = false;

            var bombsToExplode = room.Board.Bombs
                .Where(b => (now - b.PlacedAt).TotalSeconds >= _config.BombExplosionTimeSeconds)
                .ToList();

            foreach (var bomb in bombsToExplode)
            {
                var explosions = ExplodeBomb(room, bomb);
                room.Board.Bombs.Remove(bomb);

                _statistics.RecordBombExploded();

                await _eventPublisher.PublishAsync(new BombExplodedEvent
                {
                    RoomId = room.Id,
                    Bomb = bomb,
                    Explosions = explosions
                });

                updated = true;
            }

            if (room.Board.Explosions.RemoveAll(e =>
                (now - e.CreatedAt).TotalSeconds >= _config.ExplosionDurationSeconds) > 0)
            {
                updated = true;
            }

            var alivePlayers = room.Players.Where(p => p.IsAlive).ToList();
            if (alivePlayers.Count <= 1)
            {
                room.State = GameState.Finished;
                room.UpdateStateHandler();

                if (alivePlayers.Count == 1)
                {
                    var winner = alivePlayers[0];
                    _statistics.RecordWin(winner.Id);
                    _logger.LogInfo("Game", $"Player {winner.Name} won in room {room.Id}");
                }

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

                var explosion = _elementFactory.CreateExplosion(x, y);
                explosions.Add(explosion);

                var hitPlayer = room.Players.FirstOrDefault(p => p.X == x && p.Y == y && p.IsAlive);
                if (hitPlayer != null)
                {
                    hitPlayer.IsAlive = false;

                    var killer = room.Players.FirstOrDefault(p => p.Id == bomb.PlayerId);
                    if (killer != null && killer.Id != hitPlayer.Id)
                    {
                        _statistics.RecordKill(killer.Id);
                    }

                    _logger.LogInfo("Game", $"Player {hitPlayer.Name} was eliminated");
                }

                if (cellType == CellType.DestructibleWall)
                {
                    room.Board.Grid[y][x] = (int)CellType.Empty;

                    if (Random.Shared.NextDouble() < _config.PowerUpDropChance)
                    {
                        var powerUpType = (PowerUpType)Random.Shared.Next(0, 3);
                        var powerUp = _elementFactory.CreatePowerUp(x, y, powerUpType);
                        room.Board.PowerUps.Add(powerUp);

                        _logger.LogDebug("PowerUp", $"{powerUpType} spawned at ({x}, {y})");
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