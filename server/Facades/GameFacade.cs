using BombermanGame.Models;
using BombermanGame.Services;
using BombermanGame.Commands;
using BombermanGame.Events;
using BombermanGame.Factories;

namespace BombermanGame.Facades;

public interface IGameFacade
{
    Task<GameRoom> CreateAndJoinRoomAsync(string roomId, string playerId, string playerName);
    Task<bool> StartGameSessionAsync(string roomId);
    Task<bool> PerformPlayerActionAsync(string roomId, string playerId, PlayerAction action);
    Task<GameRoomStatus> GetRoomStatusAsync(string roomId);
}

public enum PlayerAction
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    PlaceBomb
}

public class GameRoomStatus
{
    public string RoomId { get; set; } = "";
    public int PlayerCount { get; set; }
    public int AlivePlayerCount { get; set; }
    public string State { get; set; } = "";
    public bool CanStart { get; set; }
    public bool IsFinished { get; set; }
}

public class GameFacade : IGameFacade
{
    private readonly IGameService _gameService;
    private readonly ICommandHandler _commandHandler;
    private readonly IGameFactory _gameFactory;
    private readonly IEventPublisher _eventPublisher;

    public GameFacade(
        IGameService gameService,
        ICommandHandler commandHandler,
        IGameFactory gameFactory,
        IEventPublisher eventPublisher)
    {
        _gameService = gameService;
        _commandHandler = commandHandler;
        _gameFactory = gameFactory;
        _eventPublisher = eventPublisher;
    }

    public async Task<GameRoom> CreateAndJoinRoomAsync(string roomId, string playerId, string playerName)
    {
        var room = _gameService.GetRoom(roomId) ?? _gameService.CreateRoom(roomId);
        var player = _gameFactory.CreatePlayer(playerId, playerName);
        
        await _gameService.JoinRoomAsync(roomId, player);
        
        return room;
    }

    public async Task<bool> StartGameSessionAsync(string roomId)
    {
        var room = _gameService.GetRoom(roomId);
        if (room == null || room.Players.Count < 2)
            return false;

        await _gameService.StartGameAsync(roomId);
        return true;
    }

    public async Task<bool> PerformPlayerActionAsync(string roomId, string playerId, PlayerAction action)
    {
        ICommand command = action switch
        {
            PlayerAction.MoveUp => new MovePlayerCommand(_gameService, roomId, playerId, 0, -1),
            PlayerAction.MoveDown => new MovePlayerCommand(_gameService, roomId, playerId, 0, 1),
            PlayerAction.MoveLeft => new MovePlayerCommand(_gameService, roomId, playerId, -1, 0),
            PlayerAction.MoveRight => new MovePlayerCommand(_gameService, roomId, playerId, 1, 0),
            PlayerAction.PlaceBomb => new PlaceBombCommand(_gameService, roomId, playerId),
            _ => throw new ArgumentException("Invalid action")
        };

        var result = await _commandHandler.HandleAsync(command);
        return result.Success;
    }

    public async Task<GameRoomStatus> GetRoomStatusAsync(string roomId)
    {
        var room = _gameService.GetRoom(roomId);
        if (room == null)
            return new GameRoomStatus { RoomId = roomId };

        return await Task.FromResult(new GameRoomStatus
        {
            RoomId = room.Id,
            PlayerCount = room.Players.Count,
            AlivePlayerCount = room.Players.Count(p => p.IsAlive),
            State = room.State.ToString(),
            CanStart = room.Players.Count >= 2 && room.State == GameState.Waiting,
            IsFinished = room.State == GameState.Finished
        });
    }
}