using Microsoft.EntityFrameworkCore;
using BrainBusters_Grp11_LF08.Models;

namespace BrainBusters_Grp11_LF08.Data;

// Manages database operations and table configurations
public class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
    }

    // Represents the Questions table in the SQLite database
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set;}
    public DbSet<PlayerAccount> Accounts { get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed the first question
        modelBuilder.Entity<Question>().HasData(
            new Question 
            { 
                Id = 1, 
                Text = "What is the capital of Australia?", 
                DifficultyLevel = 2 
            }
        );

        // Seed multiple answers and link them to QuestionId 1
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = 1, QuestionId = 1, Text = "Canberra", IsCorrect = true },
            new Answer { Id = 2, QuestionId = 1, Text = "Sydney", IsCorrect = false },
            new Answer { Id = 3, QuestionId = 1, Text = "Melbourne", IsCorrect = false },
            new Answer { Id = 4, QuestionId = 1, Text = "Brisbane", IsCorrect = false },
            new Answer { Id = 5, QuestionId = 1, Text = "Perth", IsCorrect = false }
        );
    }
}