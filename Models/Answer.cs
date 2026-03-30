namespace BrainBusters_Grp11_LF08.Models;

public class Answer
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    
    // Indicates if this is a correct or a wrong answer
    public bool IsCorrect { get; set; }
    
    // Foreign Key linking back to the Question
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
}