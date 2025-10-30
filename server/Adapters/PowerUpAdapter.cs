using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.Adapters;



public class PowerUpAdapter : IModernPowerUpProcessor
{
    private readonly IChaosPowerUpSystem _legacySystem;

    public PowerUpAdapter(IChaosPowerUpSystem legacySystem)
    {
        _legacySystem = legacySystem;
    }

    public void ProcessPowerUp(Player player, PowerUpType type)
    {
        ChaosBoostResult result = type switch
        {
            PowerUpType.BombUp => _legacySystem.ActivatePowerBoost(player.Id, 1),
            PowerUpType.RangeUp => _legacySystem.ActivateRangeBoost(player.Id, 1),
            PowerUpType.SpeedUp => _legacySystem.ActivateVelocityBoost(player.Id),
            _ => new("None", 0, "[Legacy] Unknown power-up ignored")
        };

        switch (result.Type)
        {
            case "Power":
                player.BombCount += result.Amount;
                break;
            case "Range":
                player.BombRange += result.Amount;
                break;
            case "Speed":
                UpgradeSpeedStrategy(player, result.Amount);
                break;
        }

        Console.WriteLine($"[Adapter] Applied legacy {result.Type} x{result.Amount} to {player.Name}");
    }
    private void UpgradeSpeedStrategy(Player player, int boostLevel)
    {
        IPlayerMovementStrategy newStrategy = player.MovementStrategy switch
        {
            SlowMovementStrategy => new NormalMovementStrategy(),
            NormalMovementStrategy => new SpeedBoostMovementStrategy(),
            SpeedBoostMovementStrategy => new SuperFastMovementStrategy(),
            SuperFastMovementStrategy => player.MovementStrategy,
            _ => new NormalMovementStrategy()
        };

        player.MovementStrategy = newStrategy;
        for (int i = 0; i < boostLevel; i++)
            MovementCooldownTracker.AddSpeedBoost(player.Id);

        Console.WriteLine($"[Adapter] {player.Name} speed upgraded → {newStrategy.GetType().Name} (x{boostLevel})");
    }

    public string GetProcessorInfo() => "PowerUpAdapter using LegacyPowerUpSystem";
}
