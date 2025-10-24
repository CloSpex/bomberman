using BombermanGame.Models;

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
        var newBoard = new GameBoard();

        for (int y = 0; y < GameBoard.Height; y++)
        {
            Array.Copy(_template.Grid[y], newBoard.Grid[y], GameBoard.Width);
        }

        newBoard.Bombs = _template.Bombs.Select(b => b.Clone()).ToList();
        newBoard.Explosions = _template.Explosions.Select(e => e.Clone()).ToList();
        newBoard.PowerUps = _template.PowerUps.Select(p => p.Clone()).ToList();

        return newBoard;
    }

    public GameBoard ShallowClone()
    {
        var newBoard = new GameBoard
        {
            Grid = _template.Grid,
            Bombs = _template.Bombs,
            Explosions = _template.Explosions,
            PowerUps = _template.PowerUps
        };
        return newBoard;
    }
}