using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Swift;
using System.Threading;
using BrainBusters.Classes;
using BrainBusters.Database;

namespace BrainBusters.Services;

public class QuizService
{
    private readonly QuizDatabase _db;
    private readonly int _rounds;
    private List<Player> _players = new List<Player>();

    public QuizService(QuizDatabase db, int rounds = 5)
    {
        _db = db;
        _rounds = rounds;
    }

    public void Run()
    {
        bool keepRunning = true;
        ManagePlayers();

        while (keepRunning)
        {
            // Start the interactive selection
            List<string>? selectedCategories = ChooseCategories();

            // Load questions (null means all categories)
            var questions = _db.LoadQuestions(selectedCategories);

            if (questions.Count == 0)
            {
                Console.WriteLine("\nKeine Fragen in den gewählten Kategorien gefunden!");
                Thread.Sleep(2000);
                continue;
            }

            var rnd = new Random();
            var selectedQuestions = questions.OrderBy(x => rnd.Next()).Take(_rounds).ToList();

            foreach (var p in _players) p.Score = 0;

            foreach (var q in selectedQuestions)
            {
                var shuffledAnswers = q.Answers.OrderBy(x => rnd.Next()).ToList();

                foreach (var currentPlayer in _players)
                {
                    Console.Clear();
                    Console.WriteLine($"--- SPIELER: {currentPlayer.Name} | Score: {currentPlayer.Score} ---");
                    Console.WriteLine($"\nKategorie: {q.Category}");
                    Console.WriteLine(q.QuestionText);
                    Console.WriteLine(new string('-', 20));

                    for (int i = 0; i < shuffledAnswers.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}: {shuffledAnswers[i].AnswerText}");
                    }

                    Console.Write($"\n{currentPlayer.Name}, deine Antwort: ");

                    int choice = 0;
                    while (true)
                    {
                        string input = Console.ReadLine() ?? "";
                        if (int.TryParse(input, out choice) && choice > 0 && choice <= shuffledAnswers.Count)
                        {
                            break;
                        }
                        
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Ungültig! Wähle 1-{shuffledAnswers.Count}: ");
                        Console.ResetColor();
                    }

                    if (shuffledAnswers[choice - 1].IsCorrect)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Richtig!");
                        currentPlayer.Score++;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        var correct = shuffledAnswers.FirstOrDefault(a => a.IsCorrect);
                        Console.WriteLine($"Falsch! Richtig war: {correct?.AnswerText}");
                    }
                    Console.ResetColor();
                    Thread.Sleep(1200);
                }
            }

            Console.Clear();
            Console.WriteLine("=== ENDERGEBNIS ===");
            foreach (var p in _players)
            {
                _db.UpdateHighScore(p);
                Console.WriteLine($"{p.Name}: {p.Score} Punkte (Rekord: {Math.Max(p.Score, p.HighScore)})");
            }

            bool validMenuInput = false;
            while (!validMenuInput)
            {
                Console.WriteLine("\n---------------------------");
                Console.WriteLine("1: Neustart | 2: Spieler verwalten | 3: Globales Leaderboard | 4: Beenden");
                Console.Write("Deine Wahl: ");
                
                string menuChoice = Console.ReadLine() ?? "";

                if (menuChoice == "1")
                {
                    validMenuInput = true; 
                }
                else if (menuChoice == "2")
                {
                    ManagePlayers();
                    validMenuInput = true;
                }else if (menuChoice == "3")
                {
                    ShowLeaderboard();
                }
                else if (menuChoice == "4")
                {
                    validMenuInput = true;
                    keepRunning = false;
                    Console.WriteLine("Programm wird beendet...");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Bitte nur 1, 2 oder 3 eingeben!");
                    Console.ResetColor();
                }
            }
        }
    }

    private void ManagePlayers()
    {
        Console.Clear();
        Console.WriteLine("=== SPIELER VERWALTEN ===");
        _players.Clear();
        
        int count = 0;
        while (count <= 0)
        {
            Console.Write("Wie viele Spieler nehmen teil? ");
            string input = Console.ReadLine() ?? "";
            if (int.TryParse(input, out count) && count > 0)
            {
                for (int i = 1; i <= count; i++)
                {
                    Console.Write($"Name für Spieler {i}: ");
                    string name = Console.ReadLine() ?? "";
                    _players.Add(_db.GetOrCreatePlayer(string.IsNullOrWhiteSpace(name) ? $"Spieler{i}" : name));
                }
            }
            else
            {
                Console.WriteLine("Bitte gib eine gültige Zahl ein.");
            }
        }
    }
        private List<string>? ChooseCategories()
    {
        var allCategories = _db.GetCategories();
        if (allCategories.Count == 0) return null;

        var selectedNames = new List<string>();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== KATEGORIEN WÄHLEN ===");
            
            // Header display
            Console.ForegroundColor = ConsoleColor.Cyan;
            string selectionDisplay = selectedNames.Count > 0 ? string.Join(", ", selectedNames) : "Nichts (Alle)";
            Console.WriteLine($"Aktuelle Auswahl: [{selectionDisplay}]");
            Console.ResetColor();
            Console.WriteLine(new string('-', 30));
            
            // List categories with selection marks
            for (int i = 0; i < allCategories.Count; i++)
            {
                if (selectedNames.Contains(allCategories[i]))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{i + 1}: [X] {allCategories[i]}");
                }
                else
                {
                    Console.WriteLine($"{i + 1}: [ ] {allCategories[i]}");
                }
                Console.ResetColor();
            }
            
            // The "All" option as requested
            Console.WriteLine($"{allCategories.Count + 1}: [ALLE KATEGORIEN]");
            Console.WriteLine(new string('-', 30));
            Console.WriteLine("Wähle Nummern zum Hinzufügen/Entfernen.");
            Console.Write("Oder drücke ENTER zum Starten: ");

            string input = Console.ReadLine() ?? "";

            // Empty Enter: Start with current selection
            if (string.IsNullOrWhiteSpace(input))
            {
                return selectedNames.Count > 0 ? selectedNames : null;
            }

            if (int.TryParse(input, out int choice))
            {
                // If user chooses "All Categories", return null immediately to start
                if (choice == allCategories.Count + 1)
                {
                    return null;
                }

                // Standard Toggle Logic
                if (choice >= 1 && choice <= allCategories.Count)
                {
                    string catName = allCategories[choice - 1];
                    
                    if (selectedNames.Contains(catName))
                    {
                        selectedNames.Remove(catName);
                    }
                    else
                    {
                        selectedNames.Add(catName);
                    }
                    continue; 
                }
            }
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Ungültige Wahl!");
            Console.ResetColor();
            Thread.Sleep(600);
        }
    }
    public void ShowLeaderboard()
    {
        var leaderboard = _db.GetTopPlayers(10);

        Console.WriteLine();
        Console.WriteLine("=== GLOBALE RANGLISTE ===");

        if (leaderboard.Count == 0)
        {
            Console.WriteLine("Noch keine Einträge vorhanden.");
            return;
        }

        for (int i = 0; i < leaderboard.Count; i++)
        {
            var player = leaderboard[i];
            Console.WriteLine($"{i + 1}. {player.Name} - {player.HighScore} Punkte");
        }
    }
}