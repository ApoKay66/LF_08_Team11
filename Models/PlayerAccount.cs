namespace BrainBusters_Grp11_LF08.Models;

// Persistent database model for the account and global stats
public class PlayerAccount
{
    public int Id { get; set; }
    
    public string Gamertag { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // New property for the global leaderboard
    public int GlobalWinStreak { get; set; }
    public int GlobalScore { get; set; }
}