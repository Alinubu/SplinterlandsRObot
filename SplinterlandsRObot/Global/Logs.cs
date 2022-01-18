using ConsoleTables;

namespace SplinterlandsRObot
{
    public static class Logs
    {
        public static string LOG_INFO = "INFO";
        public static string LOG_SUCCESS = "SUCCESS";
        public static string LOG_ALERT = "ALERT";
        public static string LOG_WARNING = "WARNING";
        public static string[] CONSOLE_TABLE_HEADER = new string[] { "Account", "ECR", "Wins", "Draws", "Losses", "Winrate", "Last DEC Reward", "Total DEC Rewards", "Rating", "CP", "Quest" };
        public static object _lock = new object();

        public static string nickname = "";

        public static void LogMessage(string message, string messageType = "INFO", bool supress = false)
        {
            ConsoleColor messageColor = ConsoleColor.White;
            string messageTimestamp = $"[{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }] ";

            switch (messageType.ToUpper())
            {
                case "INFO":
                    messageColor = ConsoleColor.White;
                    break;
                case "SUCCESS":
                    messageColor = ConsoleColor.DarkGreen;
                    break;
                case "ALERT":
                    messageColor = ConsoleColor.DarkYellow;
                    break;
                case "WARNING":
                    messageColor = ConsoleColor.DarkRed;
                    break;

            }

            if (supress && !Settings.DEBUG_MODE)
            {
                return;
            }

            
            lock(_lock)
            {
                Console.ForegroundColor = messageColor;
                Console.WriteLine(messageTimestamp + "" + message);
            }
        }

        public static void OutputStat()
        {
            var table = new ConsoleTable(CONSOLE_TABLE_HEADER);
            InstanceManager.UsersStatistics.ForEach(u => table.AddRow(u.Account, u.ECR, u.Wins, u.Draws, u.Losses, Math.Round((Convert.ToDouble(u.Wins) / ((u.Wins + u.Draws + u.Losses) == 0 ? Convert.ToDouble(1) : Convert.ToDouble((u.Wins + u.Draws + u.Losses))) * Convert.ToDouble(100)),2).ToString() + "%", u.MatchRewards, Math.Round(Convert.ToDouble(u.TotalRewards),3), u.Rating + "[" + u.RatingChange + "]", u.CollectionPower, u.Quest)));
            table.Configure(o => o.NumberAlignment = Alignment.Right);
            table.Configure(table => table.EnableCount = false);
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.White;
                table.Write(Format.Default);
            }
        }
    }
}
