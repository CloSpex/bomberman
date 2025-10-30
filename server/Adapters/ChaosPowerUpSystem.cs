using BombermanGame.Models;

namespace BombermanGame.Adapters;


public record ChaosBoostResult(string Type, int Amount, string Message);

public class ChaosPowerUpSystem : IChaosPowerUpSystem
{
    private static readonly Random _rand = new();

    public ChaosBoostResult ActivatePowerBoost(string playerId, int boostAmount)
    {
        var actualBoost = _rand.Next(0, boostAmount + 2);
        var msg = $"[Legacy] Power boost +{actualBoost} for {playerId}";
        Console.WriteLine(msg);
        return new("Power", actualBoost, msg);
    }

    public ChaosBoostResult ActivateRangeBoost(string playerId, int rangeIncrease)
    {
        var actualBoost = _rand.Next(1, rangeIncrease + 2);
        var msg = $"[Legacy] Range boost +{actualBoost} for {playerId}";
        Console.WriteLine(msg);
        return new("Range", actualBoost, msg);
    }

    public ChaosBoostResult ActivateVelocityBoost(string playerId)
    {
        var boost = _rand.Next(1, 3);
        var msg = $"[Legacy] Velocity boost (x{boost}) for {playerId}";
        Console.WriteLine(msg);
        return new("Speed", boost, msg);
    }
}
