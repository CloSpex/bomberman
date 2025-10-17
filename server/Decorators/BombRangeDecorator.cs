using BombermanGame.Models;

namespace BombermanGame.Decorators;


public class BombRangeDecorator : IPlayerDecorator
{
    private readonly IPlayerDecorator _decorated;
    private readonly int _additionalRange;

    public BombRangeDecorator(IPlayerDecorator decorated, int additionalRange)
    {
        _decorated = decorated;
        _additionalRange = additionalRange;
    }

    public int GetBombCount() => _decorated.GetBombCount();
    public int GetBombRange() => _decorated.GetBombRange() + _additionalRange;
    public int GetMovementSpeed() => _decorated.GetMovementSpeed();
    public void ApplyPowerUp(PowerUpType type) => _decorated.ApplyPowerUp(type);
    public Player GetBasePlayer() => _decorated.GetBasePlayer();
}