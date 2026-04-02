namespace BrainBusters
{
    static class AppConfig
    {
        public static string DbPath = Path.Combine(AppContext.BaseDirectory, "Data", "quiz.db");
        public static int ServerPort = 5000;
    }
}
