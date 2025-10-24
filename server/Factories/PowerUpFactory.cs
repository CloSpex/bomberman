using BombermanGame.Models;

namespace BombermanGame.Factories.FactoryMethod;

public abstract class PowerUpFactory
{
    public abstract PowerUp CreatePowerUp(int x, int y, PowerUpType type);


    protected virtual double GetRarityModifier()
    {
        return 1.0;
    }
}

public class StandardPowerUpFactory : PowerUpFactory
{
    public override PowerUp CreatePowerUp(int x, int y, PowerUpType type)
    {
        return new PowerUp
        {
            X = x,
            Y = y,
            Type = type
        };
    }
}

public class RarePowerUpFactory : PowerUpFactory
{
    public override PowerUp CreatePowerUp(int x, int y, PowerUpType type)
    {
        return new PowerUp
        {
            X = x,
            Y = y,
            Type = type
        };
    }

    protected override double GetRarityModifier()
    {
        return 0.5;
    }
}