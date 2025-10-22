using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.Builders;

public interface IPlayerBuilder
{
    IPlayerBuilder WithId(string id);
    IPlayerBuilder WithName(string name);
    IPlayerBuilder WithPosition(int x, int y);
    IPlayerBuilder WithColor(string color);
    IPlayerBuilder WithBombCount(int count);
    IPlayerBuilder WithBombRange(int range);
    IPlayerBuilder WithMovementStrategy(IPlayerMovementStrategy strategy);
    Player Build();
}

public class PlayerBuilder : IPlayerBuilder
{
    private string _id = "";
    private string _name = "";
    private int _x = 1;
    private int _y = 1;
    private string _color = "#ff0000";
    private int _bombCount = 1;
    private int _bombRange = 2;
    private IPlayerMovementStrategy _movementStrategy = new NormalMovementStrategy();

    public IPlayerBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public IPlayerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public IPlayerBuilder WithPosition(int x, int y)
    {
        _x = x;
        _y = y;
        return this;
    }

    public IPlayerBuilder WithColor(string color)
    {
        _color = color;
        return this;
    }

    public IPlayerBuilder WithBombCount(int count)
    {
        _bombCount = count;
        return this;
    }

    public IPlayerBuilder WithBombRange(int range)
    {
        _bombRange = range;
        return this;
    }

    public IPlayerBuilder WithMovementStrategy(IPlayerMovementStrategy strategy)
    {
        _movementStrategy = strategy;
        return this;
    }

    public Player Build()
    {
        var player = new Player
        {
            Id = _id,
            Name = _name,
            X = _x,
            Y = _y,
            Color = _color,
            BombCount = _bombCount,
            BombRange = _bombRange,
            MovementStrategy = _movementStrategy,
            IsAlive = true
        };

        _id = "";
        _name = "";
        _x = 1;
        _y = 1;
        _color = "#ff0000";
        _bombCount = 1;
        _bombRange = 2;
        _movementStrategy = new NormalMovementStrategy();

        return player;
    }
}

