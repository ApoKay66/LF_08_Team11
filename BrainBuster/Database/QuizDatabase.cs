using Microsoft.Data.Sqlite;
using BrainBusters.Classes;
using System.Collections.Generic;
using System.IO;
using System;

namespace BrainBusters.Database;

class QuizDatabase
{
    private readonly string _dbPath;

    public QuizDatabase(string dbPath)
    {
        // Setzt den Pfad zur Datenbank im Ausführungsverzeichnis
        _dbPath = dbPath;
        
        // Erstellt fehlende Tabellen (wie 'Players') automatisch beim Start
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Accounts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT UNIQUE,
                HighScore INTEGER DEFAULT 0
            );";
        cmd.ExecuteNonQuery();
    }

    public List<Question> LoadQuestions()
    {
        var questions = new List<Question>();
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        
        Console.WriteLine($"DB Path: {_dbPath}");
        Console.WriteLine(File.Exists(_dbPath) ? "✅ File exists" : "❌ File NOT found");
        
        conn.Open();

        // 1. Fragen laden
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Category, QuestionText FROM Questions";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            questions.Add(new Question
            {
                Id = reader.GetInt32(0),
                Category = reader.GetString(1),
                QuestionText = reader.GetString(2)
            });
        }
        reader.Close(); // Reader explizit schließen vor dem nächsten Schritt

        // 2. Antworten für jede Frage laden
        foreach (var q in questions)
        {
            var ansCmd = conn.CreateCommand();
            ansCmd.CommandText = "SELECT Id, AnswerText, IsCorrect FROM Answers WHERE QuestionId=@qid";
            ansCmd.Parameters.AddWithValue("@qid", q.Id);

            using var ansReader = ansCmd.ExecuteReader();
            while (ansReader.Read())
            {
                q.Answers.Add(new Answer
                {
                    Id = ansReader.GetInt32(0),
                    QuestionId = q.Id,
                    AnswerText = ansReader.GetString(1),
                    IsCorrect = ansReader.GetInt32(2) == 1
                });
            }
        }

        return questions;
    }

    public Player GetOrCreatePlayer(string name)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();

        // Versuchen, den Spieler zu finden
        var selectCmd = conn.CreateCommand();
        selectCmd.CommandText = "SELECT Id, Name, HighScore FROM Accounts WHERE Name = @name";
        selectCmd.Parameters.AddWithValue("@name", name);

        using (var reader = selectCmd.ExecuteReader())
        {
            if (reader.Read())
            {
                return new Player
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HighScore = reader.GetInt32(2)
                };
            }
        } // Reader wird hier automatisch geschlossen

        // Wenn nicht gefunden: Neu anlegen
        var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Accounts (Name, HighScore) VALUES (@name, 0); SELECT last_insert_rowid();";
        insertCmd.Parameters.AddWithValue("@name", name);

        long newId = (long)insertCmd.ExecuteScalar()!;
        return new Player { Id = (int)newId, Name = name, HighScore = 0 };
    }

    public void UpdateHighScore(Player player)
    {
        // Wir aktualisieren nur, wenn der aktuelle Score höher als der bisherige HighScore ist
        if (player.Score <= player.HighScore) return;

        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Accounts SET HighScore = @score WHERE Id = @id";
        cmd.Parameters.AddWithValue("@score", player.Score);
        cmd.Parameters.AddWithValue("@id", player.Id);
        
        cmd.ExecuteNonQuery();
        
        // Den HighScore im Objekt auch aktualisieren, damit die Anzeige zum Schluss stimmt
        player.HighScore = player.Score;
    }
}