namespace BrainBusters.Core;
using BrainBusters.Models;

public class GameSession
{
    // --- Fields for both modes ---
    private readonly List<Player> _players;
    private List<Question> _questions;
    private int _currentPlayerIndex = 0;

    // --- Logic for old console app (round-based) ---
    private readonly int _roundsTotal = -1; // Sentinel value
    private int _roundsFinished = 0;

    // --- Logic for new web app (question-count-based) ---
    private readonly int _initialQuestionCount = -1; // Sentinel value
    private int _questionsPlayed = 0;

    /// <summary>
    /// New constructor for the Web App. Game ends when all questions are played.
    /// </summary>
    public GameSession(List<Player> players, List<Question> questions)
    {
        _players = players;
        _questions = questions;
        _initialQuestionCount = questions.Count;

        foreach (var player in _players) player.Score = 0;
    }

    /// <summary>
    /// Old constructor for the Console App. Game ends after a specific number of rounds.
    /// </summary>
    [System.Obsolete("This constructor is for console app compatibility.")]
    public GameSession(List<Player> players, List<Question> questions, int roundsTotal)
    {
        _players = players;
        _questions = questions;
        _roundsTotal = roundsTotal;

        foreach (var player in _players) player.Score = 0;
    }

    public bool IsGameOver 
    {
        get 
        {
            // Use new logic if _initialQuestionCount was set, otherwise use old logic
            if (_initialQuestionCount != -1)
            {
                return _questionsPlayed >= _initialQuestionCount || _questions.Count == 0;
            }
            return _roundsFinished >= _roundsTotal || _questions.Count == 0;
        }
    }

    public Player GetCurrentPlayer() => _players[_currentPlayerIndex];
    public List<Player> GetPlayers() => _players;
    public List<Question> GetQuestions() => _questions;
    public void SetQuestions(List<Question> questions) => _questions = questions;

    // --- Methods for Web App ---
    public Question? PeekNextQuestion()
    {
        return _questions.Count > 0 ? _questions[0] : null;
    }

    public void ConsumeCurrentQuestion()
    {
        if (_questions.Count > 0)
        {
            _questions.RemoveAt(0);
            _questionsPlayed++;
        }
    }

    // --- Method for Console App ---
    public Question? GetNextQuestion()
    {
        if (_questions.Count == 0) return null;
        var q = _questions[0];
        _questions.RemoveAt(0);
        _questionsPlayed++; // Also increment for console version
        return q;
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
            // This part is for the old round-based logic
            if (_roundsTotal != -1)
            {
                _roundsFinished++;
            }
        }
    }
}