using BombermanGame.Models;
using BombermanGame.Services;

namespace BombermanGame.Commands;

public class PlaceBombCommand : ICommand
{
    private readonly IGameService _gameService;
    private readonly string _roomId;
    private readonly string _playerId;

    private Bomb? _placedBomb;

    public PlaceBombCommand(IGameService gameService, string roomId, string playerId)
    {
        _gameService = gameService;
        _roomId = roomId;
        _playerId = playerId;
    }

    public async Task<CommandResult> ExecuteAsync()
    {
        var success = await _gameService.PlaceBombAsync(_roomId, _playerId);

        if (success)
        {
            _placedBomb = _gameService.GetRoom(_roomId)?.Board.Bombs
                .FindLast(b => b.PlayerId == _playerId);
        }

        return new CommandResult { Success = success };
    }

    public async Task<CommandResult> UndoAsync()
    {
        if (_placedBomb == null)
            return new CommandResult { Success = false, Message = "No bomb placed to undo." };

        var room = _gameService.GetRoom(_roomId);
        if (room == null)
            return new CommandResult { Success = false, Message = "Game room not found." };

        var bombRemoved = room.Board.Bombs.Remove(_placedBomb);
        if (!bombRemoved)
            return new CommandResult { Success = false, Message = "Failed to remove the bomb." };

        return new CommandResult { Success = true };
    }
}