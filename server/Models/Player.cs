using BombermanGame.Singletons;
using BombermanGame.Strategies;

namespace BombermanGame.Models;

public class Player : ICloneable
{
    private readonly GameConfiguration _config = GameConfiguration.Instance;
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int X { get; set; } = 1;
    public int Y { get; set; } = 1;
    public bool IsAlive { get; set; } = true;
    public int BombCount { get; set; }
    public int BombRange { get; set; }
    public string Color { get; set; } = "#ff0000";

    private IPlayerMovementStrategy _movementStrategy = new NormalMovementStrategy();
    public IPlayerMovementStrategy MovementStrategy
    {
        get => _movementStrategy;
        set
        {
            _movementStrategy = value;
            UpdateSpeed();
        }
    }

    public double Speed { get; private set; }

    public Player()
    {
        BombCount = _config.DefaultBombCount;
        BombRange = _config.DefaultBombRange;
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        var baseCooldown = MovementStrategy.GetBaseMovementCooldownMs();
        var effectiveCooldown = MovementCooldownTracker.GetEffectiveCooldown(Id, baseCooldown);
        Speed = Math.Round(100.0 / effectiveCooldown, 2);
    }

    public Player Clone()
    {
        IPlayerMovementStrategy clonedStrategy = this.MovementStrategy switch
        {
            NormalMovementStrategy => new NormalMovementStrategy(),
            SpeedBoostMovementStrategy => new SpeedBoostMovementStrategy(),
            SlowMovementStrategy => new SlowMovementStrategy(),
            _ => new NormalMovementStrategy()
        };

        return new Player
        {
            Id = this.Id,
            Name = this.Name,
            X = this.X,
            Y = this.Y,
            IsAlive = this.IsAlive,
            BombCount = this.BombCount,
            BombRange = this.BombRange,
            Color = this.Color,
            MovementStrategy = clonedStrategy
        };
    }

    object ICloneable.Clone() => Clone();
}