using BombermanGame.Models;

namespace BombermanGame.Decorators;


public class BombCountDecorator : IPlayerDecorator
{
    private readonly IPlayerDecorator _decorated;
    private readonly int _additionalBombs;

    public BombCountDecorator(IPlayerDecorator decorated, int additionalBombs)
    {
        _decorated = decorated;
        _additionalBombs = additionalBombs;
    }

    public int GetBombCount() => _decorated.GetBombCount() + _additionalBombs;
    public int GetBombRange() => _decorated.GetBombRange();
    public int GetMovementSpeed() => _decorated.GetMovementSpeed();
    public void ApplyPowerUp(PowerUpType type) => _decorated.ApplyPowerUp(type);
    public Player GetBasePlayer() => _decorated.GetBasePlayer();
}