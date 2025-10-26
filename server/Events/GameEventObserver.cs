using BombermanGame.Hubs;
using BombermanGame.Singletons;
using Microsoft.AspNetCore.SignalR;

namespace BombermanGame.Events
{
    // ========== OBSERVER PATTERN INTERFACES ==========

    // Observer interface - defines how observers receive notifications
    public interface IGameObserver
    {
        void OnGameEvent(IGameEvent gameEvent);
    }

    // Subject interface - defines subscription management
    public interface IGameSubject
    {
        void Attach(IGameObserver observer);
        void Detach(IGameObserver observer);
        void Notify(IGameEvent gameEvent);
    }

    // ========== CONCRETE SUBJECT ==========

    // Concrete Subject - manages observers and sends notifications
    public class GameEventSubject : IGameSubject
    {
        private readonly List<IGameObserver> _observers = new List<IGameObserver>();
        private readonly object _lock = new object();

        public void Attach(IGameObserver observer)
        {
            lock (_lock)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                    Console.WriteLine($"GameEventSubject: Attached observer {observer.GetType().Name}");
                }
            }
        }

        public void Detach(IGameObserver observer)
        {
            lock (_lock)
            {
                if (_observers.Remove(observer))
                {
                    Console.WriteLine($"GameEventSubject: Detached observer {observer.GetType().Name}");
                }
            }
        }

        public void Notify(IGameEvent gameEvent)
        {
            List<IGameObserver> observersCopy;
            lock (_lock)
            {
                observersCopy = new List<IGameObserver>(_observers);
            }

            foreach (var observer in observersCopy)
            {
                try
                {
                    observer.OnGameEvent(gameEvent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GameEventSubject: Error notifying observer - {ex.Message}");
                }
            }
        }
    }

    // ========== CONCRETE OBSERVERS ==========

    // Concrete Observer - Game Event Logger
    public class GameEventLoggerObserver : IGameObserver
    {
        private readonly GameLogger _logger;

        public GameEventLoggerObserver(GameLogger logger)
        {
            _logger = logger;
        }

        public void OnGameEvent(IGameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case PlayerJoinedEvent pje:
                    _logger.LogInfo("Room", $"Player {pje.Player.Name} joined room {pje.RoomId}");
                    Console.WriteLine($"Player {pje.Player.Name} joined room {pje.RoomId}");
                    break;

                case PlayerMovedEvent pme:
                    _logger.LogDebug("Player", $"Player {pme.Player.Name} moved to ({pme.Player.X}, {pme.Player.Y})");
                    break;

                case GameStartedEvent gse:
                    _logger.LogInfo("Game", $"Game started in room {gse.RoomId}");
                    Console.WriteLine($"Game started in room {gse.RoomId}");
                    break;

                case BombPlacedEvent bpe:
                    _logger.LogDebug("Bomb", $"Bomb placed at ({bpe.Bomb.X}, {bpe.Bomb.Y}) by player {bpe.Bomb.PlayerId}");
                    break;

                case BombExplodedEvent bee:
                    _logger.LogInfo("Explosion", $"Bomb exploded in room {bee.RoomId} creating {bee.Explosions.Count} explosions");
                    Console.WriteLine($"Bomb exploded in room {bee.RoomId} creating {bee.Explosions.Count} explosions");
                    break;
            }
        }
    }

    // Concrete Observer - Statistics Tracker
    public class GameStatisticsObserver : IGameObserver
    {
        private readonly GameStatistics _statistics;
        private readonly GameLogger _logger;

        public GameStatisticsObserver(GameStatistics statistics, GameLogger logger)
        {
            _statistics = statistics;
            _logger = logger;
        }

        public void OnGameEvent(IGameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case PlayerJoinedEvent pje:
                    _statistics.RecordGamePlayed(pje.Player.Id);
                    break;

                case BombPlacedEvent bpe:
                    _statistics.RecordBombPlaced(bpe.Bomb.PlayerId);
                    break;

                case BombExplodedEvent bee:
                    _statistics.RecordBombExploded();
                    break;
            }
        }
    }

    // Concrete Observer - Achievement System
    public class AchievementObserver : IGameObserver
    {
        private readonly Dictionary<string, int> _playerBombCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _playerMoveCounts = new Dictionary<string, int>();
        private readonly GameLogger _logger;

        public AchievementObserver(GameLogger logger)
        {
            _logger = logger;
        }

        public void OnGameEvent(IGameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case BombPlacedEvent bpe:
                    if (!_playerBombCounts.ContainsKey(bpe.Bomb.PlayerId))
                        _playerBombCounts[bpe.Bomb.PlayerId] = 0;

                    _playerBombCounts[bpe.Bomb.PlayerId]++;

                    if (_playerBombCounts[bpe.Bomb.PlayerId] == 10)
                    {
                        _logger.LogInfo("Achievement", $"Player {bpe.Bomb.PlayerId} unlocked 'Bomber' - Placed 10 bombs!");
                    }
                    else if (_playerBombCounts[bpe.Bomb.PlayerId] == 50)
                    {
                        _logger.LogInfo("Achievement", $"Player {bpe.Bomb.PlayerId} unlocked 'Demolition Expert' - Placed 50 bombs!");
                    }
                    else if (_playerBombCounts[bpe.Bomb.PlayerId] == 100)
                    {
                        _logger.LogInfo("Achievement", $"Player {bpe.Bomb.PlayerId} unlocked 'Master Bomber' - Placed 100 bombs!");
                    }
                    break;

                case PlayerMovedEvent pme:
                    if (!_playerMoveCounts.ContainsKey(pme.Player.Id))
                        _playerMoveCounts[pme.Player.Id] = 0;

                    _playerMoveCounts[pme.Player.Id]++;

                    if (_playerMoveCounts[pme.Player.Id] == 100)
                    {
                        _logger.LogInfo("Achievement", $"Player {pme.Player.Name} unlocked 'Marathon Runner' - Made 100 moves!");
                    }
                    else if (_playerMoveCounts[pme.Player.Id] == 500)
                    {
                        _logger.LogInfo("Achievement", $"Player {pme.Player.Name} unlocked 'Speed Demon' - Made 500 moves!");
                    }
                    break;

                case GameStartedEvent gse:
                    // Reset achievements for new game if needed
                    break;
            }
        }
    }

    // Concrete Observer - SignalR Notification System
    public class SignalRNotificationObserver : IGameObserver
    {
        private readonly IHubContext<GameHub> _hubContext;

        public SignalRNotificationObserver(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void OnGameEvent(IGameEvent gameEvent)
        {
            var roomId = gameEvent.RoomId;

            switch (gameEvent)
            {
                case PlayerJoinedEvent pje:
                    _hubContext.Clients.Group(roomId).SendAsync("PlayerJoined", new
                    {
                        pje.Player.Id,
                        pje.Player.Name,
                        pje.Player.X,
                        pje.Player.Y
                    });
                    break;

                case PlayerMovedEvent pme:
                    _hubContext.Clients.Group(roomId).SendAsync("PlayerMoved", new
                    {
                        pme.Player.Id,
                        pme.Player.X,
                        pme.Player.Y
                    });
                    break;

                case GameStartedEvent gse:
                    _hubContext.Clients.Group(roomId).SendAsync("GameStarted", new
                    {
                        gse.RoomId,
                        gse.Timestamp
                    });
                    break;

                case BombPlacedEvent bpe:
                    _hubContext.Clients.Group(roomId).SendAsync("BombPlaced", new
                    {
                        bpe.Bomb.X,
                        bpe.Bomb.Y,
                        bpe.Bomb.PlayerId,
                        bpe.Bomb.Range
                    });
                    break;

                case BombExplodedEvent bee:
                    _hubContext.Clients.Group(roomId).SendAsync("BombExploded", new
                    {
                        bee.Bomb.X,
                        bee.Bomb.Y,
                        ExplosionCount = bee.Explosions.Count,
                        Explosions = bee.Explosions.Select(e => new { e.X, e.Y })
                    });
                    break;
            }
        }
    }
}

