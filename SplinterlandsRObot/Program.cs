using SplinterlandsRObot;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Extensions;
using SplinterlandsRObot.Global;
using SplinterlandsRObot.Hive;
using System.Runtime.InteropServices;

class Program
{
    private static readonly object _TaskLock = new();
    private static object _SleepInfoLock = new();
    private static string identifier;
    static Task Main(string[] args)
    {
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.Title = "Splinterlands Bot";
            int[] window = new int[] { Console.BufferHeight, Console.WindowHeight };
            Console.SetWindowSize(1, 1);
            Console.SetBufferSize(140, window[0]);
            Console.SetWindowSize(140, window[1]);

            if (Environment.OSVersion.Version.Major < 10)
            {
                Console.WriteLine("This Windows version is no longer supported, please use Windows 10 or above.");
                Logs.LogMessage("Press any key to exit...");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        identifier = GetMachineIdentifier();

        Settings.LoadSettings();
        Logs.LogMessage("Bot settings loaded", Logs.LOG_SUCCESS);
        SplinterlandsData.splinterlandsCards = Task.Run(() => new Splinterlands().GetSplinterlandsCards()).Result;
        SplinterlandsData.splinterlandsSettings = Task.Run(() => new Splinterlands().GetSplinterlandsSettings()).Result;
        InstanceManager.CreateUsersInstance();
        InstanceManager.CreateBotInstances(InstanceManager.userList);
        Settings.CheckThreads();
        

        Console.CancelKeyPress += (sender, eArgs) =>
        {
            Logs.LogMessage("Stopping bot...", Logs.LOG_WARNING);
            eArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        _ = Task.Run(async () => await StartBot(token)).ConfigureAwait(false);

        if (Settings.USE_RENTAL_BOT)
            _ = Task.Run(async () => await new RentProcess().StartRentingProcess(token).ConfigureAwait(false));

        if (Settings.SYNC_BOT_STATS && Settings.DO_BATTLE)
            _ = Task.Run(async () => await new Bot().SyncUserStats(identifier, token).ConfigureAwait(false));

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
                if (command == "START-DEC-REWARDS-EXPORT")
                {
                    _ = Task.Run(async () => await new DecRewards().ExportDecRewards().ConfigureAwait(false));
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
        Random rnd = new();
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
        Environment.Exit(0);
    }

    private static string GetMachineIdentifier()
    {
        string fileName = "passkey.txt";
        string id = "";
        if (File.Exists(fileName))
        {
            id = File.ReadLines(fileName).First();
        }

        if (id != "")
            return id;

        id = Helpers.GenerateMD5Hash( 
            Environment.CurrentDirectory +
            Environment.MachineName +
            Environment.OSVersion +
            Environment.WorkingSet +
            Environment.UserName +
            Environment.ProcessorCount + 
            Environment.SystemPageSize + 
            DateTime.Now.ToBinary() +
            Helpers.RandomString(20));

        File.AppendAllText(fileName, id);

        return id;

    }
}