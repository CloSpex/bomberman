using Microsoft.AspNetCore.SignalR;
using BombermanGame.Services;
using BombermanGame.Commands;
using BombermanGame.Factories;

namespace BombermanGame.Hubs;

public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly ICommandHandler _commandHandler;
    private readonly IGameFactory _gameFactory;

    public GameHub(IGameService gameService, ICommandHandler commandHandler, IGameFactory gameFactory)
    {
        _gameService = gameService;
        _commandHandler = commandHandler;
        _gameFactory = gameFactory;
    }

    public async Task JoinRoom(string roomId, string playerName)
    {
        var player = _gameFactory.CreatePlayer(Context.ConnectionId, playerName);

        if (await _gameService.JoinRoomAsync(roomId, player))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            var room = _gameService.GetRoom(roomId);
            await Clients.Group(roomId).SendAsync("PlayerJoined", room);
        }
        else
        {
            await Clients.Caller.SendAsync("JoinFailed", "Room is full or game in progress");
        }
    }

    public async Task StartGame(string roomId)
    {
        await _gameService.StartGameAsync(roomId);
        var room = _gameService.GetRoom(roomId);
        if (room != null)
        {
            await Clients.Group(roomId).SendAsync("GameStarted", room);
        }
    }

    public async Task MovePlayer(string roomId, int deltaX, int deltaY)
    {
        var command = new MovePlayerCommand(_gameService, roomId, Context.ConnectionId, deltaX, deltaY);
        var result = await _commandHandler.HandleAsync(command);

        if (result.Success)
        {
            var room = _gameService.GetRoom(roomId);
            await Clients.Group(roomId).SendAsync("GameUpdated", room);
        }
    }

    public async Task PlaceBomb(string roomId)
    {
        var command = new PlaceBombCommand(_gameService, roomId, Context.ConnectionId);
        var result = await _commandHandler.HandleAsync(command);

        if (result.Success)
        {
            var room = _gameService.GetRoom(roomId);
            await Clients.Group(roomId).SendAsync("GameUpdated", room);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}