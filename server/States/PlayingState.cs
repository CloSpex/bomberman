using BombermanGame.Models;

namespace BombermanGame.States;

public class PlayingState : GameStateBase
{
    public override bool CanJoinPlayer(GameRoom room) => false;
    public override bool CanStartGame(GameRoom room) => false;
    public override bool CanMovePlayer(GameRoom room) => true;
    public override bool CanPlaceBomb(GameRoom room) => true;
    public override GameState GetStateType() => GameState.Playing;
}