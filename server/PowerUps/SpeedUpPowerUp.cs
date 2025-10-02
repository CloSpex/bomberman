using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.PowerUps;

public class SpeedUpPowerUp : IPowerUpEffect
{
    public void ApplyEffect(Player player)
    {
        player.MovementStrategy = new SpeedBoostMovementStrategy();
    }

    public string GetDescription() => "Increases movement speed";
}