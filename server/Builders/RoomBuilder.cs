using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.Builders;

public interface IGameRoomBuilder
{
    IGameRoomBuilder WithId(string id);
    IGameRoomBuilder WithState(GameState state);
    IGameRoomBuilder WithBoard(GameBoard board);
    IGameRoomBuilder AddPlayer(Player player);
    GameRoom Build();
}

public class GameRoomBuilder : IGameRoomBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private GameState _state = GameState.Waiting;
    private GameBoard _board = new GameBoard();
    private readonly List<Player> _players = new();

    public IGameRoomBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public IGameRoomBuilder WithState(GameState state)
    {
        _state = state;
        return this;
    }

    public IGameRoomBuilder WithBoard(GameBoard board)
    {
        _board = board;
        return this;
    }

    public IGameRoomBuilder AddPlayer(Player player)
    {
        _players.Add(player);
        return this;
    }

    public GameRoom Build()
    {
        var room = new GameRoom
        {
            Id = _id,
            State = _state,
            Board = _board,
            Players = _players,
            LastUpdate = DateTime.Now
        };
        room.UpdateStateHandler();
        return room;
    }
}