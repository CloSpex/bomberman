namespace BombermanGame.Commands;

public interface ICommandHandler
{
    Task<CommandResult> HandleAsync(ICommand command);
}

public class GameCommandHandler : ICommandHandler
{
    public async Task<CommandResult> HandleAsync(ICommand command)
    {
        return await command.ExecuteAsync();
    }
}