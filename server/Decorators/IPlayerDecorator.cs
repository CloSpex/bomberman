using BombermanGame.Models;

namespace BombermanGame.Decorators;

public interface IPlayerDecorator
{
    int GetBombCount();
    int GetBombRange();
    int GetMovementSpeed();
    void ApplyPowerUp(PowerUpType type);
    Player GetBasePlayer();
}