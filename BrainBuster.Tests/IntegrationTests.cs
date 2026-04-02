using BrainBusters.Models;
using BrainBusters.DataAccess;

namespace BrainBuster.Tests;

public class IntegrationTests
{
    [Fact]
    public void CreatePlayer_ShouldSaveAndRetrieveCorrectPlayerFromDatabase()
    {
        // ARRANGE
        // Wir übergeben nur den Namen. Der Konstruktor in QuizDatabase
        // macht daraus: "Data Source=:memory:"
        string dbPath = ":memory:"; 
        using var db = new QuizDatabase(dbPath);
        
        string testName = "ChefSascha";

        // ACT
        Player createdPlayer = db.GetOrCreatePlayer(testName);

        // ASSERT
        Assert.NotNull(createdPlayer);
        Assert.Equal(testName, createdPlayer.Name);
        Assert.Equal(0, createdPlayer.HighScore);

        Player retrievedPlayer = db.GetOrCreatePlayer(testName);
        Assert.Equal(createdPlayer.Id, retrievedPlayer.Id);
        Assert.Equal(testName, retrievedPlayer.Name);
    }

    [Fact]
    public void UpdateHighScore_ShouldIncreaseScoreInDatabase()
    {
        // ARRANGE
        using var db = new QuizDatabase(":memory:");
        var player = db.GetOrCreatePlayer("TestPlayer");
        player.Score = 10;

        // ACT
        db.UpdateHighScore(player);

        // ASSERT
        var updatedPlayer = db.GetOrCreatePlayer("TestPlayer");
        Assert.Equal(10, updatedPlayer.HighScore);
    }
}