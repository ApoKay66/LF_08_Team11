using BrainBusters;
using BrainBusters.Models;
using BrainBusters.DataAccess;
using BrainBusters.Services;

class Program
{
    static void Main(string[] args)
    {
        using var db = new QuizDatabase(AppConfig.DbPath);
        QuizApp Quiz = new QuizApp(db);

        Quiz.Run();
    }

    static bool TryParseArguments(string[] args, out (bool StartServer, bool ShowLeaderboard, int Rounds) config)
    {
        config = (false, false, 5);

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-h":
                    ShowHelp();
                    return false;
                case "-s":
                    config.StartServer = true;
                    break;
                default:
                    Console.WriteLine($"Fehler: Unbekanntes Argument {args[i]}");
                    return false;
            }
        }
        return true;
    }
    static void ShowHelp()
    {
        Console.WriteLine("=== BrainBusters CLI Quiz ===");
        Console.WriteLine("-h  Zeigt diese Hilfe an");
        Console.WriteLine("-s  Starte den Webserver");
    }
}