using BombermanGame.Services;

namespace BombermanGame.Commands;

public class PlaceBombCommand : ICommand
{
    private readonly IGameService _gameService;
    private readonly string _roomId;
    private readonly string _playerId;

    public PlaceBombCommand(IGameService gameService, string roomId, string playerId)
    {
        _gameService = gameService;
        _roomId = roomId;
        _playerId = playerId;
    }

    public async Task<CommandResult> ExecuteAsync()
    {
        var success = await _gameService.PlaceBombAsync(_roomId, _playerId);
        return new CommandResult { Success = success };
    }
}