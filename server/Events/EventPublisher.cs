namespace BombermanGame.Events;

public interface IEventHandler<T> where T : IGameEvent
{
    Task HandleAsync(T gameEvent);
}

public interface IEventPublisher
{
    Task PublishAsync<T>(T gameEvent) where T : IGameEvent;
    void Subscribe<T>(IEventHandler<T> handler) where T : IGameEvent;
}

public class EventPublisher : IEventPublisher
{
    private readonly Dictionary<Type, List<object>> _handlers = new();

    public async Task PublishAsync<T>(T gameEvent) where T : IGameEvent
    {
        var eventType = typeof(T);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            var tasks = handlers.Cast<IEventHandler<T>>()
                              .Select(handler => handler.HandleAsync(gameEvent));
            await Task.WhenAll(tasks);
        }
    }

    public void Subscribe<T>(IEventHandler<T> handler) where T : IGameEvent
    {
        var eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<object>();
        }
        _handlers[eventType].Add(handler);
    }
}