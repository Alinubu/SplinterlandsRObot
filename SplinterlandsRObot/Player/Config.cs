using SplinterlandsRObot.Global;
using System.Xml;

namespace SplinterlandsRObot.Player
{
    public class Config
    {
        //User
        public int SleepBetweenBattles { get; set; }
        public double EcrLimit { get; set; }
        public bool WaitToRechargeEcr { get; set; }
        public double EcrRechargeLimit { get; set; }
        public int PowerLimit { get; set; }
        //Battle mode
        public string BattleMode { get; set; }
        //League
        public bool LeagueAdvance { get; set; }
        public int LeagueRatingThreshold { get; set; }
        public int MaxLeague { get; set; }
        //Focus
        public bool FocusEnabled { get; set; }
        public bool ClaimFocusChests { get; set; }
        public int FocusStartMinimumCP { get; set; }
        public int FocusMinimumRating { get; set; }
        public bool AvoidFocus { get; set; }
        public string[] FocusBlacklist { get; set; }
        public double FocusRate { get; set; }
        public double FocusRateFire { get; set; }
        public double FocusRateWater { get; set; }
        public double FocusRateEarth { get; set; }
        public double FocusRateLife { get; set; }
        public double FocusRateDeath { get; set; }
        public double FocusRateDragon { get; set; }
        //Season
        public bool AutoClaimSeasonRewards { get; set; }
        //Cards
        public string PreferredSummoners { get; set; }
        public bool ReplaceStarterCards { get; set; }
        public bool UseStarterCards { get; set; }
        //Private API
        public bool UsePrivateApi { get; set; }
        //SPS
        public bool ClaimSPS { get; set; }
        public int CheckForAirdropEvery { get; set; }
        public bool ClaimSPSRewards { get; set; }
        public int ClaimSPSRewardsEvery { get; set; }
        public bool UnstakeSPS { get; set; }
        public double MinimumSPSUnstakeAmount { get; set; }
        public bool UnstakeWeekly { get; set; }
        //Rentals
        public bool EnableRentals { get; set; }
        public bool BattleWhileRenting { get; set; }
        public string DaysToRent { get; set; }
        //Rent file
        public bool UseRentFile { get; set; }
        public int GroupCardsAmount { get; set; }
        public string RentFile { get; set; }
        public int MaxTriesPerAccount { get; set; }
        //Rent for Power
        public bool RentForPower { get; set; }
        public bool RentGoldCardsOnly { get; set; }
        public double CPperDEC { get; set; }
        public int MinimumPowerToRent { get; set; }
        //Renew rentals
        public bool RenewRentals { get; set; }
        public int RenewHoursBeforeEnding { get; set; }
        //Assets transfer
        public string MainAccount { get; set; }
        public bool AutoTransferAfterFocusClaim { get; set; }
        public bool AutoTransferAfterSeasonClaim { get; set; }
        public bool TransferCards { get; set; }
        public bool TransferDEC { get; set; }
        public double KeepDecAmount { get; set; }
        public double MinimumDecToTransfer { get; set; }
        public bool TransferSPS { get; set; }
        public bool TransferPacks { get; set; }
        public bool TransferVouchers { get; set; }

        //Dec Distributor
        public bool RequestDecFromMain { get; set; }
        public double DesiredDecAmount { get; set; }
        public double RequestWhenDecBelow { get; set; }

        private string _filename;

        public Config(string fileName)
        {

            _filename = fileName;
            InstanceManager._configs.Subscribe(OnConfigChanged);
            LoadSettings();
        }

        public void LoadSettings()
        {
            XmlDocument doc = new XmlDocument();
            FileStream fileStream = new FileStream(Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FOLDER, _filename),FileMode.Open,FileAccess.Read);
            doc.Load(fileStream);
            fileStream.Close();
            XmlNode rootNode = doc.SelectSingleNode("config");
            SleepBetweenBattles = Convert.ToInt32(Helpers.ReadNode(rootNode, "SleepBetweenBattles", false, "5"));
            EcrLimit = Convert.ToDouble(Helpers.ReadNode(rootNode, "ECR/Limit", false, "75"));
            WaitToRechargeEcr = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ECR/WaitToRecharge", false, "false"));
            EcrRechargeLimit = Convert.ToDouble(Helpers.ReadNode(rootNode, "ECR/RechargeLimit", false, "99"));
            PowerLimit = Convert.ToInt32(Helpers.ReadNode(rootNode, "PowerLimit", false, "0"));
            BattleMode = Helpers.ReadNode(rootNode, "BattleMode", false, "modern");
            LeagueAdvance = Convert.ToBoolean(Helpers.ReadNode(rootNode, "League/AdvanceToNext", false, "true"));
            LeagueRatingThreshold = Convert.ToInt32(Helpers.ReadNode(rootNode, "League/AdvanceRatingThreshold", false, "0"));
            MaxLeague = Convert.ToInt32(Helpers.ReadNode(rootNode, "League/MaxLeague", false, "0"));
            FocusEnabled = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/DoQuests", false, "true"));
            FocusRate = Convert.ToDouble(Helpers.ReadNode(rootNode, "Quests/FocusRate", false, "50"));
            FocusRateFire = Convert.ToDouble(Helpers.ReadNode(rootNode, "Quests/SplinterFocusOverride/Fire", false, "-1"));
            FocusRateWater = Convert.ToDouble(Helpers.ReadNode(rootNode, "Quests/SplinterFocusOverride/Water", false, "-1"));
            FocusRateEarth = Convert.ToDouble(Helpers.ReadNode(rootNode, "Quests/SplinterFocusOverride/Earth", false, "-1"));
            FocusRateLife = Convert.ToDouble(Helpers.ReadNode(rootNode, "Quests/SplinterFocusOverride/Life", false, "-1"));
            FocusRateDeath = Convert.ToDouble(Helpers.ReadNode(rootNode, "Quests/SplinterFocusOverride/Death", false, "-1"));
            FocusRateDragon = Convert.ToDouble(Helpers.ReadNode(rootNode, "Quests/SplinterFocusOverride/Dragon", false, "-1"));
            ClaimFocusChests = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/ClaimRewards", false, "true"));
            FocusStartMinimumCP = Convert.ToInt32(Helpers.ReadNode(rootNode, "Quests/FocusStartMinimumCP", false, "0"));
            FocusMinimumRating = Convert.ToInt32(Helpers.ReadNode(rootNode, "Quests/FocusMinimumRating", false, "0"));
            AvoidFocus = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/AvoidQuests/Enabled", false, "false"));
            FocusBlacklist = Helpers.ReadNode(rootNode, "Quests/AvoidQuests/QuestList", false, "none") != "none" ? Helpers.ReadNode(rootNode, "Quests/AvoidQuests/QuestList").Split(';') : new string[0];
            AutoClaimSeasonRewards = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Season/AutoClaimSeasonRewards", false, "false"));
            PreferredSummoners = Helpers.ReadNode(rootNode, "Cards/PreferredSummoners", false, "");
            ReplaceStarterCards = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Cards/ReplaceStarterCards", false, "true"));
            UseStarterCards = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Cards/UseStarterCards", false, "true"));
            ClaimSPS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/Airdrops/CollectSPS", false, "false"));
            CheckForAirdropEvery = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/Airdrops/CheckForAirdropEvery", false, "5"));
            ClaimSPSRewards = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/SPS/ClaimSPSRewards", false, "false"));
            ClaimSPSRewardsEvery = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/SPS/ClaimSPSRewardsEvery", false, "24"));
            UnstakeSPS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/SPS/UnstakeSPS", false, "false"));
            MinimumSPSUnstakeAmount = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/SPS/MinimumSPSUnstakeAmount", false, "100"));
            UnstakeWeekly = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/SPS/UnstakeWeekly", false, "false"));
            EnableRentals = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/UseRentalBot", false, "false"));
            BattleWhileRenting = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/BattleWhileRenting", false, "false"));
            DaysToRent = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/DaysToRent", false, "1");
            MaxTriesPerAccount = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/MaxTriesPerUser", false, "999999"));
            UseRentFile = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentSpecificCards", false, "false"));
            GroupCardsAmount = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/GroupCardsAmount", false, "5"));
            RentFile = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentFile", false, "false");
            RentForPower = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentForPower", false, "false"));
            RentGoldCardsOnly = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentGoldOnly", false, "false"));
            CPperDEC = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/CPperDecLimit", false, "250"));
            MinimumPowerToRent = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/MinimumPowerToRent", false, "100"));
            RenewRentals = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RenewRentals", false, "false"));
            RenewHoursBeforeEnding = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RenewHoursBeforeEnding", false, "2"));
            MainAccount = Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/MainAccount", false, "YourMainUser");
            AutoTransferAfterFocusClaim = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/AutoTransferAfterFocusClaim", false, "false"));
            AutoTransferAfterSeasonClaim = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/AutoTransferAfterSeasonClaim", false, "false"));
            TransferCards = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferCards", false, "false"));
            TransferDEC = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferDec", false, "false"));
            KeepDecAmount = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/KeepDecAmount", false, "15"));
            MinimumDecToTransfer = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/MininumDecToTransfer", false, "10"));
            TransferSPS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferSps", false, "false"));
            TransferPacks = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferPacks", false, "false"));
            TransferVouchers = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferVouchers", false, "false"));
            UsePrivateApi = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/UsePrivateAPi", false, "false"));
            RequestDecFromMain = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RequestDecFromMain", false, "false"));
            DesiredDecAmount = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/DesiredDecAmount", false, "0"));
            RequestWhenDecBelow = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/RequestWhenDecBelow", false, "0"));
        }

        private void OnConfigChanged(string filename)
        {
            Thread.Sleep(2000);
            if (filename == _filename)
                LoadSettings();
        }
    }
}
