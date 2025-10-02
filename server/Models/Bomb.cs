namespace BombermanGame.Models;

public class Bomb
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int X { get; set; }
    public int Y { get; set; }
    public string PlayerId { get; set; } = "";
    public DateTime PlacedAt { get; set; } = DateTime.Now;
    public int Range { get; set; } = 2;
}