using BombermanGame.States;

namespace BombermanGame.Models;

public class GameRoom
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<Player> Players { get; set; } = new();
    public GameState State { get; set; } = GameState.Waiting;
    public GameBoard Board { get; set; } = new();
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    public GameStateBase StateHandler { get; set; } = new WaitingState();

    public void UpdateStateHandler()
    {
        StateHandler = State switch
        {
            GameState.Waiting => new WaitingState(),
            GameState.Playing => new PlayingState(),
            GameState.Finished => new FinishedState(),
            _ => new WaitingState()
        };
    }
}