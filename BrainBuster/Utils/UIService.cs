namespace BrainBusters.UI;

public class ConsoleService
{
    public int PromptNumber(string prompt, int min, int max)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            Console.Write($"Deine Wahl ({min}-{max}): ");

            if (int.TryParse(Console.ReadLine(), out var value) && value >= min && value <= max)
            {
                return value;
            }

            ShowError("Ungültige Eingabe, bitte eine gültige Zahl eingeben.");
        }
    }

    public string PromptText(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    public void ShowMessage(string message) => Console.WriteLine(message);
    public void ShowError(string error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ResetColor();
        Thread.Sleep(1000);
        Console.Clear();
    }
    
    public void ClearScreen() => Console.Clear();
}