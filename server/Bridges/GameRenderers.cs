using BombermanGame.Models;

namespace BombermanGame.Bridges;

public interface IGameRenderer
{
    string RenderPlayer(Player player);
    string RenderBomb(Bomb bomb);
    string RenderExplosion(Explosion explosion);
    string RenderPowerUp(PowerUp powerUp);
    string RenderGameBoard(GameBoard board);
}

public class JsonGameRenderer : IGameRenderer
{
    public string RenderPlayer(Player player)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            player.Id,
            player.Name,
            player.X,
            player.Y,
            player.IsAlive,
            player.Color
        });
    }

    public string RenderBomb(Bomb bomb)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            bomb.Id,
            bomb.X,
            bomb.Y,
            bomb.PlayerId,
            bomb.Range
        });
    }

    public string RenderExplosion(Explosion explosion)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            explosion.X,
            explosion.Y
        });
    }

    public string RenderPowerUp(PowerUp powerUp)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            powerUp.X,
            powerUp.Y,
            Type = powerUp.Type.ToString()
        });
    }

    public string RenderGameBoard(GameBoard board)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            board.Grid,
            Bombs = board.Bombs.Count,
            Explosions = board.Explosions.Count,
            PowerUps = board.PowerUps.Count
        });
    }
}

public class TextGameRenderer : IGameRenderer
{
    public string RenderPlayer(Player player)
    {
        return $"Player {player.Name} at ({player.X},{player.Y}) - {(player.IsAlive ? "Alive" : "Dead")}";
    }

    public string RenderBomb(Bomb bomb)
    {
        return $"Bomb at ({bomb.X},{bomb.Y}) Range:{bomb.Range}";
    }

    public string RenderExplosion(Explosion explosion)
    {
        return $"Explosion at ({explosion.X},{explosion.Y})";
    }

    public string RenderPowerUp(PowerUp powerUp)
    {
        return $"PowerUp {powerUp.Type} at ({powerUp.X},{powerUp.Y})";
    }

    public string RenderGameBoard(GameBoard board)
    {
        var result = $"Board {GameBoard.Width}x{GameBoard.Height}\n";
        result += $"Bombs: {board.Bombs.Count}, Explosions: {board.Explosions.Count}, PowerUps: {board.PowerUps.Count}";
        return result;
    }
}

public abstract class GameElement
{
    protected IGameRenderer _renderer;

    protected GameElement(IGameRenderer renderer)
    {
        _renderer = renderer;
    }

    public void SetRenderer(IGameRenderer renderer)
    {
        _renderer = renderer;
    }

    public abstract string Render();
}

public class PlayerElement : GameElement
{
    private readonly Player _player;

    public PlayerElement(Player player, IGameRenderer renderer) : base(renderer)
    {
        _player = player;
    }

    public override string Render()
    {
        return _renderer.RenderPlayer(_player);
    }
}

public class BombElement : GameElement
{
    private readonly Bomb _bomb;

    public BombElement(Bomb bomb, IGameRenderer renderer) : base(renderer)
    {
        _bomb = bomb;
    }

    public override string Render()
    {
        return _renderer.RenderBomb(_bomb);
    }
}

public class GameBoardElement : GameElement
{
    private readonly GameBoard _board;

    public GameBoardElement(GameBoard board, IGameRenderer renderer) : base(renderer)
    {
        _board = board;
    }

    public override string Render()
    {
        return _renderer.RenderGameBoard(_board);
    }
}