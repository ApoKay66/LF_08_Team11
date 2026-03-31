using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BrainBusters.Classes;    // Damit er Player, Question und Answer findet
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
        // 1. Spieler-Setup
        Console.Write("Wie viele Spieler nehmen teil? ");
        if (int.TryParse(Console.ReadLine(), out int count))
        {
            for (int i = 1; i <= count; i++)
            {
                Console.Write($"Name für Spieler {i}: ");
                string name = Console.ReadLine() ?? $"Spieler{i}";
                _players.Add(_db.GetOrCreatePlayer(name));
            }
        }

        var questions = _db.LoadQuestions();
        var rnd = new Random();
        var selectedQuestions = questions.OrderBy(x => rnd.Next()).Take(_rounds).ToList();

        // 2. Quiz-Schleife
        foreach (var q in selectedQuestions)
        {
            foreach (var currentPlayer in _players) // Jeder Spieler bekommt die Frage
            {
                Console.Clear();
                Console.WriteLine($"--- SPIELER: {currentPlayer.Name} | Aktueller Score: {currentPlayer.Score} ---");
                Console.WriteLine($"\nKategorie: {q.Category}");
                Console.WriteLine(q.QuestionText);

                for (int i = 0; i < q.Answers.Count; i++)
                    Console.WriteLine($"{i + 1}: {q.Answers[i].AnswerText}");

                Console.Write($"{currentPlayer.Name}, deine Antwort: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= q.Answers.Count)
                {
                    if (q.Answers[choice - 1].IsCorrect)
                    {
                        Console.WriteLine("Richtig!");
                        currentPlayer.Score++;
                    }
                    else Console.WriteLine("Falsch!");
                }
                Thread.Sleep(1000); // Kurze Pause zum Lesen
            }
        }

        // 3. Abschluss & Highscore-Update
        Console.WriteLine("\n=== ENDERGEBNIS ===");
        foreach (var p in _players)
        {
            _db.UpdateHighScore(p);
            Console.WriteLine($"{p.Name}: {p.Score} Punkte (Persönlicher Rekord: {Math.Max(p.Score, p.HighScore)})");
        }
    }
}