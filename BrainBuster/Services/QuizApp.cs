using BrainBusters.Models;
using BrainBusters.DataAccess;
using BrainBusters.UI;
using BrainBusters.Core;

namespace BrainBusters.Services;

public class QuizApp
{
    private readonly QuizDatabase _db;
    private readonly ConsoleService _ui;
    private List<Player> _activePlayers = new();
    private readonly int _roundsPerGame;
    private readonly int _maxPlayers = 4; 

    public QuizApp(QuizDatabase db, int rounds = 1)
    {
        _db = db;
        _ui = new ConsoleService();
        _roundsPerGame = rounds;
    }

    public void Run()
    {
        while (true)
        {
            _ui.ClearScreen();
            _ui.ShowMessage("====== HAUPTMENÜ ======");
            int input = _ui.PromptNumber("1: Spiel Starten | 2: Spieler verwalten | 3: Leaderboard | 4: Beenden", 1, 4);
            
            switch (input)
            {
                case 1: StartNewGame(); break;
                case 2: ManagePlayers(); break;
                case 3: ShowLeaderboard(); break;
                case 4: return;
            }
        }
    }

    private void StartNewGame()
    {
        if (_activePlayers.Count == 0)
        {
            _ui.ShowError("Bitte erst Spieler im Menü anlegen!");
            return;
        }

        var categories = _db.GetCategories();
        var selectedCategories = _ui.AskPlayerCategories(categories);
        var questions = _db.LoadQuestions(selectedCategories);
        Shuffle(questions);
        // Mischen der Fragen...
        
        var session = new GameSession(_activePlayers, questions, _roundsPerGame);

        // Die Game-Loop
        while (!session.IsGameOver)
        {
            var player = session.GetCurrentPlayer();
            var question = session.GetNextQuestion();

            if (question == null) break;

            // UI Logik für den Zug
            _ui.ClearScreen();
            _ui.ShowMessage($"[{player.Name} ist am Zug]");
            
            // Antworten mischen und aufbereiten (Logik aus deinem alten Code)
            var answers = PrepareAnswers(question);
            
            for (int i = 0; i < answers.Count; i++)
            {
                _ui.ShowMessage($"{i + 1}: {answers[i].AnswerText}");
            }

            int choice = _ui.PromptNumber(question.QuestionText, 1, answers.Count);
            var selectedAnswer = answers[choice - 1];

            session.EvaluateAnswer(player, selectedAnswer);

            _ui.ShowMessage(selectedAnswer.IsCorrect ? "Korrekt!" : "Falsch!");
            Thread.Sleep(2000);

            session.AdvanceTurn();
        }

        ShowSummary();
    }
    private void ManagePlayers()
    {
        _activePlayers.Clear();
        int choice = _ui.PromptNumber("Wie viele Spieler werden Spielen?", 1, _maxPlayers);

        for (int i = 0; i < choice; i++)
        {
            string name = _ui.PromptText($"Name für Spieler {i+1}: ");
            if (!name.IsWhiteSpace())
            {
                _activePlayers.Add(_db.GetOrCreatePlayer(name));
            }
        }
        _ui.ShowMessage("====== Alle Spieler ======");
        for (int i = 0; i < _activePlayers.Count; i++)
        {
            _ui.ShowMessage(_activePlayers[i].Name);
        }
    }
    private void ShowLeaderboard()
    {
        List<Player> topPlayers = _db.GetTopPlayers(10);
        if (topPlayers == null)
        {
            _ui.ShowError("Keine Spieler in der Bestenliste!\nMöglicherweise gibt es noch keine Spieler?");
            return;
        }

        _ui.ShowMessage($"Besten {topPlayers.Count} Spieler:");
        for (int i = 0; i < topPlayers.Count; i++)
        {
            _ui.ShowMessage($"{i+1}. {topPlayers[i].Name} - {topPlayers[i].HighScore} Punkte ");
        }
        string input = string.Empty;
        while (input != "q")
        {
            input = _ui.PromptText("\nQ - Zurück zum Hauptmenü\n");
        }
    }
    private void ShowSummary()
    {
        _ui.ShowMessage("====== Ergebnis ======");

        var tmp = _activePlayers.OrderByDescending(x => x.Score).ToList();
        for (int i = 0; i < tmp.Count; i++)
        {
            _db.UpdateHighScore(tmp[i]);
            _ui.ShowMessage($"{i+1}. {tmp[i].Name} - {tmp[i].Score} Punkte");
        }
        _ui.ShowMessage("======================");
        string input = string.Empty;
        while (input != "q")
        {
            input = _ui.PromptText("\nQ - Zurück zum Hauptmenü\n");
        }
    }
    private static void Shuffle<T>(IList<T> list)
    {
        var rng = Random.Shared;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    private List<Answer> PrepareAnswers(Question question)
    {
        var rng = Random.Shared;

        var correctAnswer = question.Answers.First(x => x.IsCorrect);

        var wrongAnswers = question.Answers
            .Where(x => !x.IsCorrect)
            .OrderBy(x => rng.Next()) // acceptable here for small sets
            .Take(3)
            .ToList();

        var result = new List<Answer>(wrongAnswers)
        {
            correctAnswer
        };

        Shuffle(result);

        return result;
    }
    // PrepareAnswers, ManagePlayers, ShowSummary und ShowLeaderboard implementieren...
}