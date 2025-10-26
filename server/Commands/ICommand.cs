namespace BombermanGame.Commands;

public interface ICommand
{
    Task<CommandResult> ExecuteAsync();
    Task<CommandResult> UndoAsync();
}

public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public object? Data { get; set; }
}