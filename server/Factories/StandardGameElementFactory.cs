using BombermanGame.Models;

namespace BombermanGame.Factories;

public interface IGameElementFactory
{
    Bomb CreateBomb(int x, int y, string playerId, int range);
    PowerUp CreatePowerUp(int x, int y, PowerUpType type);
    Explosion CreateExplosion(int x, int y);
}

public class StandardGameElementFactory : IGameElementFactory
{
    public Bomb CreateBomb(int x, int y, string playerId, int range)
    {
        return new Bomb
        {
            X = x,
            Y = y,
            PlayerId = playerId,
            Range = range
        };
    }

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
}

public class EnhancedGameElementFactory : IGameElementFactory
{
    public Bomb CreateBomb(int x, int y, string playerId, int range)
    {
        return new Bomb
        {
            X = x,
            Y = y,
            PlayerId = playerId,
            Range = range + 1
        };
    }

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
}