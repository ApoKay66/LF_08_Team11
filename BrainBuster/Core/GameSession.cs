namespace BrainBusters.Core;

using BrainBusters.Models;

public class GameSession
{
    private readonly List<Player> _players;
    private readonly List<Question> _questions;
    private readonly int _roundsTotal;
    private int _roundsFinished = 0;
    private int _currentPlayerIndex = 0;

    public GameSession(List<Player> players, List<Question> questions, int roundsTotal)
    {
        _players = players;
        _questions = questions;
        _roundsTotal = roundsTotal;

        foreach (var player in _players) player.Score = 0;
    }

    public bool IsGameOver => _roundsFinished >= _roundsTotal || _questions.Count == 0;

    public Player GetCurrentPlayer() => _players[_currentPlayerIndex];

    public Question? GetNextQuestion()
    {
        if (_questions.Count == 0) return null;

        var q = _questions[0];
        _questions.RemoveAt(0); // Frage aus dem Pool nehmen
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
            _roundsFinished++;
        }
    }
}
