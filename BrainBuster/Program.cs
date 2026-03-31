using BrainBusters;
using BrainBusters.Classes;
using BrainBusters.Database;
using BrainBusters.Services;

class Program
{
    static void Main(string[] args)
    {
        bool startServer = false;
        int rounds = 5;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-h":
                    ShowHelp();
                    return;
                case "-s":
                    startServer = true;
                    break;
                case "-r":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedRounds) && parsedRounds > 0)
                    {
                        rounds = parsedRounds;
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Ungültiger Wert für -r. Bitte eine positive Zahl angeben.");
                        return;
                    }
                    break;
                default:
                    Console.WriteLine($"Unbekanntes Argument: {args[i]}");
                    ShowHelp();
                    return;
            }
        }

        if (startServer)
        {
            var webServer = new WebServerService();
            webServer.Start();
        }
        else
        {
            // Das 'using' stellt sicher, dass db.Dispose() aufgerufen wird
            using var db = new QuizDatabase(AppConfig.DbPath);
            var quiz = new QuizService(db, rounds);
            quiz.Run();
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("=== BrainBusters CLI Quiz ===");
        Console.WriteLine("-h  Zeigt diese Hilfe an");
        Console.WriteLine("-S  Starte den Webserver");
        Console.WriteLine("-r <Zahl>  Setzt die Rundenzahl im Offline-Modus (Standard: 5)");
    }
}