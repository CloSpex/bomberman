using BombermanGame.Models;

namespace BombermanGame.States;

public abstract class GameStateBase
{
    public abstract bool CanJoinPlayer(GameRoom room);
    public abstract bool CanStartGame(GameRoom room);
    public abstract bool CanMovePlayer(GameRoom room);
    public abstract bool CanPlaceBomb(GameRoom room);
    public abstract GameState GetStateType();
}