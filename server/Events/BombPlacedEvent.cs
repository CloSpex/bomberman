using BombermanGame.Models;

namespace BombermanGame.Events;

public class BombPlacedEvent : IGameEvent
{
    public string RoomId { get; set; } = "";
    public Bomb Bomb { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.Now;
}