using BrainBusters.Models;
using BrainBusters.Core;

namespace BrainBuster.Tests;

public class UnitTests
{
    [Fact]
    public void AddScore_ShouldIncreasePlayerScore()
    {
        // ARRANGE
        var player = new Player { Name = "TestPlayer", Score = 0 };

        // ACT
        player.AddScore(10);

        // ASSERT
        Assert.Equal(10, player.Score);
    }
    [Fact]
    public void AddScore_ShouldNotDecreasePlayerScore()
    {
        // ARRANGE
        var player = new Player { Name = "TestPlayer", Score = 10 };

        // ACT
        player.AddScore(-5);

        // ASSERT
        Assert.Equal(10, player.Score); // Score sollte nicht sinken
    }
    [Fact]
    public void GetCurrentPlayer_ShouldReturnCorrectPlayer()
    {
        // ARRANGE
        var players = new List<Player>
        {
            new Player { Name = "Player1" },
            new Player { Name = "Player2" }
        };
        var questions = new List<Question>();
        var gameSession = new GameSession(players, questions, roundsTotal: 3);

        // ACT & ASSERT
        Assert.Equal("Player1", gameSession.GetCurrentPlayer().Name);
        gameSession.AdvanceTurn();
        Assert.Equal("Player2", gameSession.GetCurrentPlayer().Name);
    }
    [Fact]
    public void GetNextQuestion_ShouldReturnNullWhenNoQuestionsLeft()
    {
        // ARRANGE
        var players = new List<Player> { new Player { Name = "Player1" } };
        var questions = new List<Question>(); // Keine Fragen
        var gameSession = new GameSession(players, questions, roundsTotal: 3);

        // ACT
        var question = gameSession.GetNextQuestion();

        // ASSERT
        Assert.Null(question);
    }
    [Fact]
    public void GetNextQuestion_ShouldReturnAndRemoveQuestion()
    {
        // ARRANGE
        var players = new List<Player> { new Player { Name = "Player1" } };
        var question1 = new Question { Id = 1, QuestionText = "Frage 1" };
        var question2 = new Question { Id = 2, QuestionText = "Frage 2" };
        var questions = new List<Question> { question1, question2 };
        var gameSession = new GameSession(players, questions, roundsTotal: 3);

        // ACT
        var firstQuestion = gameSession.GetNextQuestion();
        var secondQuestion = gameSession.GetNextQuestion();
        var noMoreQuestions = gameSession.GetNextQuestion();

        // ASSERT
        Assert.Equal(question1.Id, firstQuestion?.Id);
        Assert.Equal(question2.Id, secondQuestion?.Id);
        Assert.Null(noMoreQuestions); // Alle Fragen sollten verbraucht sein
    }
    [Fact]
    public void EvaluateAnswer_ShouldIncreaseScoreForCorrectAnswer()
    {
        // ARRANGE
        var player = new Player { Name = "TestPlayer", Score = 0 };
        var answer = new Answer { IsCorrect = true };
        var gameSession = new GameSession(new List<Player> { player }, new List<Question>(), roundsTotal: 3);

        // ACT
        gameSession.EvaluateAnswer(player, answer);

        // ASSERT
        Assert.Equal(10, player.Score); // Standardmäßig 10 Punkte für eine richtige Antwort
    }
    [Fact]
    public void IsGameOver_ShouldReturnTrueWhenRoundsFinished()
    {
        // ARRANGE
        var players = new List<Player> { new Player { Name = "Player1" } };
        var questions = new List<Question>();
        var gameSession = new GameSession(players, questions, roundsTotal: 1);

        // ACT
        gameSession.AdvanceTurn(); // Runde 1 beenden

        // ASSERT
        Assert.True(gameSession.IsGameOver);
    }
}