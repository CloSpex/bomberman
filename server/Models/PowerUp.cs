namespace BombermanGame.Models;

public class PowerUp : ICloneable
{
    public int X { get; set; }
    public int Y { get; set; }
    public PowerUpType Type { get; set; }

    public PowerUp Clone()
    {
        return new PowerUp
        {
            X = this.X,
            Y = this.Y,
            Type = this.Type
        };
    }

    object ICloneable.Clone() => Clone();
}