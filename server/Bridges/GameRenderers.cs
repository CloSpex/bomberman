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
        var status = player.IsAlive ? "Alive" : "Dead";
        var color = player.Color;
        return $"Player {player.Name} [{color}] at ({player.X},{player.Y}) - {status}";
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
        var icon = powerUp.Type switch
        {
            PowerUpType.BombUp => "Bomb",
            PowerUpType.RangeUp => "Range",
            PowerUpType.SpeedUp => "Speed",
            _ => "?"
        };
        return $"{icon} PowerUp {powerUp.Type} at ({powerUp.X},{powerUp.Y})";
    }

    public string RenderGameBoard(GameBoard board)
    {
        var result = $"╔══════════════════════════╗\n";
        result += $"║  Board {GameBoard.Width}x{GameBoard.Height}            ║\n";
        result += $"╠══════════════════════════╣\n";
        result += $"║  Bombs: {board.Bombs.Count,-3}            ║\n";
        result += $"║  Explosions: {board.Explosions.Count,-3}       ║\n";
        result += $"║  PowerUps: {board.PowerUps.Count,-3}         ║\n";
        result += $"╚══════════════════════════╝";
        return result;
    }
}

public class CanvasGameRenderer : IGameRenderer
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
            board.Bombs,
            board.Explosions,
            board.PowerUps,
            Width = GameBoard.Width,
            Height = GameBoard.Height
        });
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

public class ExplosionElement : GameElement
{
    private readonly Explosion _explosion;

    public ExplosionElement(Explosion explosion, IGameRenderer renderer) : base(renderer)
    {
        _explosion = explosion;
    }

    public override string Render()
    {
        return _renderer.RenderExplosion(_explosion);
    }
}

public class PowerUpElement : GameElement
{
    private readonly PowerUp _powerUp;

    public PowerUpElement(PowerUp powerUp, IGameRenderer renderer) : base(renderer)
    {
        _powerUp = powerUp;
    }

    public override string Render()
    {
        return _renderer.RenderPowerUp(_powerUp);
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