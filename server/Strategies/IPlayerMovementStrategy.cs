using BombermanGame.Models;
using System.Collections.Concurrent;

namespace BombermanGame.Strategies;

public interface IPlayerMovementStrategy
{
    bool CanMove(GameBoard board, int x, int y);
    bool CanMoveNow(string playerId);
    int GetBaseMovementCooldownMs();
}
public static class MovementCooldownTracker
{
    private static readonly ConcurrentDictionary<string, DateTime> _lastMoveTime = new();
    private static readonly ConcurrentDictionary<string, int> _speedBoostCount = new();

    private const int SPEED_BOOST_REDUCTION_MS = 20;
    private const int MINIMUM_COOLDOWN_MS = 50;

    public static bool CanMoveNow(string playerId, int baseCooldownMs)
    {
        var effectiveCooldown = GetEffectiveCooldown(playerId, baseCooldownMs);

        if (!_lastMoveTime.TryGetValue(playerId, out var lastMove))
        {
            _lastMoveTime[playerId] = DateTime.Now;
            return true;
        }

        var timeSinceLastMove = (DateTime.Now - lastMove).TotalMilliseconds;
        if (timeSinceLastMove >= effectiveCooldown)
        {
            _lastMoveTime[playerId] = DateTime.Now;
            return true;
        }

        return false;
    }

    public static void AddSpeedBoost(string playerId)
    {
        _speedBoostCount.AddOrUpdate(playerId, 1, (key, current) => current + 1);
    }

    public static int GetSpeedBoostCount(string playerId)
    {
        return _speedBoostCount.TryGetValue(playerId, out var count) ? count : 0;
    }

    public static int GetEffectiveCooldown(string playerId, int baseCooldownMs)
    {
        var boostCount = GetSpeedBoostCount(playerId);
        var reduction = boostCount * SPEED_BOOST_REDUCTION_MS;
        var effectiveCooldown = Math.Max(baseCooldownMs - reduction, MINIMUM_COOLDOWN_MS);
        return effectiveCooldown;
    }

    public static void ResetPlayer(string playerId)
    {
        _lastMoveTime.TryRemove(playerId, out _);
        _speedBoostCount.TryRemove(playerId, out _);
    }
}

public class NormalMovementStrategy : IPlayerMovementStrategy
{
    private const int BASE_MOVEMENT_COOLDOWN_MS = 200;

    public bool CanMove(GameBoard board, int x, int y)
    {
        if (x < 0 || x >= GameBoard.Width || y < 0 || y >= GameBoard.Height)
            return false;

        var cellType = (CellType)board.Grid[y][x];
        if (cellType == CellType.Wall || cellType == CellType.DestructibleWall)
            return false;

        return !board.Bombs.Any(b => b.X == x && b.Y == y);
    }

    public bool CanMoveNow(string playerId)
    {
        return MovementCooldownTracker.CanMoveNow(playerId, BASE_MOVEMENT_COOLDOWN_MS);
    }


    public int GetBaseMovementCooldownMs() => BASE_MOVEMENT_COOLDOWN_MS;
}

public class SpeedBoostMovementStrategy : IPlayerMovementStrategy
{
    private const int BASE_MOVEMENT_COOLDOWN_MS = 150;

    public bool CanMove(GameBoard board, int x, int y)
    {
        if (x < 0 || x >= GameBoard.Width || y < 0 || y >= GameBoard.Height)
            return false;

        var cellType = (CellType)board.Grid[y][x];
        if (cellType == CellType.Wall || cellType == CellType.DestructibleWall)
            return false;

        return !board.Bombs.Any(b => b.X == x && b.Y == y);
    }

    public bool CanMoveNow(string playerId)
    {
        return MovementCooldownTracker.CanMoveNow(playerId, BASE_MOVEMENT_COOLDOWN_MS);
    }


    public int GetBaseMovementCooldownMs() => BASE_MOVEMENT_COOLDOWN_MS;
}

public class SlowMovementStrategy : IPlayerMovementStrategy
{
    private const int BASE_MOVEMENT_COOLDOWN_MS = 300;

    public bool CanMove(GameBoard board, int x, int y)
    {
        if (x < 0 || x >= GameBoard.Width || y < 0 || y >= GameBoard.Height)
            return false;

        var cellType = (CellType)board.Grid[y][x];
        if (cellType == CellType.Wall || cellType == CellType.DestructibleWall)
            return false;

        return !board.Bombs.Any(b => b.X == x && b.Y == y);
    }

    public bool CanMoveNow(string playerId)
    {
        return MovementCooldownTracker.CanMoveNow(playerId, BASE_MOVEMENT_COOLDOWN_MS);
    }

    public int GetBaseMovementCooldownMs() => BASE_MOVEMENT_COOLDOWN_MS;
}

public class SuperFastMovementStrategy : IPlayerMovementStrategy
{
    private const int BASE_MOVEMENT_COOLDOWN_MS = 100;

    public bool CanMove(GameBoard board, int x, int y)
    {
        if (x < 0 || x >= GameBoard.Width || y < 0 || y >= GameBoard.Height)
            return false;

        var cellType = (CellType)board.Grid[y][x];
        if (cellType == CellType.Wall || cellType == CellType.DestructibleWall)
            return false;

        return !board.Bombs.Any(b => b.X == x && b.Y == y);
    }

    public bool CanMoveNow(string playerId)
    {
        return MovementCooldownTracker.CanMoveNow(playerId, BASE_MOVEMENT_COOLDOWN_MS);
    }

    public int GetBaseMovementCooldownMs() => BASE_MOVEMENT_COOLDOWN_MS;
}