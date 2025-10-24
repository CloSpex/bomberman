using BombermanGame.Models;

namespace BombermanGame.Factories.FactoryMethod;

public abstract class BombFactory
{
    public abstract Bomb CreateBomb(int x, int y, string playerId, int range);

    public Bomb CreateAndLogBomb(int x, int y, string playerId, int range)
    {
        var bomb = CreateBomb(x, y, playerId, range);
        Console.WriteLine($"[Factory Method] Created {bomb.GetType().Name} at ({x}, {y})");
        return bomb;
    }
}

public class StandardBombFactory : BombFactory
{
    public override Bomb CreateBomb(int x, int y, string playerId, int range)
    {
        return new Bomb
        {
            X = x,
            Y = y,
            PlayerId = playerId,
            Range = range
        };
    }
}

public class EnhancedBombFactory : BombFactory
{
    public override Bomb CreateBomb(int x, int y, string playerId, int range)
    {
        return new Bomb
        {
            X = x,
            Y = y,
            PlayerId = playerId,
            Range = range + 1
        };
    }
}