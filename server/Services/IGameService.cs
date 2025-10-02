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
    List<GameRoom> GetRooms();
}