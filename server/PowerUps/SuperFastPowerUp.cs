using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.PowerUps;

public class SuperFastPowerUp : IPowerUpEffect
{
    public void ApplyEffect(Player player)
    {
        MovementCooldownTracker.AddSpeedBoost(player.Id);

        if (player.MovementStrategy is not SuperFastMovementStrategy)
        {
            player.MovementStrategy = new SuperFastMovementStrategy();
        }
    }

    public string GetDescription()
    {
        var boostReduction = 20;
        return $"Decreases movement delay by {boostReduction}ms (stackable)";
    }
}