namespace BombermanGame.Models;

public class GameBoard
{
    public const int Width = 15;
    public const int Height = 13;

    public int[][] Grid { get; set; } = new int[Height][];
    public List<Bomb> Bombs { get; set; } = new();
    public List<Explosion> Explosions { get; set; } = new();
    public List<PowerUp> PowerUps { get; set; } = new();

    public GameBoard()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int y = 0; y < Height; y++)
        {
            Grid[y] = new int[Width];
        }

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    Grid[y][x] = (int)CellType.Wall;
                }
                else if (x % 2 == 0 && y % 2 == 0)
                {
                    Grid[y][x] = (int)CellType.Wall;
                }
                else if ((x == 1 && y == 1) || (x == 1 && y == 2) || (x == 2 && y == 1) ||
                         (x == Width - 2 && y == 1) || (x == Width - 2 && y == 2) || (x == Width - 3 && y == 1) ||
                         (x == 1 && y == Height - 2) || (x == 1 && y == Height - 3) || (x == 2 && y == Height - 2) ||
                         (x == Width - 2 && y == Height - 2) || (x == Width - 2 && y == Height - 3) || (x == Width - 3 && y == Height - 2))
                {
                    Grid[y][x] = (int)CellType.Empty;
                }
                else if (Random.Shared.NextDouble() < 0.6)
                {
                    Grid[y][x] = (int)CellType.DestructibleWall;
                }
                else
                {
                    Grid[y][x] = (int)CellType.Empty;
                }
            }
        }
    }

    public CellType GetCellType(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return CellType.Wall;
        return (CellType)Grid[y][x];
    }

    public void SetCellType(int x, int y, CellType cellType)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            Grid[y][x] = (int)cellType;
        }
    }
}