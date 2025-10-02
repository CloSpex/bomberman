namespace BombermanGame.Models;

public enum CellType
{
    Empty,
    Wall,
    DestructibleWall
}

public enum GameState
{
    Waiting,
    Playing,
    Finished
}

public enum PowerUpType
{
    BombUp,
    RangeUp,
    SpeedUp
}