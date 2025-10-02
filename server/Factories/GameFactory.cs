using BombermanGame.Models;

namespace BombermanGame.Factories;

public interface IGameFactory
{
    GameRoom CreateGameRoom(string roomId);
    Player CreatePlayer(string id, string name);
    GameBoard CreateGameBoard();
}

public class GameFactory : IGameFactory
{
    public GameRoom CreateGameRoom(string roomId)
    {
        return new GameRoom { Id = roomId };
    }

    public Player CreatePlayer(string id, string name)
    {
        return new Player { Id = id, Name = name };
    }

    public GameBoard CreateGameBoard()
    {
        return new GameBoard();
    }
}