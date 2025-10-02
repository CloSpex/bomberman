namespace BombermanGame.Models;

public class Explosion
{
    public int X { get; set; }
    public int Y { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}