using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.PowerUps;

public class SpeedUpPowerUp : IPowerUpEffect
{
    public void ApplyEffect(Player player)
    {
        MovementCooldownTracker.AddSpeedBoost(player.Id);

        if (player.MovementStrategy is not SpeedBoostMovementStrategy)
        {
            player.MovementStrategy = new SpeedBoostMovementStrategy();
        }
    }

    public string GetDescription()
    {
        var boostReduction = 20;
        return $"Decreases movement delay by {boostReduction}ms (stackable)";
    }
}