using BombermanGame.Models;

namespace BombermanGame.Decorators;

public class SpeedDecorator : IPlayerDecorator
{
    private readonly IPlayerDecorator _decorated;
    private readonly int _additionalSpeed;

    public SpeedDecorator(IPlayerDecorator decorated, int additionalSpeed)
    {
        _decorated = decorated;
        _additionalSpeed = additionalSpeed;
    }

    public int GetBombCount() => _decorated.GetBombCount();
    public int GetBombRange() => _decorated.GetBombRange();
    public int GetMovementSpeed() => _decorated.GetMovementSpeed() + _additionalSpeed;
    public void ApplyPowerUp(PowerUpType type) => _decorated.ApplyPowerUp(type);
}