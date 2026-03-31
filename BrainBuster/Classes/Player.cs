namespace BrainBusters.Classes;

public class Player
{
    public int Id { get; set; } // Primärschlüssel aus der DB
    public string Name { get; set; } = "Gast";
    public int Score { get; set; } = 0;      // Aktueller Score der Runde
    public int HighScore { get; set; } = 0;  // Bestwert aus der DB
}