using System.Net;
using HiveAPI.CS;
using SplinterlandsRObot.Models.Account;
using SplinterlandsRObot.Game;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace SplinterlandsRObot
{
    public static class InstanceManager
    {
        public static List<BotInstance> BotInstances = new();
        public static List<User> userList = new();
        public static List<UserStats> UsersStatistics = new();
        public static Dictionary<string,int> RentingQueue = new();
        public static Dictionary<string,string> FocusRentingQueue = new();//
        public static object StartBattleLock = new();
        public static HttpClient HttpClient = new();
        public static CookieContainer CookieContainer = new();
        public static bool isRentingServiceRunning = false;
        public static bool isStatsSyncRunning = false;
        private static FileSystemWatcher _fsWatcher = new FileSystemWatcher();
        public static Subject<string> _configs = new Subject<string>();
        public static IObservable<string> Configs => _configs.AsObservable();
        public static void CreateUsersInstance()
        {
            userList = new Users().GetUsers();
        }

        public static void CreateBotInstances(List<User> userList)
        {
            int index = 0;
            foreach (User user in userList)
            {
                BotInstances.Add(new BotInstance(user, index));
                index++;
            }
        }
        public static void StartFileWatcher()
        {
            _fsWatcher = new FileSystemWatcher();
            _fsWatcher.Path = Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FOLDER);
            _fsWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fsWatcher.Changed += new FileSystemEventHandler(OnConfigChanged);
            _fsWatcher.Filter = "*.xml";
            _fsWatcher.EnableRaisingEvents = true;
        }

        private static void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            Logs.LogMessage($"Settings in {e.Name} have changed, checking users if config should reload", Logs.LOG_ALERT);
            Thread.Sleep(2000);
            _configs.OnNext(e.Name);
        }
    }
}
