using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BrainBusters.Classes;
using BrainBusters.Database;

class QuizService
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

        // 1. Initiales Spieler-Setup
        ManagePlayers();

        while (keepRunning)
        {
            var questions = _db.LoadQuestions();
            var rnd = new Random();
            var selectedQuestions = questions.OrderBy(x => rnd.Next()).Take(_rounds).ToList();

            // Score für die neue Runde zurücksetzen
            foreach (var p in _players) p.Score = 0;

            // 2. Quiz-Schleife
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

                    // --- VALIDIERUNG DER ANTWORT ---
                    int choice = 0;
                    while (true)
                    {
                        string input = Console.ReadLine();
                        if (int.TryParse(input, out choice) && choice > 0 && choice <= shuffledAnswers.Count)
                        {
                            break; // Gültige Zahl eingegeben
                        }
                        
                        // Fehlermeldung in der Konsole nach oben schieben
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.Write(new string(' ', Console.WindowWidth)); // Zeile löschen
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

            // 3. Abschluss-Bildschirm
            Console.Clear();
            Console.WriteLine("=== ENDERGEBNIS ===");
            foreach (var p in _players)
            {
                _db.UpdateHighScore(p);
                Console.WriteLine($"{p.Name}: {p.Score} Punkte (Rekord: {Math.Max(p.Score, p.HighScore)})");
            }

            // 4. After-Game Menü mit eigener Validierung
            bool validMenuInput = false;
            while (!validMenuInput)
            {
                Console.WriteLine("\n---------------------------");
                Console.WriteLine("1: Neustart | 2: Spieler verwalten | 3: Beenden");
                Console.Write("Deine Wahl: ");
                
                string menuChoice = Console.ReadLine();

                if (menuChoice == "1")
                {
                    validMenuInput = true; 
                    // Loop startet von vorn
                }
                else if (menuChoice == "2")
                {
                    ManagePlayers();
                    validMenuInput = true;
                }
                else if (menuChoice == "3")
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
            string input = Console.ReadLine();
            if (int.TryParse(input, out count) && count > 0)
            {
                for (int i = 1; i <= count; i++)
                {
                    Console.Write($"Name für Spieler {i}: ");
                    string name = Console.ReadLine();
                    _players.Add(_db.GetOrCreatePlayer(string.IsNullOrWhiteSpace(name) ? $"Spieler{i}" : name));
                }
            }
            else
            {
                Console.WriteLine("Bitte gib eine gültige Zahl ein.");
            }
        }
    }
}