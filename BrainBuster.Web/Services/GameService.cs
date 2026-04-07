using BrainBusters.Core;
using BrainBusters.DataAccess;
using BrainBusters.Models;
using BrainBuster.Web.Models;

namespace BrainBuster.Web.Services;

public class GameService
{
    private readonly QuizDatabase _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession _session
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new InvalidOperationException("HttpContext is null. GameService requires an active HTTP context.");
            }
            if (_httpContextAccessor.HttpContext.Session == null)
            {
                throw new InvalidOperationException("HttpContext.Session is null. GameService requires an active session.");
            }
            return _httpContextAccessor.HttpContext.Session;
        }
    }

    private const string GameSessionKey = "GameSession";
    private const string TurnViewModelKey = "TurnViewModel";

    public GameService(QuizDatabase db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public void StartNewGame(List<string> playerNames, int rounds = 5, string? category = null)
    {
        var players = playerNames.Select(name => _db.GetOrCreatePlayer(name)).ToList();
        var categoryList = category == null ? null : new List<string> { category };
        
        // 1. Load the unique set of questions
        var uniqueQuestions = _db.LoadQuestions(categoryList);
        
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
        
        // 4. Create the session with the expanded list
        var session = new GameSession(players, gameQuestions);
        _session.Set(GameSessionKey, session);
    }

    public GameSession? GetGameSession()
    {
        return _session.Get<GameSession>(GameSessionKey);
    }
    
    public TurnViewModel? StartTurn()
    {
        var game = GetGameSession();
        if (game == null || game.IsGameOver) return null;

        var question = game.GetNextQuestion();
        if (question == null)
        {
            // Should not happen if IsGameOver is checked, but as a safeguard
            return null;
        }

        var turnModel = new TurnViewModel
        {
            PlayerName = game.GetCurrentPlayer().Name,
            Question = question,
            ShuffledAnswers = PrepareAnswers(question)
        };
        
        _session.Set(GameSessionKey, game); // Save session after GetNextQuestion
        _session.Set(TurnViewModelKey, turnModel);

        return turnModel;
    }
    
    public TurnViewModel? GetActiveTurn()
    {
        return _session.Get<TurnViewModel>(TurnViewModelKey);
    }

    public (bool isCorrect, int newScore)? SubmitAnswer(int choiceIndex)
    {
        var game = GetGameSession();
        var turn = GetActiveTurn();

        if (game == null || turn == null || turn.ShuffledAnswers == null || choiceIndex < 0 || choiceIndex >= turn.ShuffledAnswers.Count)
        {
            return null;
        }

        var player = game.GetCurrentPlayer();
        var selectedAnswer = turn.ShuffledAnswers[choiceIndex];

        game.EvaluateAnswer(player, selectedAnswer);
        game.AdvanceTurn();

        _session.Set(GameSessionKey, game);
        _session.Remove(TurnViewModelKey); // Turn is over, remove it

        return (selectedAnswer.IsCorrect, player.Score);
    }

    public List<Player> GetSummary()
    {
        var session = GetGameSession();
        if (session == null) return new List<Player>();

        var finalScores = session.GetPlayers().OrderByDescending(p => p.Score).ToList();
        
        foreach (var player in finalScores)
        {
            _db.UpdateHighScore(player);
        }
        
        return finalScores;
    }
    
    private List<Answer> PrepareAnswers(Question question)
    {
        var answers = new List<Answer>(question.Answers);
        Shuffle(answers);
        return answers.Take(4).ToList();
    }

    private static void Shuffle<T>(IList<T> list)
    {
        var rng = new Random(); // Use a new Random instance for each shuffle
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
