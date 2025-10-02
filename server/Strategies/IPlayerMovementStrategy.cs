using BombermanGame.Models;

namespace BombermanGame.Strategies;

public interface IPlayerMovementStrategy
{
    bool CanMove(GameBoard board, int x, int y);
}

public class NormalMovementStrategy : IPlayerMovementStrategy
{
    public bool CanMove(GameBoard board, int x, int y)
    {
        if (x < 0 || x >= GameBoard.Width || y < 0 || y >= GameBoard.Height)
            return false;

        var cellType = (CellType)board.Grid[y][x];
        if (cellType == CellType.Wall || cellType == CellType.DestructibleWall)
            return false;

        return !board.Bombs.Any(b => b.X == x && b.Y == y);
    }
}

public class SpeedBoostMovementStrategy : IPlayerMovementStrategy
{
    public bool CanMove(GameBoard board, int x, int y)
    {
        return new NormalMovementStrategy().CanMove(board, x, y);
    }
}