using BombermanGame.Services;

namespace BombermanGame.Commands;

public class MovePlayerCommand : ICommand
{
    private readonly IGameService _gameService;
    private readonly string _roomId;
    private readonly string _playerId;
    private readonly int _deltaX;
    private readonly int _deltaY;

    private int? _previousX;
    private int? _previousY;

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
        var room = _gameService.GetRoom(_roomId);
        var player = room?.Players.Find(p => p.Id == _playerId);

        if (player == null)
            return new CommandResult { Success = false, Message = "Player not found." };

        _previousX = player.X;
        _previousY = player.Y;

        var success = await _gameService.MovePlayerAsync(_roomId, _playerId, _deltaX, _deltaY);
        return new CommandResult { Success = success };
    }

    public async Task<CommandResult> UndoAsync()
    {
        if (_previousX == null || _previousY == null)
            return new CommandResult { Success = false, Message = "No previous state to undo to." };

        var room = _gameService.GetRoom(_roomId);
        var player = room?.Players.Find(p => p.Id == _playerId);
        if (player == null)
            return new CommandResult { Success = false, Message = "Player not found." };

        // compute delta to move the player back to the previous position
        var deltaX = _previousX.Value - player.X;
        var deltaY = _previousY.Value - player.Y;

        if (deltaX == 0 && deltaY == 0)
            return new CommandResult { Success = true, Message = "Already at previous position." };

        var success = await _gameService.MovePlayerAsync(_roomId, _playerId, deltaX, deltaY);
        return new CommandResult
        {
            Success = success,
            Message = success ? "Move undone successfully." : "Undo failed."
        };
    }
}