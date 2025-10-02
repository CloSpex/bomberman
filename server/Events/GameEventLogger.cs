namespace BombermanGame.Events;

public class GameEventLogger : IEventHandler<PlayerJoinedEvent>,
                              IEventHandler<GameStartedEvent>,
                              IEventHandler<BombExplodedEvent>
{
    public async Task HandleAsync(PlayerJoinedEvent gameEvent)
    {
        Console.WriteLine($"Player {gameEvent.Player.Name} joined room {gameEvent.RoomId}");
        await Task.CompletedTask;
    }

    public async Task HandleAsync(GameStartedEvent gameEvent)
    {
        Console.WriteLine($"Game started in room {gameEvent.RoomId}");
        await Task.CompletedTask;
    }

    public async Task HandleAsync(BombExplodedEvent gameEvent)
    {
        Console.WriteLine($"Bomb exploded in room {gameEvent.RoomId} creating {gameEvent.Explosions.Count} explosions");
        await Task.CompletedTask;
    }
}