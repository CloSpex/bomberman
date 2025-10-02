using BombermanGame.Services;

namespace BombermanGame.Commands;

public class MovePlayerCommand : ICommand
{
    private readonly IGameService _gameService;
    private readonly string _roomId;
    private readonly string _playerId;
    private readonly int _deltaX;
    private readonly int _deltaY;

    public MovePlayerCommand(IGameService gameService, string roomId, string playerId, int deltaX, int deltaY)
    {
        _gameService = gameService;
        _roomId = roomId;
        _playerId = playerId;
        _deltaX = deltaX;
        _deltaY = deltaY;
    }

    public async Task<CommandResult> ExecuteAsync()
    {
        var success = await _gameService.MovePlayerAsync(_roomId, _playerId, _deltaX, _deltaY);
        return new CommandResult { Success = success };
    }
}