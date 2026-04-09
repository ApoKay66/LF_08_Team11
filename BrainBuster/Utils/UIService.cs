<<<<<<< HEAD
using System.ComponentModel;
using System.Reflection.Metadata;
using Microsoft.VisualBasic;
=======
using BrainBusters.Models;
>>>>>>> d0b1b41 (added things)

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
    public List<string> PlayerSelection(int maxPlayers)
    {
        var players = new List<string>();
        int choice = PromptNumber("Wie viele Spieler werden Spielen?", 1, maxPlayers);

        for (int i = 0; i < choice; i++)
        {
            string name = PromptText($"Name für Spieler {i+1}: ");
            if (!name.IsWhiteSpace())
            {
                players.Add(name);
            }
        }
        return players;
    }
    public void ShowLeaderboard(List<Player> topPlayers)
    {
        if (topPlayers == null)
        {
            ShowError("Keine Spieler in der Bestenliste!\nMöglicherweise gibt es noch keine Spieler?");
            return;
        }

        ShowMessage($"Besten {topPlayers.Count} Spieler:");
        for (int i = 0; i < topPlayers.Count; i++)
        {
            ShowMessage($"{i+1}. {topPlayers[i].Name} - {topPlayers[i].HighScore} Punkte ");
        }
        string input = string.Empty;
        while (input != "q")
        {
            input = PromptText("\nQ - Zurück zum Hauptmenü\n");
        }
    }

    // Prompt the Player to select the Categories for the Quiz, if none are Selected return NULL, all Categories will be shown
    public List<string> AskPlayerCategories(List<string> categories)
    {
        List<string> selectedCategories = new List<string>();
    
        while (true)
        {
            Console.Clear();
            Console.WriteLine("--- Kategorie-Auswahl ---");
        
            for (int i = 0; i < categories.Count; i++)
            {
                string status = selectedCategories.Contains(categories[i]) ? "[X]" : "[ ]";
                Console.WriteLine($"{i + 1}) {status} {categories[i]}");
            }

            Console.WriteLine("\nBereits gewählt: " + (selectedCategories.Count > 0 ? string.Join(", ", selectedCategories) : "Keine"));
        
            int choice = PromptNumber("\nWähle eine Nummer zum Hinzufügen/Entfernen (0 zum Starten):", 0, categories.Count);

            if (choice == 0) break;

            string picked = categories[choice - 1];

            if (selectedCategories.Contains(picked))
            {
                selectedCategories.Remove(picked);
            }
            else
            {
                selectedCategories.Add(picked);
            }
        }

        return selectedCategories.Count > 0 ? selectedCategories : null;
    }

    public string PromptText(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    public void ShowSummary(List<Player> players)
    {
        ShowMessage("====== Ergebnis ======");

        var tmp = players.OrderByDescending(x => x.Score).ToList();
        for (int i = 0; i < tmp.Count; i++)
        {
            ShowMessage($"{i+1}. {tmp[i].Name} - {tmp[i].Score} Punkte");
        }
        ShowMessage("======================");
        string input = string.Empty;
        while (input != "q")
        {
            input = PromptText("\nQ - Zurück zum Hauptmenü\n");
        }
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