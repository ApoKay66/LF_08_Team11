using Microsoft.Data.Sqlite;
using BrainBusters.Models;

namespace BrainBusters.DataAccess;

public class QuizDatabase : IDisposable
{
    private readonly string _dbPath;
    private readonly SqliteConnection _connection;

    public QuizDatabase(string dbPath)
    {
        _dbPath = dbPath;
        
        // Verbindung einmalig erstellen und öffnen
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
        
        InitializeDatabase();
    }

        public List<string> GetCategories()
    {
        var categories = new List<string>();
        var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT Category FROM Questions";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(reader.GetString(0));
        }
        return categories;
    }

    // The Initial Database consists of only the Questions and Answers
    private void InitializeDatabase()
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Accounts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT UNIQUE,
                HighScore INTEGER DEFAULT 0
            );";
        cmd.ExecuteNonQuery();
    }

public List<Question> LoadQuestions(List<string>? categoryFilters = null)
{
    var questions = new Dictionary<int, Question>();

    var cmd = _connection.CreateCommand();
            if (categoryFilters == null || categoryFilters.Count == 0)
        {
            cmd.CommandText = @"
                SELECT q.Id, q.Category, q.QuestionText,
                        a.Id, a.AnswerText, a.IsCorrect
                FROM Questions q
                LEFT JOIN Answers a ON q.Id = a.QuestionId
                ORDER BY q.Id;
            ";
        }
        else
        {
            // Build parameters for the IN clause: WHERE Category IN (@cat0, @cat1, ...)
            var parameterNames = categoryFilters.Select((_, i) => $"@cat{i}").ToList();
            string inClause = string.Join(", ", parameterNames);
            
            cmd.CommandText = 
                $"SELECT q.Id, q.Category, q.QuestionText, a.Id, a.AnswerText, a.IsCorrect FROM Questions q LEFT JOIN Answers a ON q.Id = a.QuestionId WHERE Category IN ({inClause})";
            
            for (int i = 0; i < categoryFilters.Count; i++)
            {
                cmd.Parameters.AddWithValue(parameterNames[i], categoryFilters[i]);
            }
        }

    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        int questionId = reader.GetInt32(0);
        // Check if the Dictionary already has a Question for that ID, if so we skip this one
        if (!questions.TryGetValue(questionId, out var question))
        {
            question = new Question
            {
                Id = questionId,
                Category = reader.GetString(1),
                QuestionText = reader.GetString(2),
                Answers = new List<Answer>()
            };

            questions.Add(questionId, question);
        }

        // Answers will now be added to the Questions list of Answers, that is if the Question has any Answers in the Database...
        if (!reader.IsDBNull(3))
        {
            var answer = new Answer
            {
                Id = reader.GetInt32(3),
                QuestionId = questionId,
                AnswerText = reader.GetString(4),
                IsCorrect = reader.GetInt32(5) == 1
            };

            question.Answers.Add(answer);
        }
    }

    var result = new List<Question>(questions.Values);
    Console.WriteLine($"[QuizDatabase] Loaded {result.Count} questions from DB (after filtering).");
    return result;
}

    public Player GetOrCreatePlayer(string name)
    {
        var selectCmd = _connection.CreateCommand();
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
        }

        var insertCmd = _connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Accounts (Name, HighScore) VALUES (@name, 0); SELECT last_insert_rowid();";
        insertCmd.Parameters.AddWithValue("@name", name);

        long newId = (long)insertCmd.ExecuteScalar()!;
        return new Player { Id = (int)newId, Name = name, HighScore = 0 };
    }

    public void UpdateHighScore(Player player)
    {
        if (player.Score <= player.HighScore) return;
        
        var cmd = _connection.CreateCommand();
        cmd.CommandText = "UPDATE Accounts SET HighScore = @score WHERE Id = @id";
        cmd.Parameters.AddWithValue("@score", player.Score);
        cmd.Parameters.AddWithValue("@id", player.Id);
        
        cmd.ExecuteNonQuery();
        player.HighScore = player.Score;
    }

    public List<Player> GetTopPlayers(int limit)
{
    var players = new List<Player>();

    var cmd = _connection.CreateCommand();
    cmd.CommandText = @"
        SELECT Id, Name, HighScore
        FROM Accounts
        ORDER BY HighScore DESC, Name ASC
        LIMIT @limit;
    ";

    cmd.Parameters.AddWithValue("@limit", limit);

    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        players.Add(new Player
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            HighScore = reader.GetInt32(2)
        });
    }

    return players;
}

    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}