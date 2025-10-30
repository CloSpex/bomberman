using BombermanGame.Models;

namespace BombermanGame.Adapters;

public interface IChaosPowerUpSystem
{
    ChaosBoostResult ActivatePowerBoost(string playerId, int boostAmount);
    ChaosBoostResult ActivateRangeBoost(string playerId, int rangeIncrease);
    ChaosBoostResult ActivateVelocityBoost(string playerId);
}
