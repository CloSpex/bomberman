using BombermanGame.Factories.FactoryMethod;
using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.Factories.AbstractFactory;

public interface IGameModeFactory
{
    PowerUp CreatePowerUp(int x, int y, PowerUpType type);
    Explosion CreateExplosion(int x, int y);
    BombFactory GetBombFactory();
    string GetModeName();
    string GetModeDescription();
    double GetPowerUpDropRate();
    void ApplyModeEffectsToPlayer(Player player);
}

public class StandardModeFactory : IGameModeFactory
{
    public PowerUp CreatePowerUp(int x, int y, PowerUpType type)
    {
        return new PowerUp
        {
            X = x,
            Y = y,
            Type = type
        };
    }

    public Explosion CreateExplosion(int x, int y)
    {
        return new Explosion
        {
            X = x,
            Y = y,
            CreatedAt = DateTime.Now
        };
    }

    public BombFactory GetBombFactory()
    {
        return new StandardBombFactory();
    }

    public string GetModeName() => "Standard";

    public string GetModeDescription() => "Balanced gameplay with standard mechanics";

    public double GetPowerUpDropRate() => 0.3;

    public void ApplyModeEffectsToPlayer(Player player)
    {
        Console.WriteLine($"[StandardMode] Player {player.Name} - No modifications applied");
    }
}

public class ChaosModeFactory : IGameModeFactory
{
    public PowerUp CreatePowerUp(int x, int y, PowerUpType type)
    {
        var randomType = (PowerUpType)Random.Shared.Next(0, 4);
        Console.WriteLine($"[ChaosModeFactory] Requested {type}, dropping {randomType}");
        return new PowerUp
        {
            X = x,
            Y = y,
            Type = randomType
        };
    }

    public Explosion CreateExplosion(int x, int y)
    {
        return new Explosion
        {
            X = x,
            Y = y,
            CreatedAt = DateTime.Now.AddSeconds(-0.2)
        };
    }

    public BombFactory GetBombFactory()
    {
        var choice = Random.Shared.Next(0, 2);
        BombFactory factory = choice == 0
            ? (BombFactory)new StandardBombFactory()
            : (BombFactory)new EnhancedBombFactory();
        Console.WriteLine($"[ChaosModeFactory] Selected {factory.GetType().Name}");
        return factory;
    }

    public string GetModeName() => "Chaos";

    public string GetModeDescription() => "Unpredictable gameplay with random power-ups and bomb types";

    public double GetPowerUpDropRate() => 0.5;

    public void ApplyModeEffectsToPlayer(Player player)
    {
        var statChoice = Random.Shared.Next(0, 3);

        switch (statChoice)
        {
            case 0:
                player.BombCount += 1;
                Console.WriteLine($"[ChaosMode] Player {player.Name} - Bonus bomb! (Total: {player.BombCount})");
                break;
            case 1:
                player.BombRange += 1;
                Console.WriteLine($"[ChaosMode] Player {player.Name} - Bonus range! (Total: {player.BombRange})");
                break;
            case 2:
                player.MovementStrategy = new SpeedBoostMovementStrategy();
                MovementCooldownTracker.AddSpeedBoost(player.Id);
                Console.WriteLine($"[ChaosMode] Player {player.Name} - Speed boost!");
                break;
        }
    }
}

public class SpeedModeFactory : IGameModeFactory
{
    public PowerUp CreatePowerUp(int x, int y, PowerUpType type)
    {
        return new PowerUp
        {
            X = x,
            Y = y,
            Type = type
        };
    }

    public Explosion CreateExplosion(int x, int y)
    {
        return new Explosion
        {
            X = x,
            Y = y,
            CreatedAt = DateTime.Now.AddSeconds(0.3)
        };
    }

    public BombFactory GetBombFactory()
    {
        return new EnhancedBombFactory();
    }

    public string GetModeName() => "Speed";

    public string GetModeDescription() => "Fast-paced gameplay with enhanced bombs and quick explosions";

    public double GetPowerUpDropRate() => 0.4;

    public void ApplyModeEffectsToPlayer(Player player)
    {
        if (player.MovementStrategy is not SuperFastMovementStrategy)
        {
            player.MovementStrategy = new SuperFastMovementStrategy();
            MovementCooldownTracker.AddSpeedBoost(player.Id);
            MovementCooldownTracker.AddSpeedBoost(player.Id);
            Console.WriteLine($"[SpeedMode] Player {player.Name} - Super speed activated! Speed: {player.Speed}");
        }

        player.BombRange += 1;
        Console.WriteLine($"[SpeedMode] Player {player.Name} - Enhanced range! (Total: {player.BombRange})");
    }
}