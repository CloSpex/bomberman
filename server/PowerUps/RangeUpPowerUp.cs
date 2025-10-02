using BombermanGame.Models;

namespace BombermanGame.PowerUps;

public class RangeUpPowerUp : IPowerUpEffect
{
    public void ApplyEffect(Player player)
    {
        player.BombRange++;
    }

    public string GetDescription() => "Increases bomb range by 1";
}