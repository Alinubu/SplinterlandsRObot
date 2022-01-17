using SplinterlandsRObot;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Extensions;
using SplinterlandsRObot.Hive;
using System.Runtime.InteropServices;

class Program
{
    private static readonly object _TaskLock = new();
    private static object _SleepInfoLock = new();
    static Task Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.Title = "Splinterlands Bot";
            int[] window = new int[] { Console.BufferHeight, Console.WindowHeight };
            Console.SetWindowSize(1, 1);
            Console.SetBufferSize(140, window[0]);
            Console.SetWindowSize(140, window[1]);

            if (Environment.OSVersion.Version.Major < 10)
            {
                Console.WriteLine("Windows 7 detected, running bot in Legacy Mode");
                Settings.WINDOWS7 = true;
                Console.Title = "Splinterlands Bot - Legacy Mode";
            }
            else
            {
                Settings.WINDOWS7 = false;
            }
        }

        Settings.ParseConfigFile();
        Logs.LogMessage("Config file loaded", Logs.LOG_SUCCESS);
        SplinterlandsData.splinterlandsCards = Task.Run(() => new Splinterlands().GetSplinterlandsCards()).Result;
        SplinterlandsData.splinterlandsSettings = Task.Run(() => new Splinterlands().GetSplinterlandsSettings()).Result;
        InstanceManager.CreateUsersInstance();
        InstanceManager.CreateBotInstances(Users.userList);
        Settings.CheckThreads();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;

        Console.CancelKeyPress += (sender, eArgs) =>
        {
            Logs.LogMessage("Stopping bot...", Logs.LOG_WARNING);
            eArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        _ = Task.Run(async () => await StartBot(token)).ConfigureAwait(false);

        string? command = "";
        while (true)
        {
            command = Console.ReadLine();

            if (command != null)
            {
                if (command == "START-TRANSFER-BOT")
                {
                    _ = Task.Run(async () => await new TransferAssets().TransferAssetsAsync().ConfigureAwait(false));
                }
                if (command == "START-QUEST-REWARDS-EXPORT")
                {
                    _ = Task.Run(async () => await new QuestsRewards().ExportQuestsRewardsAsync().ConfigureAwait(false));
                }
                if (command == "START-CLAIM-SEASON-REWARDS")
                {
                    _ = Task.Run(async () => await new HiveActions().ClaimSeasonRewards().ConfigureAwait(false));
                }
                else
                {
                    string[] setting = command.Split('=');
                    if (setting.Count() == 2)
                    {
                        Settings.ChangeConfig(setting[0], setting[1]);
                    }
                }
            }
        }
    }

    static async Task StartBot(CancellationToken token)
    {
        var instances = new HashSet<Task>();
        int nextBotInstance = -1;
        bool firstRuntrough = true;
        DateTime[] sleepInfo = new DateTime[InstanceManager.BotInstances.Count];
        Logs.LogMessage("Starting Bot");
        while (!token.IsCancellationRequested)
        {
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
                            if(Settings.SHOW_BATTLE_RESULTS) Logs.OutputStat();
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
                            Random rnd = new();
                            Thread.Sleep(rnd.Next(1000, 5000));
                        }

                        while (InstanceManager.BotInstances.ElementAt(nextBotInstance).CurrentlyActive)
                        {
                            nextBotInstance++;
                            nextBotInstance = nextBotInstance >= InstanceManager.BotInstances.Count ? 0 : nextBotInstance;
                        }
                        // create local copies for thread safety
                        int botInstance = nextBotInstance;

                        instances.Add(Task.Run(async () =>
                        {
                            var result = await InstanceManager.BotInstances[botInstance].DoBattleAsync(botInstance);
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
    }
}