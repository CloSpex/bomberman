using BombermanGame.Bridges;
using BombermanGame.Models;

namespace BombermanGame.Services;

public interface IGameService
{
    GameRoom CreateRoom(string roomId);
    GameRoom? GetRoom(string roomId);
    Task<bool> JoinRoomAsync(string roomId, Player player);
    Task StartGameAsync(string roomId);
    Task<bool> MovePlayerAsync(string roomId, string playerId, int deltaX, int deltaY);
    Task<bool> PlaceBombAsync(string roomId, string playerId);
    IGameRenderer? GetRoomRenderer(string roomId);
    void SetRoomRenderer(string roomId, IGameRenderer renderer);
    List<GameRoom> GetRooms();
}