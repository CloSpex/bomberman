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
using BombermanGame.PowerUps;
using BombermanGame.Commands;

namespace BombermanGame.Services;

public class GameService : IGameService
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly ConcurrentDictionary<string, MovePlayerCommand> _lastMoveCommands = new();
    private readonly ConcurrentDictionary<string, PlaceBombCommand> _lastBombCommands = new();
    private readonly Dictionary<string, PlayerDecoratorManager> _roomDecorators = new();
    private readonly Dictionary<string, IGameRenderer> _roomRenderers = new();
    private readonly Dictionary<string, IGameModeFactory> _roomModeFactories = new();
    private readonly Timer _gameTimer;
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
        IHubContext<GameHub> hubContext,
        PrototypeManager prototypeManager,
        IGameDataService gameDataService,
        IPlayerBuilder playerBuilder,
        IGameRoomBuilder gameRoomBuilder)
    {
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

    public GameRoom CreateRoom(string roomId, string gameMode = "standard")
    {
        var modeFactory = CreateGameModeFactory(gameMode);
        _roomModeFactories[roomId] = modeFactory;

        var room = _gameRoomBuilder
            .WithId(roomId)
            .WithState(GameState.Waiting)
            .WithBoard(new GameBoard())
            .Build();

        _rooms.TryAdd(roomId, room);
        _roomDecorators[roomId] = new PlayerDecoratorManager();
        _roomRenderers[roomId] = new JsonGameRenderer();

        var modeName = modeFactory.GetModeName();
        var modeDescription = modeFactory.GetModeDescription();
        var dropRate = modeFactory.GetPowerUpDropRate();

        _logger.LogInfo("Room", $"Created room {roomId} with {modeName} mode - {modeDescription}");
        _logger.LogInfo("Factory", $"Mode settings: PowerUp drop rate = {dropRate:P0}");

        return room;
    }

    private IGameModeFactory CreateGameModeFactory(string gameMode)
    {
        var factory = (IGameModeFactory)(gameMode.ToLower() switch
        {
            "standard" => new StandardModeFactory(),
            "chaos" => new ChaosModeFactory(),
            "speed" => new SpeedModeFactory(),
            _ => new StandardModeFactory()
        });

        _logger.LogInfo("Factory", $"Created {factory.GetModeName()} factory with {factory.GetPowerUpDropRate():P0} drop rate");
        return factory;
    }

    public void SetRoomGameMode(string roomId, string gameMode)
    {
        if (!_roomModeFactories.ContainsKey(roomId))
        {
            _logger.LogWarning("Room", $"Cannot set game mode - room {roomId} not found");
            return;
        }

        var oldFactory = _roomModeFactories[roomId];
        var oldMode = oldFactory.GetModeName();
        var oldDropRate = oldFactory.GetPowerUpDropRate();

        _roomModeFactories[roomId] = CreateGameModeFactory(gameMode);

        var newMode = _roomModeFactories[roomId].GetModeName();
        var newDropRate = _roomModeFactories[roomId].GetPowerUpDropRate();

        _logger.LogInfo("Room", $"Changed room {roomId} game mode from {oldMode} (drop: {oldDropRate:P0}) to {newMode} (drop: {newDropRate:P0})");
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
            _logger.LogWarning("Room", $"Player {player.Name} cannot join room {roomId} in state {room.State}");
            return false;
        }

        var playerCount = room.Players.Count;
        if (playerCount >= _config.MaxPlayersPerRoom)
        {
            _logger.LogWarning("Room", $"Room {roomId} is full ({playerCount}/{_config.MaxPlayersPerRoom})");
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

        if (_roomModeFactories.TryGetValue(roomId, out var modeFactory))
        {
            modeFactory.ApplyModeEffectsToPlayer(newPlayer);
            var gameMode = modeFactory.GetModeName();
            _logger.LogInfo("GameMode", $"Applied {gameMode} mode effects to player {newPlayer.Name}");
            _logger.LogInfo("GameMode", $"Player stats - Bombs: {newPlayer.BombCount}, Range: {newPlayer.BombRange}, Speed: {newPlayer.Speed}");
        }

        room.Players.Add(newPlayer);
        _roomDecorators[roomId].RegisterPlayer(newPlayer);

        _gameEventSubject.Notify(new PlayerJoinedEvent
        {
            RoomId = roomId,
            Player = newPlayer
        });

        await SaveRoomDataAsync(roomId, room);

        var currentGameMode = GetRoomGameMode(roomId);
        _logger.LogInfo("Room", $"Player {newPlayer.Name} ({prototypeKey} role) joined room {roomId} ({currentGameMode} mode)");

        return true;
    }

    public async Task StartGameAsync(string roomId)
    {
        var room = GetRoom(roomId);

        if (room != null && room.StateHandler.CanStartGame(room))
        {
            room.State = GameState.Playing;

            var prototypeBoard = _prototypeManager.CreateBoardFromPrototype("standard");
            room.Board = prototypeBoard ?? new GameBoard();

            room.UpdateStateHandler();

            _gameEventSubject.Notify(new GameStartedEvent
            {
                RoomId = roomId
            });

            await SaveRoomDataAsync(roomId, room);

            var modeName = GetRoomGameMode(roomId);
            var modeDescription = GetRoomGameModeDescription(roomId);
            var dropRate = GetRoomPowerUpDropRate(roomId);

            _logger.LogInfo("Game", $"Game started in room {roomId} with {room.Players.Count} players");
            _logger.LogInfo("Game", $"Mode: {modeName} - {modeDescription}");
            _logger.LogInfo("Game", $"PowerUp drop rate: {dropRate:P0}");

            foreach (var p in room.Players)
            {
                _logger.LogInfo("Game", $"Player {p.Name}: Bombs={p.BombCount}, Range={p.BombRange}, Speed={p.Speed}");
            }
        }
        else if (room != null)
        {
            _logger.LogWarning("Game", $"Cannot start game in room {roomId} - State: {room.State}, Players: {room.Players.Count}");
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
            return false;

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

    public void AddPreviousMoveCommand(string playerId, MovePlayerCommand command)
    {
        _lastMoveCommands[playerId] = command;
    }

    public void AddPreviousBombCommand(string playerId, PlaceBombCommand command)
    {
        _lastBombCommands[playerId] = command;
    }

    public MovePlayerCommand? GetPreviousMoveCommand(string playerId)
    {
        _lastMoveCommands.TryGetValue(playerId, out var command);
        return command;
    }

    public PlaceBombCommand? GetPreviousBombCommand(string playerId)
    {
        _lastBombCommands.TryGetValue(playerId, out var command);
        return command;
    }

    public void RemovePreviousMoveCommand(string playerId)
    {
        _lastMoveCommands.TryRemove(playerId, out _);
    }

    public void RemovePreviousBombCommand(string playerId)
    {
        _lastBombCommands.TryRemove(playerId, out _);
    }

    private void ApplyPowerUpToPlayer(string roomId, string playerId, Player player, PowerUp powerUp)
    {
        var powerUpEffect = PowerUpEffectFactory.CreateEffect(powerUp.Type);
        powerUpEffect.ApplyEffect(player);

        if (_roomDecorators.TryGetValue(roomId, out var decoratorManager))
        {
            switch (powerUp.Type)
            {
                case PowerUpType.BombUp:
                    decoratorManager.ApplyBombUpgrade(playerId);
                    _logger.LogInfo("PowerUp", $"Player {player.Name} collected BombUp - Now has {player.BombCount} bombs");
                    break;
                case PowerUpType.RangeUp:
                    decoratorManager.ApplyRangeUpgrade(playerId);
                    _logger.LogInfo("PowerUp", $"Player {player.Name} collected RangeUp - Now has {player.BombRange} range");
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

                    _logger.LogInfo("PowerUp", $"Player {player.Name} collected SpeedUp (x{boostCount}) - Delay: {effectiveCooldown}ms, Speed: {player.Speed}");
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

        _logger.LogInfo("Prototype", $"Generated {previews.Count} role previews");
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
        {
            _logger.LogDebug("Bomb", $"Player {player.Name} already has max bombs ({playerBombs}/{effectiveBombCount})");
            return false;
        }

        if (room.Board.Bombs.Any(b => b.X == player.X && b.Y == player.Y))
        {
            _logger.LogDebug("Bomb", $"Bomb already exists at position ({player.X}, {player.Y})");
            return false;
        }

        var modeFactory = _roomModeFactories.GetValueOrDefault(roomId, new StandardModeFactory());
        var bombFactory = modeFactory.GetBombFactory();

        var bomb = bombFactory.CreateAndConfigureBomb(player.X, player.Y, playerId, effectiveBombRange);
        room.Board.Bombs.Add(bomb);

        _gameEventSubject.Notify(new BombPlacedEvent
        {
            RoomId = roomId,
            Bomb = bomb
        });

        var gameMode = GetRoomGameMode(roomId);
        var factoryName = bombFactory.GetType().Name;
        _logger.LogInfo("Bomb", $"Player {player.Name} placed bomb in {gameMode} mode using {factoryName}");
        _logger.LogInfo("Bomb", $"Bomb at ({bomb.X}, {bomb.Y}) with range {bomb.Range}");

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

                var gameMode = GetRoomGameMode(room.Id);
                _logger.LogInfo("Explosion", $"Bomb exploded in {gameMode} mode at ({bomb.X}, {bomb.Y})");
                _logger.LogInfo("Explosion", $"Created {explosions.Count} explosion cells");

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
                    var gameMode = GetRoomGameMode(room.Id);
                    _logger.LogInfo("Game", $"Player {winner.Name} won in room {room.Id} ({gameMode} mode)");
                }
                else
                {
                    _logger.LogInfo("Game", $"Game ended in room {room.Id} with no survivors (Draw)");
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

        var modeFactory = _roomModeFactories.GetValueOrDefault(room.Id, new StandardModeFactory());
        var powerUpDropRate = modeFactory.GetPowerUpDropRate();

        var gameMode = modeFactory.GetModeName();
        _logger.LogDebug("Explosion", $"Exploding bomb in {gameMode} mode with {powerUpDropRate:P0} drop rate");

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

                var explosion = modeFactory.CreateExplosion(x, y);
                explosions.Add(explosion);

                var hitPlayer = room.Players.FirstOrDefault(p => p.X == x && p.Y == y && p.IsAlive);
                if (hitPlayer != null)
                {
                    hitPlayer.IsAlive = false;
                    _logger.LogInfo("Game", $"Player {hitPlayer.Name} was eliminated in {gameMode} mode at ({x}, {y})");
                }

                if (cellType == CellType.DestructibleWall)
                {
                    room.Board.Grid[y][x] = (int)CellType.Empty;

                    if (Random.Shared.NextDouble() < powerUpDropRate)
                    {
                        var powerUpType = (PowerUpType)Random.Shared.Next(0, 4);

                        var powerUp = modeFactory.CreatePowerUp(x, y, powerUpType);
                        room.Board.PowerUps.Add(powerUp);

                        _logger.LogInfo("PowerUp", $"Dropped {powerUpType} in {gameMode} mode at ({x}, {y}) (rate: {powerUpDropRate:P0})");
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
        else
        {
            _logger.LogWarning("Bridge", $"Cannot set renderer - room {roomId} not found");
        }
    }

    public string GetRoomGameMode(string roomId)
    {
        if (_roomModeFactories.TryGetValue(roomId, out var factory))
        {
            return factory.GetModeName();
        }

        _logger.LogWarning("Factory", $"No game mode factory found for room {roomId}, defaulting to Standard");
        return "Standard";
    }

    public string GetRoomGameModeDescription(string roomId)
    {
        if (_roomModeFactories.TryGetValue(roomId, out var factory))
        {
            return factory.GetModeDescription();
        }

        _logger.LogWarning("Factory", $"No game mode factory found for room {roomId}");
        return "Balanced gameplay with standard mechanics";
    }

    private double GetRoomPowerUpDropRate(string roomId)
    {
        if (_roomModeFactories.TryGetValue(roomId, out var factory))
        {
            return factory.GetPowerUpDropRate();
        }
        return 0.3;
    }
}