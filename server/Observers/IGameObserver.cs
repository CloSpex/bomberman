namespace BombermanGame.Events
{
    public interface IGameObserver
    {
        void OnGameEvent(IGameEvent gameEvent);
    }
}