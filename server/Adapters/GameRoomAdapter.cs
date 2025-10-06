using BombermanGame.Models;

namespace BombermanGame.Adapters;

public interface IGameDataService
{
    Task<GameRoomData> GetGameDataAsync(string roomId);
    Task SaveGameDataAsync(string roomId, GameRoomData data);
}

public class GameRoomData
{
    public string RoomId { get; set; } = "";
    public List<PlayerData> Players { get; set; } = new();
    public string State { get; set; } = "";
    public DateTime LastUpdate { get; set; }
}

public class PlayerData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsAlive { get; set; }
}

public class GameRoomAdapter : IGameDataService
{
    private readonly IGameRoomRepository _repository;

    public GameRoomAdapter(IGameRoomRepository repository)
    {
        _repository = repository;
    }

    public async Task<GameRoomData> GetGameDataAsync(string roomId)
    {
        var room = await _repository.GetRoomAsync(roomId);
        if (room == null)
            return new GameRoomData();

        return new GameRoomData
        {
            RoomId = room.Id,
            State = room.State.ToString(),
            LastUpdate = room.LastUpdate,
            Players = room.Players.Select(p => new PlayerData
            {
                Id = p.Id,
                Name = p.Name,
                X = p.X,
                Y = p.Y,
                IsAlive = p.IsAlive
            }).ToList()
        };
    }

    public async Task SaveGameDataAsync(string roomId, GameRoomData data)
    {
        var room = new GameRoom
        {
            Id = data.RoomId,
            State = Enum.Parse<GameState>(data.State),
            LastUpdate = data.LastUpdate,
            Players = data.Players.Select(p => new Player
            {
                Id = p.Id,
                Name = p.Name,
                X = p.X,
                Y = p.Y,
                IsAlive = p.IsAlive
            }).ToList()
        };

        await _repository.SaveRoomAsync(room);
    }
}

public interface IGameRoomRepository
{
    Task<GameRoom?> GetRoomAsync(string roomId);
    Task SaveRoomAsync(GameRoom room);
}

public class InMemoryGameRoomRepository : IGameRoomRepository
{
    private readonly Dictionary<string, GameRoom> _rooms = new();

    public Task<GameRoom?> GetRoomAsync(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return Task.FromResult(room);
    }

    public Task SaveRoomAsync(GameRoom room)
    {
        _rooms[room.Id] = room;
        return Task.CompletedTask;
    }
}