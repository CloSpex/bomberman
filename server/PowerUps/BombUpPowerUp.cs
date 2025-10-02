using BombermanGame.Models;

namespace BombermanGame.PowerUps;

public class BombUpPowerUp : IPowerUpEffect
{
    public void ApplyEffect(Player player)
    {
        player.BombCount++;
    }

    public string GetDescription() => "Increases bomb count by 1";
}