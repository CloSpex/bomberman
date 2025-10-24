namespace BombermanGame.Models;

public class Explosion : ICloneable
{
    public int X { get; set; }
    public int Y { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Explosion Clone()
    {
        return new Explosion
        {
            X = this.X,
            Y = this.Y,
            CreatedAt = this.CreatedAt
        };
    }

    object ICloneable.Clone() => Clone();
}