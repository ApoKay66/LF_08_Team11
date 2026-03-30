namespace BrainBusters_Grp11_LF08.Models;

public class GameRoom
{
    public string RoomCode { get; set; } = string.Empty;
    public int TotalRounds { get; set; }
    public int CurrentRound { get; set; }
    public List<Player> Players { get; set; } = new();
    
    // Tracks the current active question
    public Question? CurrentQuestion { get; set; }
}