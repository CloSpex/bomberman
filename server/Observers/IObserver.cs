namespace BombermanGame.Events
{
    public interface IObserver
    {
        void Update(ISubject subject);
    }
}