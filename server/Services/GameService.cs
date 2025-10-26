using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using BombermanGame.Models;
using BombermanGame.Events;
using BombermanGame.Factories;
using BombermanGame.Factories.FactoryMethod;
using BombermanGame.Factories.AbstractFactory;
using BombermanGame.Hubs;
using BombermanGame.Decorators;
using BombermanGame.Prototypes;
using BombermanGame.Singletons;
using BombermanGame.Strategies;
using BombermanGame.Adapters;
using BombermanGame.Bridges;
using BombermanGame.Builders;

namespace BombermanGame.Services;

public class GameService : IGameService
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly Dictionary<string, PlayerDecoratorManager> _roomDecorators = new();
    private readonly Dictionary<string, IGameRenderer> _roomRenderers = new();
    private readonly Dictionary<string, BombFactory> _roomBombFactories = new();
    private readonly Dictionary<string, IGameThemeFactory> _roomThemeFactories = new();

    private readonly Timer _gameTimer;
    private readonly IGameFactory _gameFactory;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly PrototypeManager _prototypeManager;
    private readonly IGameDataService _gameDataService;
    private readonly IPlayerBuilder _playerBuilder;
    private readonly IGameRoomBuilder _gameRoomBuilder;
    private readonly GameEventSubject _gameEventSubject;
    private readonly GameEventLoggerObserver _loggerObserver;
    private readonly SignalRNotificationObserver _notificationObserver;

    private readonly GameConfiguration _config = GameConfiguration.Instance;
    private readonly GameLogger _logger = GameLogger.Instance;

    public GameService(
        IGameFactory gameFactory,
        IHubContext<GameHub> hubContext,
        PrototypeManager prototypeManager,
        IGameDataService gameDataService,
        IPlayerBuilder playerBuilder,
        IGameRoomBuilder gameRoomBuilder)
    {
        _gameFactory = gameFactory;
        _hubContext = hubContext;
        _prototypeManager = prototypeManager;
        _gameDataService = gameDataService;
        _playerBuilder = playerBuilder;
        _gameRoomBuilder = gameRoomBuilder;

        _gameEventSubject = new GameEventSubject();

        _loggerObserver = new GameEventLoggerObserver(_logger);
        _notificationObserver = new SignalRNotificationObserver(_hubContext);

        _gameEventSubject.Attach(_loggerObserver);
        _gameEventSubject.Attach(_notificationObserver);

        _logger.LogInfo("Observer", "All observers attached successfully");

        _gameTimer = new Timer(
            UpdateGames,
            null,
            TimeSpan.FromMilliseconds(_config.GameUpdateIntervalMs),
            TimeSpan.FromMilliseconds(_config.GameUpdateIntervalMs)
        );

        InitializePrototypes();
    }

    private void InitializePrototypes()
    {
        var powerPlayer = _playerBuilder
            .WithName("Power")
            .WithPosition(1, 1)
            .WithBombCount(_config.DefaultBombCount)
            .WithBombRange(_config.DefaultBombRange + 2)
            .WithMovementStrategy(new SlowMovementStrategy())
            .WithColor("#ff0000")
            .Build();
        _prototypeManager.RegisterPlayerPrototype("power", powerPlayer);

        var speedPlayer = _playerBuilder
            .WithName("Speed")
            .WithPosition(1, 1)
            .WithBombCount(_config.DefaultBombCount)
            .WithBombRange(_config.DefaultBombRange)
            .WithMovementStrategy(new SpeedBoostMovementStrategy())
            .WithColor("#00ff00")
            .Build();
        _prototypeManager.RegisterPlayerPrototype("speed", speedPlayer);

        var bomberPlayer = _playerBuilder
            .WithName("Bomber")
            .WithPosition(1, 1)
            .WithBombCount(_config.DefaultBombCount + 1)
            .WithBombRange(_config.DefaultBombRange)
            .WithMovementStrategy(new NormalMovementStrategy())
            .WithColor("#ffff00")
            .Build();
        _prototypeManager.RegisterPlayerPrototype("bomber", bomberPlayer);

        var balancedPlayer = _playerBuilder
            .WithName("Balanced")
            .WithPosition(1, 1)
            .WithBombCount(_config.DefaultBombCount + 1)
            .WithBombRange(_config.DefaultBombRange + 1)
            .WithMovementStrategy(new NormalMovementStrategy())
            .WithColor("#0000ff")
            .Build();
        _prototypeManager.RegisterPlayerPrototype("balanced", balancedPlayer);

        var standardBoard = new GameBoard();
        _prototypeManager.RegisterBoardPrototype("standard", standardBoard);
    }

    public GameRoom CreateRoom(string roomId, string theme = "classic")
    {
        var room = _gameRoomBuilder
            .WithId(roomId)
            .WithState(GameState.Waiting)
            .WithBoard(new GameBoard())
            .Build();

        _rooms.TryAdd(roomId, room);
        _roomDecorators[roomId] = new PlayerDecoratorManager();
        _roomRenderers[roomId] = new JsonGameRenderer();
        _roomBombFactories[roomId] = new StandardBombFactory();
        _roomThemeFactories[roomId] = CreateThemeFactory(theme);

        return room;
    }

    private IGameThemeFactory CreateThemeFactory(string theme)
    {
        return theme.ToLower() switch
        {
            "neon" => new NeonThemeFactory(),
            _ => new ClassicThemeFactory()
        };
    }

    public void SetRoomBombFactory(string roomId, string factoryType)
    {
        if (!_roomBombFactories.ContainsKey(roomId))
            return;

        _roomBombFactories[roomId] = factoryType.ToLower() switch
        {
            "enhanced" => new EnhancedBombFactory(),
            _ => new StandardBombFactory()
        };
    }

    public void SetRoomTheme(string roomId, string theme)
    {
        if (!_roomThemeFactories.ContainsKey(roomId))
            return;

        _roomThemeFactories[roomId] = CreateThemeFactory(theme);
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
            _logger.LogWarning("Room", $"Player {player.Name} cannot join room {roomId}");
            return false;
        }

        var playerCount = room.Players.Count;
        if (playerCount >= _config.MaxPlayersPerRoom)
        {
            _logger.LogWarning("Room", $"Room {roomId} is full");
            return false;
        }

        string[] roles = { "power", "speed", "bomber", "balanced" };
        var roleIndex = playerCount % roles.Length;
        var prototypeKey = roles[roleIndex];

        var template = _prototypeManager.CreatePlayerFromPrototype(prototypeKey);
        Player newPlayer;

        if (template != null)
        {
            newPlayer = template;
            newPlayer.Id = player.Id;
            newPlayer.Name = player.Name;
        }
        else
        {
            newPlayer = player;
        }

        var spawn = _config.GetSpawnPosition(playerCount);
        newPlayer.X = spawn.X;
        newPlayer.Y = spawn.Y;

        room.Players.Add(newPlayer);
        _roomDecorators[roomId].RegisterPlayer(newPlayer);

        _gameEventSubject.Notify(new PlayerJoinedEvent
        {
            RoomId = roomId,
            Player = newPlayer
        });

        await SaveRoomDataAsync(roomId, room);

        _logger.LogInfo("Room", $"Player {newPlayer.Name} ({prototypeKey} role) joined room {roomId}");

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

            _roomBombFactories[roomId] = new EnhancedBombFactory();

            room.UpdateStateHandler();

            _gameEventSubject.Notify(new GameStartedEvent
            {
                RoomId = roomId
            });

            await SaveRoomDataAsync(roomId, room);

            var theme = _roomThemeFactories[roomId].GetThemeName();
            _logger.LogInfo("Game", $"Game started in room {roomId} with {room.Players.Count} players using {theme} theme");
        }
    }

    public async Task<bool> MovePlayerAsync(string roomId, string playerId, int deltaX, int deltaY)
    {
        var room = GetRoom(roomId);
        var player = room?.Players.FirstOrDefault(p => p.Id == playerId);

        if (room == null || player == null || !player.IsAlive ||
            !room.StateHandler.CanMovePlayer(room))
            return false;

        var newX = player.X + deltaX;
        var newY = player.Y + deltaY;

        if (!player.MovementStrategy.CanMove(room.Board, newX, newY))
            return false;

        if (!player.MovementStrategy.CanMoveNow(player.Id))
        {
            return false;
        }

        player.X = newX;
        player.Y = newY;

        var powerUp = room.Board.PowerUps.FirstOrDefault(p => p.X == newX && p.Y == newY);
        if (powerUp != null)
        {
            ApplyPowerUpToPlayer(roomId, playerId, player, powerUp);
            room.Board.PowerUps.Remove(powerUp);
        }

        _gameEventSubject.Notify(new PlayerMovedEvent
        {
            RoomId = roomId,
            Player = player
        });

        await SaveRoomDataAsync(roomId, room);

        return true;
    }

    private void ApplyPowerUpToPlayer(string roomId, string playerId, Player player, PowerUp powerUp)
    {
        var powerUpEffect = PowerUps.PowerUpFactory.CreatePowerUp(powerUp.Type);
        powerUpEffect.ApplyEffect(player);

        if (_roomDecorators.TryGetValue(roomId, out var decoratorManager))
        {
            switch (powerUp.Type)
            {
                case PowerUpType.BombUp:
                    decoratorManager.ApplyBombUpgrade(playerId);
                    break;
                case PowerUpType.RangeUp:
                    decoratorManager.ApplyRangeUpgrade(playerId);
                    break;
                case PowerUpType.SpeedUp:
                    decoratorManager.ApplySpeedUpgrade(playerId);
                    player.MovementStrategy = new SpeedBoostMovementStrategy();
                    MovementCooldownTracker.AddSpeedBoost(playerId);

                    var boostCount = MovementCooldownTracker.GetSpeedBoostCount(playerId);
                    var effectiveCooldown = MovementCooldownTracker.GetEffectiveCooldown(
                        playerId,
                        player.MovementStrategy.GetBaseMovementCooldownMs()
                    );

                    _logger.LogInfo("PowerUp", $"Player {player.Name} collected SpeedUp (x{boostCount}) - Delay: {effectiveCooldown}ms");
                    break;
            }
        }
    }

    public List<Player> GetPlayerRolePreviews()
    {
        string[] roles = { "power", "speed", "bomber", "balanced" };
        var previews = new List<Player>();

        foreach (var role in roles)
        {
            var preview = _prototypeManager.CreatePlayerPreview(role);
            if (preview != null)
            {
                previews.Add(preview);
            }
        }

        return previews;
    }

    public async Task<bool> PlaceBombAsync(string roomId, string playerId)
    {
        var room = GetRoom(roomId);
        var player = room?.Players.FirstOrDefault(p => p.Id == playerId);

        if (room == null || player == null || !player.IsAlive ||
            !room.StateHandler.CanPlaceBomb(room))
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

        var bombFactory = _roomBombFactories.GetValueOrDefault(roomId, new StandardBombFactory());
        var bomb = bombFactory.CreateBomb(player.X, player.Y, playerId, effectiveBombRange);
        room.Board.Bombs.Add(bomb);

        _gameEventSubject.Notify(new BombPlacedEvent
        {
            RoomId = roomId,
            Bomb = bomb
        });

        if (_roomRenderers.TryGetValue(roomId, out var renderer))
        {
            var bombElement = new BombElement(bomb, renderer);
            _logger.LogDebug("Bomb", $"Bomb placed: {bombElement.Render()}");
        }

        await SaveRoomDataAsync(roomId, room);

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
                var explosions = await ExplodeBombAsync(room, bomb);
                room.Board.Bombs.Remove(bomb);

                _gameEventSubject.Notify(new BombExplodedEvent
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
                    _logger.LogInfo("Game", $"Player {winner.Name} won in room {room.Id}");
                }

                updated = true;
            }

            if (updated)
            {
                room.LastUpdate = now;
                await SaveRoomDataAsync(room.Id, room);
                await _hubContext.Clients.Group(room.Id).SendAsync("GameUpdated", room);
            }
        }
    }

    private async Task<List<Explosion>> ExplodeBombAsync(GameRoom room, Bomb bomb)
    {
        var explosions = new List<Explosion>();
        var directions = new[] { (0, 0), (0, 1), (0, -1), (1, 0), (-1, 0) };

        var themeFactory = _roomThemeFactories.GetValueOrDefault(room.Id, new ClassicThemeFactory());

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

                var explosion = themeFactory.CreateExplosion(x, y);
                explosions.Add(explosion);

                var hitPlayer = room.Players.FirstOrDefault(p => p.X == x && p.Y == y && p.IsAlive);
                if (hitPlayer != null)
                {
                    hitPlayer.IsAlive = false;

                    _logger.LogInfo("Game", $"Player {hitPlayer.Name} was eliminated");
                }

                if (cellType == CellType.DestructibleWall)
                {
                    room.Board.Grid[y][x] = (int)CellType.Empty;

                    if (Random.Shared.NextDouble() < _config.PowerUpDropChance)
                    {
                        var powerUpType = (PowerUpType)Random.Shared.Next(0, 3);
                        var powerUp = themeFactory.CreatePowerUp(x, y, powerUpType);
                        room.Board.PowerUps.Add(powerUp);
                    }

                    break;
                }
            }
        }

        room.Board.Explosions.AddRange(explosions);
        return explosions;
    }

    private async Task SaveRoomDataAsync(string roomId, GameRoom room)
    {
        try
        {
            var roomData = new GameRoomData
            {
                RoomId = room.Id,
                State = room.State.ToString(),
                LastUpdate = room.LastUpdate,
                Players = room.Players.Select(p => new PlayerData
                {
                    Id = p.Id,
                    Name = p.Name,
                    X = p.X,
                    Y = p.Y,
                    IsAlive = p.IsAlive
                }).ToList()
            };

            await _gameDataService.SaveGameDataAsync(roomId, roomData);
        }
        catch (Exception ex)
        {
            _logger.LogError("Adapter", $"Failed to save room data: {ex.Message}");
        }
    }

    public List<GameRoom> GetRooms()
    {
        return _rooms.Values.ToList();
    }

    public IGameRenderer? GetRoomRenderer(string roomId)
    {
        _roomRenderers.TryGetValue(roomId, out var renderer);
        return renderer;
    }

    public void SetRoomRenderer(string roomId, IGameRenderer renderer)
    {
        if (_roomRenderers.ContainsKey(roomId))
        {
            _roomRenderers[roomId] = renderer;
            _logger.LogInfo("Bridge", $"Renderer changed for room {roomId} to {renderer.GetType().Name}");
        }
    }

    public string GetRoomTheme(string roomId)
    {
        if (_roomThemeFactories.TryGetValue(roomId, out var factory))
        {
            return factory.GetThemeName();
        }
        return "Unknown";
    }

    public string GetRoomBombFactoryType(string roomId)
    {
        if (_roomBombFactories.TryGetValue(roomId, out var factory))
        {
            return factory.GetType().Name;
        }
        return "Unknown";
    }


}

