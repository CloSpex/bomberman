using BombermanGame.Models;

namespace BombermanGame.PowerUps;

public class PowerUpFactory
{
    public static IPowerUpEffect CreatePowerUp(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.BombUp => new BombUpPowerUp(),
            PowerUpType.RangeUp => new RangeUpPowerUp(),
            PowerUpType.SpeedUp => new SpeedUpPowerUp(),
            PowerUpType.SuperFast => new SuperFastPowerUp(),
            _ => throw new ArgumentException("Invalid power-up type")
        };
    }
}