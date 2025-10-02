using BombermanGame.Models;

namespace BombermanGame.Events;

public class PlayerJoinedEvent : IGameEvent
{
    public string RoomId { get; set; } = "";
    public Player Player { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.Now;
}