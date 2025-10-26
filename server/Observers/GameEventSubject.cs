namespace BombermanGame.Events
{

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
}