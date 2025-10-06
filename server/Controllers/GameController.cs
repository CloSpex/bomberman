using Microsoft.AspNetCore.Mvc;
using BombermanGame.Facades;

namespace BombermanGame.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameFacade _gameFacade;

    public GameController(IGameFacade gameFacade)
    {
        _gameFacade = gameFacade;
    }

    [HttpPost("room/{roomId}/join")]
    public async Task<IActionResult> JoinRoom(string roomId, [FromBody] JoinRoomRequest request)
    {
        var room = await _gameFacade.CreateAndJoinRoomAsync(roomId, request.PlayerId, request.PlayerName);
        return Ok(new { success = true, room });
    }

    [HttpPost("room/{roomId}/start")]
    public async Task<IActionResult> StartGame(string roomId)
    {
        var success = await _gameFacade.StartGameSessionAsync(roomId);
        return success ? Ok(new { success = true }) : BadRequest(new { success = false, message = "Cannot start game" });
    }

    [HttpPost("room/{roomId}/action")]
    public async Task<IActionResult> PerformAction(string roomId, [FromBody] PlayerActionRequest request)
    {
        var success = await _gameFacade.PerformPlayerActionAsync(roomId, request.PlayerId, request.Action);
        return Ok(new { success });
    }

    [HttpGet("room/{roomId}/status")]
    public async Task<IActionResult> GetRoomStatus(string roomId)
    {
        var status = await _gameFacade.GetRoomStatusAsync(roomId);
        return Ok(status);
    }
}

public class JoinRoomRequest
{
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
}

public class PlayerActionRequest
{
    public string PlayerId { get; set; } = "";
    public PlayerAction Action { get; set; }
}