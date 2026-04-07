using BrainBusters.Core;
using BrainBusters.DataAccess;
using BrainBusters.Models;
using BrainBuster.Web.Models;

namespace BrainBuster.Web.Services;

public class GameHubService
{
    private readonly QuizDatabase _db;
    private GameSession? _gameSession;
    private TurnViewModel? _activeTurn; // Holds the state for the current turn
    private readonly object _lock = new object();

    public GameHubService(QuizDatabase db)
    {
        _db = db;
    }
    
    public List<string> GetCategories() => _db.GetCategories();

    public void StartNewGame(List<string> playerNames, List<string> categories)
    {
        lock (_lock)
        {
            var players = playerNames.Select(name => _db.GetOrCreatePlayer(name)).ToList();
            
            // 1. Load the unique set of questions
            var uniqueQuestions = _db.LoadQuestions(categories.Count > 0 ? categories : null);
            Console.WriteLine($"[GameHubService] Loaded {uniqueQuestions.Count} unique questions from DB.");

            // 2. Shuffle them to create a random order for this game
            Shuffle(uniqueQuestions);

            // 3. Create the full list, repeating each question for each player before moving to the next
            var gameQuestions = new List<Question>();
            foreach (var question in uniqueQuestions)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    gameQuestions.Add(question);
                }
            }
            Console.WriteLine($"[GameHubService] Created a game with {gameQuestions.Count} total questions for {players.Count} players.");
            
            // 4. Create the session with the expanded list
            _gameSession = new GameSession(players, gameQuestions);
            
            // Prepare the very first turn
            _activeTurn = CreateTurnViewModel(_gameSession);
        }
    }
    
    public void EndGame()
    {
        lock (_lock)
        {
            _gameSession = null;
            _activeTurn = null;
        }
    }

    public GameState GetGameState()
    {
        lock (_lock)
        {
            if (_gameSession == null)
            {
                return new GameState { Status = "Lobby" };
            }

            if (_gameSession.IsGameOver)
            {
                return new GameState { Status = "Summary", Summary = GetSummary(_gameSession) };
            }
            
            return new GameState
            {
                Status = "InProgress",
                CurrentPlayerName = _gameSession.GetCurrentPlayer().Name,
                ActiveTurn = _activeTurn,
                Players = _gameSession.GetPlayers()
            };
        }
    }

    public void SubmitAnswer(int choiceIndex)
    {
        lock (_lock)
        {
            if (_gameSession == null || _gameSession.IsGameOver || _activeTurn?.ShuffledAnswers == null) return;
            
            if (choiceIndex < 0 || choiceIndex >= _activeTurn.ShuffledAnswers.Count) return;

            var selectedAnswer = _activeTurn.ShuffledAnswers[choiceIndex];
            var player = _gameSession.GetCurrentPlayer();

            _gameSession.EvaluateAnswer(player, selectedAnswer);
            _gameSession.ConsumeCurrentQuestion();
            _gameSession.AdvanceTurn();

            if (!_gameSession.IsGameOver)
            {
                // Prepare the next turn
                _activeTurn = CreateTurnViewModel(_gameSession);
            }
            else
            {
                GetSummary(_gameSession);
                _activeTurn = null;
            }
        }
    }
    
    private TurnViewModel? CreateTurnViewModel(GameSession session)
    {
        var question = session.PeekNextQuestion();
        if (question == null) return null;

        return new TurnViewModel
        {
            PlayerName = session.GetCurrentPlayer().Name,
            Question = question,
            ShuffledAnswers = PrepareAnswers(question)
        };
    }
    
    private List<Player> GetSummary(GameSession session)
    {
        var finalScores = session.GetPlayers().OrderByDescending(p => p.Score).ToList();
        foreach (var player in finalScores)
        {
            _db.UpdateHighScore(player);
        }
        return finalScores;
    }

    private List<Answer> PrepareAnswers(Question q)
    {
        var a = new List<Answer>(q.Answers);
        Shuffle(a);
        return a.Take(4).ToList();
    }

    private static void Shuffle<T>(IList<T> list)
    {
        var rng = new Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
