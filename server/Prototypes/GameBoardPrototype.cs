using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.Prototypes;


public class GameBoardPrototype : IPrototype<GameBoard>
{
    private readonly GameBoard _template;

    public GameBoardPrototype(GameBoard template)
    {
        _template = template;
    }

    public GameBoard Clone()
    {
        var newBoard = new GameBoard
        {
            Grid = new int[GameBoard.Height][],
            Bombs = new List<Bomb>(_template.Bombs.Select(b => new Bomb
            {
                Id = b.Id,
                X = b.X,
                Y = b.Y,
                PlayerId = b.PlayerId,
                PlacedAt = b.PlacedAt,
                Range = b.Range
            })),
            Explosions = new List<Explosion>(_template.Explosions.Select(e => new Explosion
            {
                X = e.X,
                Y = e.Y,
                CreatedAt = e.CreatedAt
            })),
            PowerUps = new List<PowerUp>(_template.PowerUps.Select(p => new PowerUp
            {
                X = p.X,
                Y = p.Y,
                Type = p.Type
            }))
        };

        for (int y = 0; y < GameBoard.Height; y++)
        {
            newBoard.Grid[y] = new int[GameBoard.Width];
            Array.Copy(_template.Grid[y], newBoard.Grid[y], GameBoard.Width);
        }

        return newBoard;
    }
}
