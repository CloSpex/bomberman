using BombermanGame.Models;

namespace BombermanGame.Decorators;

public class PlayerDecoratorManager
{
    private readonly Dictionary<string, IPlayerDecorator> _decoratedPlayers = new();

    public void RegisterPlayer(Player player)
    {
        _decoratedPlayers[player.Id] = new BasePlayerDecorator(player);
    }

    public void ApplyBombUpgrade(string playerId)
    {
        if (_decoratedPlayers.TryGetValue(playerId, out var decorator))
        {
            _decoratedPlayers[playerId] = new BombCountDecorator(decorator, 1);
        }
    }

    public void ApplyRangeUpgrade(string playerId)
    {
        if (_decoratedPlayers.TryGetValue(playerId, out var decorator))
        {
            _decoratedPlayers[playerId] = new BombRangeDecorator(decorator, 1);
        }
    }

    public void ApplySpeedUpgrade(string playerId)
    {
        if (_decoratedPlayers.TryGetValue(playerId, out var decorator))
        {
            _decoratedPlayers[playerId] = new SpeedDecorator(decorator, 1);
        }
    }

    public IPlayerDecorator? GetDecoratedPlayer(string playerId)
    {
        _decoratedPlayers.TryGetValue(playerId, out var decorator);
        return decorator;
    }

    public int GetEffectiveBombCount(string playerId)
    {
        return _decoratedPlayers.TryGetValue(playerId, out var decorator)
            ? decorator.GetBombCount()
            : 1;
    }

    public int GetEffectiveBombRange(string playerId)
    {
        return _decoratedPlayers.TryGetValue(playerId, out var decorator)
            ? decorator.GetBombRange()
            : 2;
    }

    public int GetEffectiveSpeed(string playerId)
    {
        return _decoratedPlayers.TryGetValue(playerId, out var decorator)
            ? decorator.GetMovementSpeed()
            : 1;
    }

    public void RemovePlayer(string playerId)
    {
        _decoratedPlayers.Remove(playerId);
    }
}