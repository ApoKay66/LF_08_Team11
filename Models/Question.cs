namespace BrainBusters_Grp11_LF08.Models;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    
    // Navigation property representing the relationship
    public List<Answer> Answers { get; set; } = new();
}