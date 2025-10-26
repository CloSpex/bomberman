using Microsoft.AspNetCore.SignalR;
using BombermanGame.Services;
using BombermanGame.Bridges;
using BombermanGame.Singletons;
using BombermanGame.Models;
using System.Linq;

namespace BombermanGame.Hubs;

public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly GameLogger _logger = GameLogger.Instance;

    public GameHub(IGameService gameService)
    {
        _gameService = gameService;
    }

    public async Task JoinRoom(string roomId, string playerName, string gameMode = "standard")
    {
        try
        {
            _logger.LogInfo("Hub", $"Player {playerName} attempting to join room {roomId} with mode {gameMode}");

            var player = new Player
            {
                Id = Context.ConnectionId,
                Name = playerName
            };

            var room = _gameService.GetRoom(roomId);
            if (room == null)
            {
                room = _gameService.CreateRoom(roomId, gameMode);
                _logger.LogInfo("Hub", $"Created new room {roomId} with {gameMode} mode");
            }

            var success = await _gameService.JoinRoomAsync(roomId, player);

            if (success)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

                room = _gameService.GetRoom(roomId);
                if (room != null)
                {
                    var roomResponse = CreateRoomResponse(roomId, room);
                    await Clients.Group(roomId).SendAsync("PlayerJoined", roomResponse);
                    _logger.LogInfo("Hub", $"Player {playerName} successfully joined room {roomId}");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("JoinFailed", "Room is full or game in progress");
                _logger.LogWarning("Hub", $"Player {playerName} failed to join room {roomId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Hub", $"Error in JoinRoom: {ex.Message}");
            await Clients.Caller.SendAsync("JoinFailed", "An error occurred while joining the room");
        }
    }

    public async Task RolePreviews()
    {
        try
        {
            var previews = _gameService.GetPlayerRolePreviews();
            await Clients.Caller.SendAsync("RolePreviews", previews);
        }
        catch (Exception ex)
        {
            _logger.LogError("Hub", $"Error in RolePreviews: {ex.Message}");
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInfo("Hub", $"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            _logger.LogInfo("Hub", $"Client disconnected: {Context.ConnectionId}");

            if (exception != null)
            {
                _logger.LogError("Hub", $"Disconnection error: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError("Hub", $"Error in OnDisconnectedAsync: {ex.Message}");
        }
    }

    public async Task StartGame(string roomId)
    {
        try
        {
            _logger.LogInfo("Hub", $"Starting game in room {roomId}");

            var room = _gameService.GetRoom(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("StartFailed", "Room not found");
                return;
            }

            if (room.Players.Count < 2)
            {
                await Clients.Caller.SendAsync("StartFailed", "Cannot start game - need at least 2 players");
                _logger.LogWarning("Hub", $"Failed to start game in room {roomId} - not enough players");
                return;
            }

            await _gameService.StartGameAsync(roomId);

            var roomResponse = CreateRoomResponse(roomId, room);
            await Clients.Group(roomId).SendAsync("GameStarted", roomResponse);
            _logger.LogInfo("Hub", $"Game started successfully in room {roomId}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Hub", $"Error in StartGame: {ex.Message}");
        }
    }

    public async Task MovePlayer(string roomId, int deltaX, int deltaY)
    {
        try
        {
            var success = await _gameService.MovePlayerAsync(
                roomId,
                Context.ConnectionId,
                deltaX,
                deltaY
            );

            if (success)
            {
                var room = _gameService.GetRoom(roomId);
                if (room != null)
                {
                    var roomResponse = CreateRoomResponse(roomId, room);
                    await Clients.Group(roomId).SendAsync("GameUpdated", roomResponse);
                }
            }
            else
            {
                _logger.LogDebug("Hub", $"Player {Context.ConnectionId} move rejected in room {roomId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Hub", $"Error in MovePlayer: {ex.Message}");
        }
    }

    public async Task PlaceBomb(string roomId)
    {
        try
        {
            var success = await _gameService.PlaceBombAsync(
                roomId,
                Context.ConnectionId
            );

            if (success)
            {
                var room = _gameService.GetRoom(roomId);
                if (room != null)
                {
                    var roomResponse = CreateRoomResponse(roomId, room);
                    await Clients.Group(roomId).SendAsync("GameUpdated", roomResponse);
                    _logger.LogDebug("Hub", $"Bomb placed by player {Context.ConnectionId} in room {roomId}");
                }
            }
            else
            {
                _logger.LogDebug("Hub", $"Bomb placement rejected for player {Context.ConnectionId} in room {roomId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Hub", $"Error in PlaceBomb: {ex.Message}");
        }
    }

    public async Task GetRoomStatus(string roomId)
    {
        try
        {
            var room = _gameService.GetRoom(roomId);

            if (room == null)
            {
                await Clients.Caller.SendAsync("RoomStatus", new
                {
                    roomId = roomId,
                    playerCount = 0,
                    alivePlayerCount = 0,
                    state = "NotFound",
                    canStart = false,
                    isFinished = false
                });
                return;
            }

            var status = new
            {
                roomId = room.Id,
                playerCount = room.Players.Count,
                alivePlayerCount = room.Players.Count(p => p.IsAlive),
                state = room.State.ToString(),
                canStart = room.Players.Count >= 2 && room.State == GameState.Waiting,
                isFinished = room.State == GameState.Finished
            };

            await Clients.Caller.SendAsync("RoomStatus", status);
        }
        catch (Exception ex)
        {
            _logger.LogError("Hub", $"Error in GetRoomStatus: {ex.Message}");
        }
    }

    public async Task ChangeRenderer(string roomId, string rendererType)
    {
        try
        {
            IGameRenderer renderer = rendererType.ToLower() switch
            {
                "json" => new JsonGameRenderer(),
                "text" => new TextGameRenderer(),
                "canvas" => new CanvasGameRenderer(),
                _ => new CanvasGameRenderer()
            };

            _gameService.SetRoomRenderer(roomId, renderer);
            _logger.LogInfo("Hub", $"Renderer changed to {rendererType} for room {roomId}");

            var room = _gameService.GetRoom(roomId);
            if (room != null)
            {
                var roomResponse = CreateRoomResponse(roomId, room);
                await Clients.Group(roomId).SendAsync("RendererChanged", roomResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Hub", $"Error in ChangeRenderer: {ex.Message}");
        }
    }

    private object CreateRoomResponse(string roomId, GameRoom room)
    {
        var renderer = _gameService.GetRoomRenderer(roomId);
        var gameMode = _gameService.GetRoomGameMode(roomId);
        var gameModeDescription = _gameService.GetRoomGameModeDescription(roomId);

        var playersList = room.Players?.Select(p => (object)new
        {
            id = p.Id,
            name = p.Name,
            x = p.X,
            y = p.Y,
            isAlive = p.IsAlive,
            bombCount = p.BombCount,
            bombRange = p.BombRange,
            color = p.Color,
            speed = p.Speed
        }).ToList() ?? new List<object>();

        var boardData = new
        {
            grid = room.Board?.Grid ?? Array.Empty<int[]>(),
            bombs = room.Board?.Bombs?.Select(b => (object)new
            {
                x = b.X,
                y = b.Y,
                playerId = b.PlayerId,
                range = b.Range,
                placedAt = b.PlacedAt
            }).ToList() ?? new List<object>(),
            explosions = room.Board?.Explosions?.Select(e => (object)new
            {
                x = e.X,
                y = e.Y,
                createdAt = e.CreatedAt
            }).ToList() ?? new List<object>(),
            powerUps = room.Board?.PowerUps?.Select(p => (object)new
            {
                x = p.X,
                y = p.Y,
                type = p.Type.ToString()
            }).ToList() ?? new List<object>()
        };

        if (renderer is TextGameRenderer)
        {
            var boardElement = new GameBoardElement(room.Board, renderer);
            var textView = boardElement.Render() + "\n\n";

            textView += "Players:\n";
            foreach (var player in room.Players ?? Enumerable.Empty<Player>())
            {
                var playerElement = new PlayerElement(player, renderer);
                textView += playerElement.Render() + "\n";
            }

            if (room.Board?.Bombs?.Any() == true)
            {
                textView += "\nBombs:\n";
                foreach (var bomb in room.Board.Bombs)
                {
                    var bombElement = new BombElement(bomb, renderer);
                    textView += bombElement.Render() + "\n";
                }
            }

            return new
            {
                id = room.Id,
                players = playersList,
                board = boardData,
                state = room.State.ToString(),
                textView = textView,
                rendererType = "text",
                gameMode = gameMode,
                gameModeDescription = gameModeDescription
            };
        }

        return new
        {
            id = room.Id,
            players = playersList,
            board = boardData,
            state = room.State.ToString(),
            rendererType = renderer?.GetType().Name.Replace("GameRenderer", "").ToLower() ?? "json",
            gameMode = gameMode,
            gameModeDescription = gameModeDescription
        };
    }
}