using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.Prototypes;

public class PlayerPrototype : IPrototype<Player>
{
    private readonly Player _template;

    public PlayerPrototype(Player template)
    {
        _template = template;
    }


    public Player Clone()
    {
        IPlayerMovementStrategy newStrategy = _template.MovementStrategy switch
        {
            NormalMovementStrategy => new NormalMovementStrategy(),
            SpeedBoostMovementStrategy => new SpeedBoostMovementStrategy(),
            SlowMovementStrategy => new SlowMovementStrategy(),
            _ => new NormalMovementStrategy()
        };

        return new Player
        {
            Id = _template.Id,
            Name = _template.Name,
            X = _template.X,
            Y = _template.Y,
            IsAlive = _template.IsAlive,
            BombCount = _template.BombCount,
            BombRange = _template.BombRange,
            Color = _template.Color,
            MovementStrategy = newStrategy
        };
    }

    public Player ShallowClone()
    {
        return new Player
        {
            Id = _template.Id,
            Name = _template.Name,
            X = _template.X,
            Y = _template.Y,
            IsAlive = _template.IsAlive,
            BombCount = _template.BombCount,
            BombRange = _template.BombRange,
            Color = _template.Color,
            MovementStrategy = _template.MovementStrategy
        };
    }
}