using SplinterlandsRObot;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Extensions;
using SplinterlandsRObot.Global;
using SplinterlandsRObot.Hive;
using System.Runtime.InteropServices;

class Program
{
    static async Task Main(string[] args)
    {
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.Title = "Splinterlands Bot";

            if (Environment.OSVersion.Version.Major < 10)
            {
                Console.WriteLine("This Windows version is no longer supported, please use Windows 10 or above.");
                Logs.LogMessage("Press any key to exit...");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        InstanceManager.identifier = Helpers.GetMachineIdentifier();

        Settings.LoadSettings();
        Logs.LogMessage("Bot settings loaded", Logs.LOG_SUCCESS);
        SplinterlandsData.splinterlandsCards = Task.Run(() => new Splinterlands().GetSplinterlandsCards()).Result;
        SplinterlandsData.splinterlandsSettings = Task.Run(() => new Splinterlands().GetSplinterlandsSettings()).Result;
        InstanceManager.CreateUsersInstance();
        InstanceManager.CreateBotInstances(InstanceManager.userList);
        InstanceManager.StartFileWatcher();
        Settings.CheckThreads();
        

        Console.CancelKeyPress += (sender, eArgs) =>
        {
            Logs.LogMessage("Stopping bot...", Logs.LOG_WARNING);
            eArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };
        
        _ = Task.Run(async () => await InstanceManager.StartBot(token)).ConfigureAwait(false);

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
                else if (command == "START-QUEST-REWARDS-EXPORT")
                {
                    _ = Task.Run(async () => await new QuestsRewards().ExportQuestsRewardsAsync().ConfigureAwait(false));
                }
                else if (command == "START-DEC-REWARDS-EXPORT")
                {
                    _ = Task.Run(async () => await new DecRewards().ExportDecRewards().ConfigureAwait(false));
                }
                else if (command == "START-CLAIM-SEASON-REWARDS")
                {
                    _ = Task.Run(async () => await new HiveService().ClaimSeasonRewards().ConfigureAwait(false));
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
}