using BrainBusters;
using BrainBusters.Classes;
using BrainBusters.Database;
using BrainBusters.Services;

class Program
{
    static void Main(string[] args)
    {
        using var db = new QuizDatabase(AppConfig.DbPath);
        QuizService Quiz = new QuizService(db, 5);

        if (!TryParseArguments(args, out var config)) return;

        if (config.StartServer)
        {
            new WebServerService().Start();
        }
        if (config.ShowLeaderboard)
        {
            Quiz.ShowLeaderboard();
        }
        if (!config.ShowLeaderboard && !config.StartServer)
        {
            Quiz.Run();
        }
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
                case "-r" when i + 1 < args.Length && int.TryParse(args[i + 1], out int r) && r > 0:
                    config.Rounds = r;
                    i++;
                    break;
                case "-lb":
                    //TODO:Show Leaderboard
                    config.ShowLeaderboard = true;
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
        Console.WriteLine("-S  Starte den Webserver");
        Console.WriteLine("-r <Zahl>  Setzt die Rundenzahl im Offline-Modus (Standard: 5)");
    }
}