using BombermanGame.Models;
using BombermanGame.Bridges;

namespace BombermanGame.Services;

public interface IGameService
{
    GameRoom CreateRoom(string roomId, string theme = "classic");
    GameRoom? GetRoom(string roomId);
    List<GameRoom> GetRooms();

    Task<bool> JoinRoomAsync(string roomId, Player player);
    Task StartGameAsync(string roomId);
    Task<bool> MovePlayerAsync(string roomId, string playerId, int deltaX, int deltaY);
    Task<bool> PlaceBombAsync(string roomId, string playerId);
    void SetRoomBombFactory(string roomId, string factoryType);
    string GetRoomBombFactoryType(string roomId);

    void SetRoomTheme(string roomId, string theme);
    string GetRoomTheme(string roomId);
    IGameRenderer? GetRoomRenderer(string roomId);
    List<Player> GetPlayerRolePreviews();
    void SetRoomRenderer(string roomId, IGameRenderer renderer);
}