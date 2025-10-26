using BombermanGame.Hubs;
using BombermanGame.Singletons;
using Microsoft.AspNetCore.SignalR;

namespace BombermanGame.Events
{

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

