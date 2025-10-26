using BombermanGame.Models;

namespace BombermanGame.PowerUps;

public static class PowerUpEffectFactory
{
    public static IPowerUpEffect CreateEffect(PowerUpType type)
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