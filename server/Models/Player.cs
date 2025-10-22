using BombermanGame.Strategies;

namespace BombermanGame.Models;

public class Player
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int X { get; set; } = 1;
    public int Y { get; set; } = 1;
    public bool IsAlive { get; set; } = true;
    public int BombCount { get; set; } = 1;
    public int BombRange { get; set; } = 2;
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

    private void UpdateSpeed()
    {
        var baseCooldown = MovementStrategy.GetBaseMovementCooldownMs();
        var effectiveCooldown = MovementCooldownTracker.GetEffectiveCooldown(Id, baseCooldown);
        Speed = Math.Round(100.0 / effectiveCooldown, 2);
    }
}