using BombermanGame.Models;
namespace BombermanGame.Adapters;


public interface IModernPowerUpProcessor
{
    void ProcessPowerUp(Player player, PowerUpType type);
    string GetProcessorInfo();
}