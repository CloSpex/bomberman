using BombermanGame.Models;

namespace BombermanGame.Decorators;


public class BasePlayerDecorator : IPlayerDecorator
{
    protected Player _player;

    public BasePlayerDecorator(Player player)
    {
        _player = player;
    }

    public virtual int GetBombCount() => _player.BombCount;
    public virtual int GetBombRange() => _player.BombRange;
    public virtual int GetMovementSpeed() => 1;

    public virtual void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.BombUp:
                _player.BombCount++;
                break;
            case PowerUpType.RangeUp:
                _player.BombRange++;
                break;
        }
    }

    public Player GetBasePlayer() => _player;
}
