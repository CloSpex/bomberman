using BombermanGame.Models;

namespace BombermanGame.States;

public class FinishedState : GameStateBase
{
    public override bool CanJoinPlayer(GameRoom room) => false;
    public override bool CanStartGame(GameRoom room) => false;
    public override bool CanMovePlayer(GameRoom room) => false;
    public override bool CanPlaceBomb(GameRoom room) => false;
    public override GameState GetStateType() => GameState.Finished;
}