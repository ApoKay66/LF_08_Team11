using BrainBuster.Web.Hubs;
using BrainBuster.Web.Services;
using BrainBuster.Web.Models;
using BrainBusters.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BrainBuster.Web.Controllers;

[ApiController]
[Route("api/game")]
public class GameController : ControllerBase
{
    private readonly GameHubService _gameService;
    private readonly IHubContext<GameHub> _hubContext;

    public GameController(GameHubService gameService, IHubContext<GameHub> hubContext)
    {
        _gameService = gameService;
        _hubContext = hubContext;
    }

    [HttpGet("state")]
    public ActionResult<GameState> GetGameState()
    {
        return Ok(_gameService.GetGameState());
    }
    
    [HttpGet("categories")]
    public ActionResult<List<string>> GetCategories()
    {
        return Ok(_gameService.GetCategories());
    }

    [HttpGet("highscores")]
    public ActionResult<List<Player>> GetHighscores()
    {
        return Ok(_gameService.GetGlobalHighscores());
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromBody] StartGameRequest request)
    {
        _gameService.StartNewGame(request.PlayerNames, request.Categories);
        await _hubContext.Clients.All.SendAsync("GameStateChanged", _gameService.GetGameState());
        return Ok();
    }
    
    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] AnswerRequest request)
    {
        _gameService.SubmitAnswer(request.ChoiceIndex);
        await _hubContext.Clients.All.SendAsync("GameStateChanged", _gameService.GetGameState());
        return Ok();
    }
    
    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        _gameService.EndGame();
        await _hubContext.Clients.All.SendAsync("GameStateChanged", _gameService.GetGameState());
        return Ok();
    }
}

// --- Request Models ---
public class StartGameRequest
{
    public List<string> PlayerNames { get; set; } = new();
    public List<string> Categories { get; set; } = new();
}

public class AnswerRequest
{
    public int ChoiceIndex { get; set; }
}
