using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.Adapters;

public class DirectPowerUpProcessor : IModernPowerUpProcessor
{
    public void ProcessPowerUp(Player player, PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.BombUp:
                player.BombCount++;
                Console.WriteLine($"[Direct] {player.Name} gained +1 Bomb (Total: {player.BombCount})");
                break;

            case PowerUpType.RangeUp:
                player.BombRange++;
                Console.WriteLine($"[Direct] {player.Name} gained +1 Range (Total: {player.BombRange})");
                break;

            case PowerUpType.SpeedUp:
                UpgradeSpeedStrategy(player);
                break;

            default:
                Console.WriteLine($"[Direct] Unknown power-up ignored for {player.Name}");
                break;
        }
    }

    private void UpgradeSpeedStrategy(Player player)
    {
        IPlayerMovementStrategy newStrategy = player.MovementStrategy switch
        {
            SlowMovementStrategy => new NormalMovementStrategy(),
            NormalMovementStrategy => new SpeedBoostMovementStrategy(),
            SpeedBoostMovementStrategy => new SuperFastMovementStrategy(),
            SuperFastMovementStrategy => player.MovementStrategy,
            _ => new NormalMovementStrategy()
        };

        MovementCooldownTracker.AddSpeedBoost(player.Id);
        player.MovementStrategy = newStrategy;

        Console.WriteLine($"[Direct] {player.Name} speed upgraded → {newStrategy.GetType().Name}");
    }

    public string GetProcessorInfo() => "DirectPowerUpProcessor (Modern system)";
}
