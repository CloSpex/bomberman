namespace BombermanGame.Events
{

    public class GameEventSubject : ISubject
    {
        private readonly List<IObserver> _observers = new();
        private readonly object _lock = new();
        private IGameEvent? _lastEvent;

        public IGameEvent? LastEvent => _lastEvent;

        public void Attach(IObserver observer)
        {
            lock (_lock)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }
        }

        public void Detach(IObserver observer)
        {
            lock (_lock)
            {
                _observers.Remove(observer);
            }
        }

        public void Notify()
        {
            List<IObserver> observersCopy;
            lock (_lock)
            {
                observersCopy = new List<IObserver>(_observers);
            }

            foreach (var observer in observersCopy)
            {
                try
                {
                    observer.Update(this);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error notifying observer: {ex.Message}");
                }
            }
        }

        public void NotifyEvent(IGameEvent gameEvent)
        {
            _lastEvent = gameEvent;
            Notify();
        }
    }
}