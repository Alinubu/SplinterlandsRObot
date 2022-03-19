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

            DO_BATTLE = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("DoBattle").InnerText);
            MAX_THREADS = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("MaxThreads").InnerText);
            HOLD_CACHE_FOR = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("HoldCacheFor").InnerText);
            DEBUG_MODE = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("DebugMode").InnerText);
            API_URL = doc.DocumentElement.SelectSingleNode("ApiUrl").InnerText;
            SLEEP_BETWEEN_BATTLES = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("SleepBetweenBattles").InnerText);
            SHOW_BATTLE_RESULTS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ShowBattleResults").InnerText);
            ECR_LIMIT = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("ECR/Limit").InnerText);
            ECR_WAIT_TO_RECHARGE = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ECR/WaitToRecharge").InnerText);
            ECR_RECHARGE_LIMIT = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("ECR/RechargeLimit").InnerText);
            LEAGUE_ADVANCE_TO_NEXT = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("League/AdvanceToNext").InnerText);
            DO_QUESTS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/DoQuests").InnerText);
            IGNORE_ECR_LIMIT_FOR_QUEST = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/IgnoreECRLimit").InnerText);
            SLEEP_AFTER_QUEST_COMPLETED = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/SleepAfterQuestCompleted").InnerText);
            CLAIM_QUEST_REWARDS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/ClaimRewards").InnerText);
            SHOW_QUEST_REWARDS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/ShowQuestRewards").InnerText);
            DONT_CLAIM_QUEST_NEAR_NEXT_LEAGUE = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/DontClaimNearNextLeague").InnerText);
            AVOID_SPECIFIC_QUESTS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/AvoidQuests/Enabled").InnerText);
            AVOID_SPECIFIC_QUESTS_LIST = doc.DocumentElement.SelectSingleNode("Quests/AvoidQuests/QuestList").InnerText != null ? doc.DocumentElement.SelectSingleNode("Quests/AvoidQuests/QuestList").InnerText.Split(';') : new string[0];
            PREFERRED_SUMMONERS = doc.DocumentElement.SelectSingleNode("PreferredSummoners").InnerText;
            REPLACE_STARTER_CARDS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ReplaceStarterCards").InnerText);
            COLLECT_SPS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/Airdrops/CollectSPS").InnerText);
            USE_RENTAL_BOT = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/UseRentalBot").InnerText);
            BATTLE_WHILE_RENTING = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/BattleWhileRenting").InnerText);
            RENT_GOLD_ONLY = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/RentGoldOnly").InnerText);
            CP_PER_DEC_LIMIT = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/CPperDecLimit").InnerText);
            DAYS_TO_RENT = doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/DaysToRent").InnerText;
            RENT_SPECIFIC_CARDS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/RentSpecificCards").InnerText);
            RENT_FOR_POWER = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/RentForPower").InnerText);
            MINIMUM_POWER_TO_RENT = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/MinimumPowerToRent").InnerText);
            RENEW_RENTALS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/RenewRentals").InnerText);
            RENEW_HOURS_BEFORE_ENDING = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/RenewHoursBeforeEnding").InnerText);
            TRANSFER_BOT_MAIN_ACCOUNT = doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/MainAccount").InnerText;
            TRANSFER_BOT_SEND_CARDS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/TransferCards").InnerText);
            TRANSFER_BOT_SEND_DEC = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/TransferDec").InnerText);
            TRANSFER_BOT_KEEP_DEC_AMOUNT = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/KeepDecAmount").InnerText);
            TRANSFER_BOT_MINIMUM_DEC_TO_TRANSFER = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/MininumDecToTransfer").InnerText);
            TRANSFER_BOT_SEND_SPS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/TransferSps").InnerText);
            TRANSFER_BOT_SEND_PACKS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/TransferPacks").InnerText);
            USE_PRIVATE_API = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/UsePrivateAPi").InnerText);
            SYNC_BOT_STATS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/SyncBotStats").InnerText);
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
