using System.Reflection;
using System.Xml;

namespace SplinterlandsRObot
{
    public static class Settings
    {
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
        public static bool CLAIM_QUEST_REWARDS { get; set; }
        public static bool DONT_CLAIM_QUEST_NEAR_NEXT_LEAGUE { get; set; }
        public static bool AVOID_SPECIFIC_QUESTS { get; private set; }
        public static string[]? AVOID_SPECIFIC_QUESTS_LIST { get; private set; }
        public static string PREFERRED_SUMMONERS { get; private set; }
        public static bool COLLECT_SPS { get; set; }
        public static bool USE_RENTAL_BOT { get; set; }
        public static bool RENT_GOLD_ONLY { get; set; }
        public static double MAX_PRICE_PER_500_DEC { get; set; }
        public static string DAYS_TO_RENT { get; set; }
        public static bool USE_PRIVATE_API { get; set; }
        public static bool USE_ENEMY_PREDICTION { get; set; }
        public static string TRANSFER_BOT_MAIN_ACCOUNT { get; set; }
        public static bool TRANSFER_BOT_SEND_CARDS { get; set; }
        public static bool TRANSFER_BOT_SEND_DEC { get; set; }
        public static double TRANSFER_BOT_KEEP_DEC_AMOUNT { get; set; }
        public static double TRANSFER_BOT_MINIMUM_DEC_TO_TRANSFER { get; set; }
        public static bool TRANSFER_BOT_SEND_SPS { get; set; }
        public static bool WINDOWS7 { get; set; }

        public static void ParseConfigFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Constants.CONFIG_FOLDER, "config.xml"));

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
            CLAIM_QUEST_REWARDS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/ClaimRewards").InnerText);
            DONT_CLAIM_QUEST_NEAR_NEXT_LEAGUE = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/DontClaimNearNextLeague").InnerText);
            AVOID_SPECIFIC_QUESTS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("Quests/AvoidQuests/Enabled").InnerText);
            AVOID_SPECIFIC_QUESTS_LIST = doc.DocumentElement.SelectSingleNode("Quests/AvoidQuests/QuestList").InnerText != null ? doc.DocumentElement.SelectSingleNode("Quests/AvoidQuests/QuestList").InnerText.Split(';') : new string[0];
            PREFERRED_SUMMONERS = doc.DocumentElement.SelectSingleNode("PreferredSummoners").InnerText;
            COLLECT_SPS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/Airdrops/CollectSPS").InnerText);
            USE_RENTAL_BOT = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/UseRentalBot").InnerText);
            RENT_GOLD_ONLY = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/RentGoldOnly").InnerText);
            MAX_PRICE_PER_500_DEC = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/MaxPricePer500DEC").InnerText);
            DAYS_TO_RENT = doc.DocumentElement.SelectSingleNode("ProFeatures/RentalBot/DaysToRent").InnerText;
            USE_PRIVATE_API = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/UsePrivateAPi").InnerText);
            TRANSFER_BOT_MAIN_ACCOUNT = doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/MainAccount").InnerText;
            TRANSFER_BOT_SEND_CARDS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/TransferCards").InnerText);
            TRANSFER_BOT_SEND_DEC = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/TransferDec").InnerText);
            TRANSFER_BOT_KEEP_DEC_AMOUNT = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/KeepDecAmount").InnerText);
            TRANSFER_BOT_MINIMUM_DEC_TO_TRANSFER = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/MininumDecToTransfer").InnerText);
            TRANSFER_BOT_SEND_SPS = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("ProFeatures/TransferBot/TransferSps").InnerText);
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
            if ( MAX_THREADS > Users.userList.Count) MAX_THREADS = 1;
        }
    }
}
