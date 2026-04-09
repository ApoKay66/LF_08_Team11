namespace BrainBusters.Core;
using BrainBusters.Models;
using System.Linq;

public class GameSession
{
    // --- Fields for both modes ---
    private readonly List<Player> _players;
    private int _currentPlayerIndex = 0;

    // --- Logic for old console app (round-based) ---
    private readonly int _roundsTotal = -1; // Sentinel value
    private int _roundsFinished = 0;

    // --- Logic for new web app (question-count-based) ---
    private readonly int _initialQuestionCountPerPlayer = -1; // Sentinel value
    private Dictionary<Player, Queue<Question>> _playerQuestionQueues;

    /// <summary>
    /// New constructor for the Web App. Game ends when all players have answered all questions.
    /// </summary>
    public GameSession(List<Player> players, List<Question> uniqueQuestions)
    {
        _players = players;
        _initialQuestionCountPerPlayer = uniqueQuestions.Count;
        _playerQuestionQueues = new Dictionary<Player, Queue<Question>>();

        foreach (var player in _players)
        {
            var playerQuestions = new List<Question>(uniqueQuestions);
            Shuffle(playerQuestions); // Shuffle questions individually for each player
            _playerQuestionQueues.Add(player, new Queue<Question>(playerQuestions));
            player.Score = 0;
        }
    }

    /// <summary>
    /// Old constructor for the Console App. Game ends after a specific number of rounds.
    /// This constructor is now adapted to use the new question queuing system, but for a single shared queue.
    /// </summary>
    [System.Obsolete("This constructor is for console app compatibility. It uses a shared queue for questions.")]
    public GameSession(List<Player> players, List<Question> questions, int roundsTotal)
    {
        _players = players;
        _roundsTotal = roundsTotal;
        
        _initialQuestionCountPerPlayer = questions.Count; // For compatibility
        _playerQuestionQueues = new Dictionary<Player, Queue<Question>>();
        
        // For console app, all players share the same queue, conceptually
        var sharedQuestions = new List<Question>(questions);
        Shuffle(sharedQuestions); // Shuffle once for the shared pool

        foreach (var player in _players)
        {
            // Each player gets a reference to the same shared queue (not ideal but for compatibility)
            // Or, more accurately, we create a single queue that players will draw from sequentially
            // For the purpose of "rounds", we'll just create a single queue for the first player
            // and have other methods draw from it. This constructor is deprecated anyway.
            _playerQuestionQueues.Add(player, new Queue<Question>(sharedQuestions));
            player.Score = 0;
        }
    }

    public bool IsGameOver 
    {
        get 
        {
            // Game is over if all player queues are empty
            return _playerQuestionQueues.Values.All(q => q.Count == 0);
        }
    }

    public Player GetCurrentPlayer() => _players[_currentPlayerIndex];
    public List<Player> GetPlayers() => _players;

    // --- Methods for Web App ---
    public Question? PeekNextQuestion()
    {
        var currentPlayerQueue = _playerQuestionQueues[GetCurrentPlayer()];
        return currentPlayerQueue.Count > 0 ? currentPlayerQueue.Peek() : null;
    }

    public void ConsumeCurrentQuestion()
    {
        var currentPlayerQueue = _playerQuestionQueues[GetCurrentPlayer()];
        if (currentPlayerQueue.Count > 0)
        {
            currentPlayerQueue.Dequeue();
        }
    }

    // --- Method for Console App ---
    // This method now also uses the current player's queue.
    public Question? GetNextQuestion()
    {
        var currentPlayerQueue = _playerQuestionQueues[GetCurrentPlayer()];
        if (currentPlayerQueue.Count == 0) return null;
        return currentPlayerQueue.Dequeue();
    }

    public void EvaluateAnswer(Player player, Answer selectedAnswer, int scoreGain = 10)
    {
        if (selectedAnswer.IsCorrect)
        {
            player.AddScore(scoreGain);
        }
    }

    public void AdvanceTurn()
    {
        _currentPlayerIndex++;
        if (_currentPlayerIndex >= _players.Count)
        {
            _currentPlayerIndex = 0;
            // This part is for the old round-based logic (now less relevant with individual queues)
            if (_roundsTotal != -1)
            {
                _roundsFinished++;
            }
        }
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