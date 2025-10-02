using BombermanGame.Models;

namespace BombermanGame.States;

public class WaitingState : GameStateBase
{
    public override bool CanJoinPlayer(GameRoom room) => room.Players.Count < 4;
    public override bool CanStartGame(GameRoom room) => room.Players.Count >= 2;
    public override bool CanMovePlayer(GameRoom room) => false;
    public override bool CanPlaceBomb(GameRoom room) => false;
    public override GameState GetStateType() => GameState.Waiting;
}