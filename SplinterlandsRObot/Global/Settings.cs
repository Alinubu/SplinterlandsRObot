using SplinterlandsRObot.Global;
using System.Reflection;
using System.Xml;

namespace SplinterlandsRObot
{
    public static class Settings
    {
        public static bool DO_BATTLE { get; private set; }
        public static int MAX_THREADS { get; private set; }
        public static int HOLD_CACHE_FOR { get; set; }
        public static bool DEBUG_MODE { get; set; }
        public static string? API_URL { get; private set; }
        public static int SLEEP_BETWEEN_BATTLES { get; set; }
        public static bool SHOW_BATTLE_RESULTS { get; set; }
        public static double ECR_LIMIT { get; set; }
        public static bool ECR_WAIT_TO_RECHARGE { get; set; }
        public static double ECR_RECHARGE_LIMIT { get; set; }
        public static bool LEAGUE_ADVANCE_TO_NEXT { get; set; }
        public static bool DO_QUESTS { get; set; }
        public static bool IGNORE_ECR_LIMIT_FOR_QUEST { get; set; }
        public static bool SLEEP_AFTER_QUEST_COMPLETED { get; set; }
        public static bool CLAIM_QUEST_REWARDS { get; set; }
        public static bool SHOW_QUEST_REWARDS { get; private set; }
        public static bool DONT_CLAIM_QUEST_NEAR_NEXT_LEAGUE { get; set; }
        public static bool AVOID_SPECIFIC_QUESTS { get; private set; }
        public static string[]? AVOID_SPECIFIC_QUESTS_LIST { get; private set; }
        public static string PREFERRED_SUMMONERS { get; private set; }
        public static bool REPLACE_STARTER_CARDS { get; set; }
        public static bool USE_STARTER_CARDS { get; private set; }
        public static bool COLLECT_SPS { get; set; }
        public static bool USE_RENTAL_BOT { get; set; }
        public static bool BATTLE_WHILE_RENTING { get; private set; }
        public static bool RENT_GOLD_ONLY { get; set; }
        public static double CP_PER_DEC_LIMIT { get; set; }
        public static string DAYS_TO_RENT { get; set; }
        public static bool RENT_SPECIFIC_CARDS { get; private set; }
        public static bool RENT_FOR_POWER { get; private set; }
        public static int MINIMUM_POWER_TO_RENT { get; private set; }
        public static bool RENEW_RENTALS { get; private set; }
        public static int RENEW_HOURS_BEFORE_ENDING { get; private set; }
        public static string TRANSFER_BOT_MAIN_ACCOUNT { get; set; }
        public static bool TRANSFER_BOT_SEND_CARDS { get; set; }
        public static bool TRANSFER_BOT_SEND_DEC { get; set; }
        public static double TRANSFER_BOT_KEEP_DEC_AMOUNT { get; set; }
        public static double TRANSFER_BOT_MINIMUM_DEC_TO_TRANSFER { get; set; }
        public static bool TRANSFER_BOT_SEND_SPS { get; set; }
        public static bool TRANSFER_BOT_SEND_PACKS { get; private set; }
        public static bool USE_PRIVATE_API { get; set; }
        public static bool SYNC_BOT_STATS { get; set; }
        public static bool WINDOWS7 { get; set; }

        public static void ParseConfigFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FOLDER, "config.xml"));
            XmlNode rootNode = doc.SelectSingleNode("config");
            DO_BATTLE = Convert.ToBoolean(Helpers.ReadNode(doc.SelectSingleNode("config"), "DoBattle", false, "true"));
            MAX_THREADS = Convert.ToInt32(Helpers.ReadNode(rootNode, "MaxThreads", false, "1"));
            HOLD_CACHE_FOR = Convert.ToInt32(Helpers.ReadNode(rootNode, "HoldCacheFor", false, "30"));
            DEBUG_MODE = Convert.ToBoolean(Helpers.ReadNode(rootNode, "DebugMode", false, "false"));
            API_URL = Helpers.ReadNode(rootNode, "ApiUrl", false, "http://api.splinterlandsrobot.com:5000");
            SLEEP_BETWEEN_BATTLES = Convert.ToInt32(Helpers.ReadNode(rootNode, "SleepBetweenBattles", false, "5"));
            SHOW_BATTLE_RESULTS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ShowBattleResults", false, "true"));
            ECR_LIMIT = Convert.ToDouble(Helpers.ReadNode(rootNode, "ECR/Limit", false, "75"));
            ECR_WAIT_TO_RECHARGE = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ECR/WaitToRecharge", false, "false"));
            ECR_RECHARGE_LIMIT = Convert.ToDouble(Helpers.ReadNode(rootNode, "ECR/RechargeLimit", false, "99"));
            LEAGUE_ADVANCE_TO_NEXT = Convert.ToBoolean(Helpers.ReadNode(rootNode, "League/AdvanceToNext", false, "true"));
            DO_QUESTS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/DoQuests", false, "true"));
            IGNORE_ECR_LIMIT_FOR_QUEST = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/IgnoreECRLimit", false, "false"));
            SLEEP_AFTER_QUEST_COMPLETED = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/SleepAfterQuestCompleted", false, "false"));
            CLAIM_QUEST_REWARDS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/ClaimRewards", false, "true"));
            SHOW_QUEST_REWARDS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/ShowQuestRewards", false, "true"));
            DONT_CLAIM_QUEST_NEAR_NEXT_LEAGUE = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/DontClaimNearNextLeague", false, "true"));
            AVOID_SPECIFIC_QUESTS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/AvoidQuests/Enabled", false, "false"));
            AVOID_SPECIFIC_QUESTS_LIST = Helpers.ReadNode(rootNode, "Quests/AvoidQuests/QuestList", false, "none") != "none" ? Helpers.ReadNode(rootNode, "Quests/AvoidQuests/QuestList").Split(';') : new string[0];
            PREFERRED_SUMMONERS = Helpers.ReadNode(rootNode, "Cards/PreferredSummoners", false, "");
            REPLACE_STARTER_CARDS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Cards/ReplaceStarterCards", false, "true"));
            USE_STARTER_CARDS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Cards/UseStarterCards", false, "true"));
            COLLECT_SPS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/Airdrops/CollectSPS", false, "false"));
            USE_RENTAL_BOT = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/UseRentalBot", false, "false"));
            BATTLE_WHILE_RENTING = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/BattleWhileRenting", false, "false"));
            RENT_SPECIFIC_CARDS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentSpecificCards", false, "false"));
            RENT_FOR_POWER = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentForPower", false, "false"));
            RENT_GOLD_ONLY = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentGoldOnly", false, "false"));
            CP_PER_DEC_LIMIT = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/CPperDecLimit", false, "250"));
            DAYS_TO_RENT = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/DaysToRent", false, "1");
            MINIMUM_POWER_TO_RENT = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/MinimumPowerToRent", false, "100"));
            RENEW_RENTALS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RenewRentals", false, "false"));
            RENEW_HOURS_BEFORE_ENDING = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RenewHoursBeforeEnding", false, "2"));
            TRANSFER_BOT_MAIN_ACCOUNT = Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/MainAccount", false, "YourMainUser");
            TRANSFER_BOT_SEND_CARDS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferCards", false, "false"));
            TRANSFER_BOT_SEND_DEC = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferDec", false, "false"));
            TRANSFER_BOT_KEEP_DEC_AMOUNT = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/KeepDecAmount", false, "15"));
            TRANSFER_BOT_MINIMUM_DEC_TO_TRANSFER = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/MininumDecToTransfer", false, "10"));
            TRANSFER_BOT_SEND_SPS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferSps", false, "false"));
            TRANSFER_BOT_SEND_PACKS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferPacks", false, "false"));
            USE_PRIVATE_API = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/UsePrivateAPi", false, "false"));
            SYNC_BOT_STATS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/SyncBotStats", false, "false"));
        }
        public static void ChangeConfig(string settingName, string settingValue)
        {
            try
            {
                if (Constants.NOT_CHANGEABLE_SETTINGS.Contains(settingName))
                {
                    Logs.LogMessage($"Changing {settingName} is not allowed", Logs.LOG_WARNING);
                    return;
                }
                PropertyInfo settingInfo = typeof(Settings).GetProperty(settingName);
                settingInfo.SetValue(settingName, Convert.ChangeType(settingValue,settingInfo.PropertyType), null);
                Logs.LogMessage($"{settingName} has changed to: {settingValue}", Logs.LOG_SUCCESS);
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"Error changing {settingName}: {ex.Message}", Logs.LOG_WARNING);
            }
        }

        public static void CheckThreads()
        {
            if ( MAX_THREADS > InstanceManager.userList.Count) MAX_THREADS = 1;
        }
    }
}
