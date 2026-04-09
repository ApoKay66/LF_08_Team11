using BrainBusters.Models;
using System.Collections.Generic;

namespace BrainBuster.Web.Models;

public class GameState
{
    public string Status { get; set; } = "Lobby";
    public string? CurrentPlayerName { get; set; }
    public List<Player> Players { get; set; } = new();
    public TurnViewModel? ActiveTurn { get; set; }
    public List<Player>? Summary { get; set; }
}
