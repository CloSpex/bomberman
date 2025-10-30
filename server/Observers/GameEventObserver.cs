using BombermanGame.Hubs;
using BombermanGame.Singletons;
using Microsoft.AspNetCore.SignalR;

namespace BombermanGame.Events;

public class GameEventLoggerObserver : IObserver
{
    private readonly GameLogger _logger;

    public GameEventLoggerObserver(GameLogger logger)
    {
        _logger = logger;
    }

    public void Update(ISubject subject)
    {
        if (subject is GameEventSubject eventSubject && eventSubject.LastEvent != null)
        {
            var gameEvent = eventSubject.LastEvent;

            switch (gameEvent)
            {
                case PlayerJoinedEvent pje:
                    _logger.LogInfo("Observer", $"[LOGGER] Player {pje.Player.Name} joined room {pje.RoomId}");
                    break;

                case PlayerMovedEvent pme:
                    _logger.LogDebug("Observer", $"[LOGGER] Player {pme.Player.Name} moved to ({pme.Player.X}, {pme.Player.Y})");
                    break;

                case GameStartedEvent gse:
                    _logger.LogInfo("Observer", $"[LOGGER] Game started in room {gse.RoomId}");
                    break;

                case BombPlacedEvent bpe:
                    _logger.LogDebug("Observer", $"[LOGGER] Bomb placed at ({bpe.Bomb.X}, {bpe.Bomb.Y})");
                    break;

                case BombExplodedEvent bee:
                    _logger.LogInfo("Observer", $"[LOGGER] Bomb exploded creating {bee.Explosions.Count} explosions");
                    break;
            }
        }
    }
}

public class SignalRNotificationObserver : IObserver
{
    private readonly IHubContext<GameHub> _hubContext;

    public SignalRNotificationObserver(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public void Update(ISubject subject)
    {
        if (subject is GameEventSubject eventSubject && eventSubject.LastEvent != null)
        {
            var gameEvent = eventSubject.LastEvent;
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
