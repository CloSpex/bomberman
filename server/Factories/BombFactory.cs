using BombermanGame.Models;

namespace BombermanGame.Factories.FactoryMethod;

public abstract class BombFactory
{
    public Bomb CreateAndConfigureBomb(int x, int y, string playerId, int range)
    {
        var bomb = CreateBomb(x, y, playerId, range);
        LogBombCreation(bomb);
        return bomb;
    }

    protected abstract Bomb CreateBomb(int x, int y, string playerId, int range);

    protected virtual void LogBombCreation(Bomb bomb)
    {
        Console.WriteLine($"[{GetType().Name}] Created bomb at ({bomb.X}, {bomb.Y}) with range {bomb.Range}");
    }
}

public class StandardBombFactory : BombFactory
{
    protected override Bomb CreateBomb(int x, int y, string playerId, int range)
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
    protected override Bomb CreateBomb(int x, int y, string playerId, int range)
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
