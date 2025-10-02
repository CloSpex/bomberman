namespace BombermanGame.Events;

public class GameStartedEvent : IGameEvent
{
    public string RoomId { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}