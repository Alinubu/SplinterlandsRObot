using System.Net;
using HiveAPI.CS;
using SplinterlandsRObot.Constructors;
using SplinterlandsRObot.Game;

namespace SplinterlandsRObot
{
    public static class InstanceManager
    {
        public static List<BotInstance> BotInstances;
        public static List<UserStats> UsersStatistics;
        public static object StartBattleLock = new();
        public static HttpClient HttpClient = new();
        public static CookieContainer CookieContainer = new();
        public static CHived oHived;
        public static void CreateUsersInstance()
        {
            Users users = new Users();
        }

        public static void CreateBotInstances(List<User> userList)
        {
            BotInstances = new List<BotInstance>();
            UsersStatistics = new List<UserStats>();
            oHived = new CHived(HttpClient, Constants.HIVE_NODE);
            int index = 0;
            foreach (User user in userList)
            {
                BotInstances.Add(new BotInstance(user, index));
                index++;
            }
        }
    }
}
