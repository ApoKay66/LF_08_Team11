namespace BrainBusters_Grp11_LF08.Models;

// Temporary in-memory model for an active game session
public class Player
{
    // Required to send real-time messages to this specific user
    public string ConnectionId { get; set; } = string.Empty;
    
    // Link to the database account
    public int AccountId { get; set; }
    public string Gamertag { get; set; } = string.Empty;
    
    // Score for the current active match only
    public int CurrentMatchScore { get; set; }
}