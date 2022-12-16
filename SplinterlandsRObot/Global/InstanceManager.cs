using System.Net;
using SplinterlandsRObot.Player;
using SplinterlandsRObot.Game;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using SplinterlandsRObot.Extensions;
using SplinterlandsRObot.API;

namespace SplinterlandsRObot
{
    public static class InstanceManager
    {
        private static readonly object _TaskLock = new();
        private static object _SleepInfoLock = new();
        public static string identifier;
        public static List<BotInstance> BotInstances = new();
        public static List<User> userList = new();
        public static List<UserStats> UsersStatistics = new();
        public static Dictionary<string, int> RentingQueue = new();
        public static Dictionary<string, BotInstance> DecTransferQueue = new();
        public static object StartBattleLock = new();
        public static HttpClient HttpClient = new();
        public static CookieContainer CookieContainer = new();
        public static bool isRentingServiceRunning = false;
        public static bool isRenewRentingServiceRunning = false;
        public static bool isStatsSyncRunning = false;
        public static bool isDecDistributorRunning = false;
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
            Logs.LogMessage($"Settings in {e.Name} have changed, checking users if config should reload", Logs.LOG_ALERT, supress: true);
            Thread.Sleep(2000);
            _configs.OnNext(e.Name);
        }

        public static async Task StartBot(CancellationToken token)
        {
            var instances = new HashSet<Task>();
            int nextBotInstance = -1;
            bool firstRuntrough = true;
            DateTime lastStatsOutput = DateTime.MinValue;
            DateTime[] sleepInfo = new DateTime[InstanceManager.BotInstances.Count];
            Random rnd = new();
            Logs.LogMessage("Starting Bot");

            while (!token.IsCancellationRequested)
            {
                lock (_TaskLock)
                {
                    if (!isRentingServiceRunning)
                    {
                        _ = Task.Run(async () => await RentProcess.StartRentingProcess(token).ConfigureAwait(false));
                        isRentingServiceRunning = true;
                        Task.Delay(1500);
                    }

                    if (!isRenewRentingServiceRunning)
                    {
                        _ = Task.Run(async () => await RentProcess.RenewRentals()).ConfigureAwait(false);
                        isRenewRentingServiceRunning = true;
                        Task.Delay(1500);
                    }

                    if (!isStatsSyncRunning)
                    {
                        _ = Task.Run(async () => await new Bot().SyncUserStats(identifier, token).ConfigureAwait(false));
                        isStatsSyncRunning = true;
                        Task.Delay(1500);
                    }

                    if (!isDecDistributorRunning)
                    {
                        _ = Task.Run(async () => await new DecDistributor().Start().ConfigureAwait(false));
                        isDecDistributorRunning = true;
                        Task.Delay(1500);
                    }
                }

                while (instances.Count < Settings.MAX_THREADS && !token.IsCancellationRequested)
                {
                    try
                    {
                        lock (_TaskLock)
                        {
                            if (++nextBotInstance >= InstanceManager.BotInstances.Count)
                            {
                                firstRuntrough = false;
                                nextBotInstance = 0;
                                if ((DateTime.Now - lastStatsOutput).TotalMinutes > 1)
                                {
                                    Logs.OutputStat();
                                    lastStatsOutput = DateTime.Now;
                                }
                            }

                            while (InstanceManager.BotInstances.All(x => x.CurrentlyActive
                                || (DateTime)sleepInfo[InstanceManager.BotInstances.IndexOf(x)] > DateTime.Now))
                            {
                                Thread.Sleep(20000);
                            }
                        }

                        lock (_TaskLock)
                        {
                            if (firstRuntrough)
                            {
                                Thread.Sleep(rnd.Next(1000, 5000));
                            }

                            while (InstanceManager.BotInstances.ElementAt(nextBotInstance).CurrentlyActive)
                            {
                                nextBotInstance++;
                                nextBotInstance = nextBotInstance >= InstanceManager.BotInstances.Count ? 0 : nextBotInstance;
                            }

                            int botInstance = nextBotInstance;

                            instances.Add(Task.Run(async () =>
                            {
                                var result = await InstanceManager.BotInstances[botInstance].Start(botInstance);
                                lock (_SleepInfoLock)
                                {
                                    sleepInfo[nextBotInstance] = result;
                                    InstanceManager.UsersStatistics[nextBotInstance].NextMatchIn = result;
                                }
                            }, CancellationToken.None));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logs.LogMessage("BotLoop Error: " + ex.ToString(), Logs.LOG_WARNING);
                    }
                }
                _ = await Task.WhenAny(instances);
                instances.RemoveWhere(x => x.IsCompleted);
            }
            await Task.WhenAll(instances);
            Logs.LogMessage("Bot stopped!");
            Environment.Exit(0);
        }
    }
}
