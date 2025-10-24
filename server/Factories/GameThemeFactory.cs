using BombermanGame.Models;

namespace BombermanGame.Factories.AbstractFactory;

public interface IGameThemeFactory
{
    PowerUp CreatePowerUp(int x, int y, PowerUpType type);
    Explosion CreateExplosion(int x, int y);
    string GetThemeName();
    string GetThemeColor();
}

public class ClassicThemeFactory : IGameThemeFactory
{

    public PowerUp CreatePowerUp(int x, int y, PowerUpType type)
    {
        return new PowerUp
        {
            X = x,
            Y = y,
            Type = type
        };
    }

    public Explosion CreateExplosion(int x, int y)
    {
        return new Explosion
        {
            X = x,
            Y = y
        };
    }

    public string GetThemeName() => "Classic";
    public string GetThemeColor() => "#FF6B00";
}

public class NeonThemeFactory : IGameThemeFactory
{

    public PowerUp CreatePowerUp(int x, int y, PowerUpType type)
    {
        return new PowerUp
        {
            X = x,
            Y = y,
            Type = type
        };
    }

    public Explosion CreateExplosion(int x, int y)
    {
        return new Explosion
        {
            X = x,
            Y = y
        };
    }

    public string GetThemeName() => "Neon";
    public string GetThemeColor() => "#00FFFF";
}
