using BombermanGame.Models;
using BombermanGame.Bridges;

namespace BombermanGame.Services;

public interface IGameService
{
    GameRoom CreateRoom(string roomId, string gameMode = "standard");
    GameRoom? GetRoom(string roomId);
    List<GameRoom> GetRooms();

    Task<bool> JoinRoomAsync(string roomId, Player player);
    List<Player> GetPlayerRolePreviews();

    Task StartGameAsync(string roomId);
    Task<bool> MovePlayerAsync(string roomId, string playerId, int deltaX, int deltaY);
    Task<bool> PlaceBombAsync(string roomId, string playerId);

    IGameRenderer? GetRoomRenderer(string roomId);
    void SetRoomRenderer(string roomId, IGameRenderer renderer);

    void SetRoomGameMode(string roomId, string gameMode);
    string GetRoomGameMode(string roomId);
    string GetRoomGameModeDescription(string roomId);
}