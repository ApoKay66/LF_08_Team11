namespace BrainBusters.Classes;

class Question
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public List<Answer> Answers { get; set; } = new List<Answer>();
}