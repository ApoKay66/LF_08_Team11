namespace BrainBusters.Utils;

static class ConsoleHelper
{
    public static void WriteLineColor(string text, ConsoleColor color)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = prev;
    }
}