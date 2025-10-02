namespace BombermanGame.Events;

public interface IGameEvent
{
    string RoomId { get; }
    DateTime Timestamp { get; }
}