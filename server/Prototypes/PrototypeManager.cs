using BombermanGame.Models;
namespace BombermanGame.Prototypes;
public class PrototypeManager
{
    private readonly Dictionary<string, IPrototype<Player>> _playerPrototypes = new();
    private readonly Dictionary<string, IPrototype<GameBoard>> _boardPrototypes = new();

    public void RegisterPlayerPrototype(string key, Player template)
    {
        _playerPrototypes[key] = new PlayerPrototype(template);
    }

    public void RegisterBoardPrototype(string key, GameBoard template)
    {
        _boardPrototypes[key] = new GameBoardPrototype(template);
    }

    public Player? CreatePlayerFromPrototype(string key)
    {
        return _playerPrototypes.TryGetValue(key, out var prototype) ? prototype.Clone() : null;
    }

    public GameBoard? CreateBoardFromPrototype(string key)
    {
        return _boardPrototypes.TryGetValue(key, out var prototype) ? prototype.Clone() : null;
    }
}