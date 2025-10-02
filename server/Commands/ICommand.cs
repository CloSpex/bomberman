namespace BombermanGame.Commands;

public interface ICommand
{
    Task<CommandResult> ExecuteAsync();
}

public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public object? Data { get; set; }
}