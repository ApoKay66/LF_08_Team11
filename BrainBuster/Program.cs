using BrainBusters;
using BrainBusters.Models;
using BrainBusters.DataAccess;
using BrainBusters.Services;

class Program
{
    static void Main(string[] args)
    {
        var tmp = TryParseArguments(args)
    }

    static bool TryParseArguments(string[] args)
    {

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-h":
                    ShowHelp();
                    return false;
                case "-s":
                    break;
                default:
                var db = new QuizDatabase(AppConfig.DbPath);
                QuizApp Quiz = new QuizApp(db);

                Quiz.Run();
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