namespace BombermanGame.Models;

public class Bomb : ICloneable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int X { get; set; }
    public int Y { get; set; }
    public string PlayerId { get; set; } = "";
    public DateTime PlacedAt { get; set; } = DateTime.Now;
    public int Range { get; set; } = 2;
    public Bomb Clone()
    {
        return new Bomb
        {
            Id = this.Id,
            X = this.X,
            Y = this.Y,
            PlayerId = this.PlayerId,
            PlacedAt = this.PlacedAt,
            Range = this.Range
        };
    }

    object ICloneable.Clone() => Clone();
}