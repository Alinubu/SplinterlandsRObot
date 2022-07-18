using SplinterlandsRObot.Global;
using System.Xml;

namespace SplinterlandsRObot.Models.Account
{
    public class Config
    {
        //User
        public int SleepBetweenBattles { get; set; }
        public double EcrLimit { get; set; }
        public bool WaitToRechargeEcr { get; set; }
        public double EcrRechargeLimit { get; set; }
        //Battle mode
        public string BattleMode { get; set; }
        //League
        public bool LeagueAdvance { get; set; }
        public int LeagueRatingThreshold { get; set; }
        public int MaxLeague { get; set; }
        //Focus
        public bool FocusEnabled { get; set; }
        public bool ClaimFocusChests { get; set; }
        public bool AvoidFocus { get; set; }
        public string[] FocusBlacklist { get; set; }
        public double FocusRate { get; set; }
        public double FocusRateFire { get; set; }
        public double FocusRateWater { get; set; }
        public double FocusRateEarth { get; set; }
        public double FocusRateLife { get; set; }
        public double FocusRateDeath { get; set; }
        public double FocusRateDragon { get; set; }
        //Cards
        public string PreferredSummoners { get; set; }
        public bool ReplaceStarterCards { get; set; }
        public bool UseStarterCards { get; set; }
        //Private API
        public bool UsePrivateApi { get; set; }
        //SPS
        public bool ClaimSPS { get; set; }
        public int CheckForAirdropEvery { get; set; }
        //Rentals
        public bool EnableRentals { get; set; }
        public int PowerLimit { get; set; }
        public bool BattleWhileRenting { get; set; }
        public string DaysToRent { get; set; }
        public bool IgnorePowerLimit { get; set; }//used by rent files
        //Rent file
        public bool UseRentFile { get; set; }
        public string RentFile { get; set; }
        public int MaxTriesPerAccount { get; set; }
        //Rent for focus
        //public bool RentForFocus { get; set; }
        //public string FireRentFile { get; set; }
        //public string WaterRentFile { get; set; }
        //public string EarthRentFile { get; set; }
        //public string LifeRentFile { get; set; }
        //public string DeathRentFile { get; set; }
        //public string DragonRentFile { get; set; }
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
        public bool TransferCards { get; set; }
        public bool TransferDEC { get; set; }
        public double KeepDecAmount { get; set; }
        public double MinimumDecToTransfer { get; set; }
        public bool TransferSPS { get; set; }
        public bool TransferPacks { get; set; }

        private string _filename;

        //private FileSystemWatcher _fsWatcher;

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
            AvoidFocus = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Quests/AvoidQuests/Enabled", false, "false"));
            FocusBlacklist = Helpers.ReadNode(rootNode, "Quests/AvoidQuests/QuestList", false, "none") != "none" ? Helpers.ReadNode(rootNode, "Quests/AvoidQuests/QuestList").Split(';') : new string[0];
            PreferredSummoners = Helpers.ReadNode(rootNode, "Cards/PreferredSummoners", false, "");
            ReplaceStarterCards = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Cards/ReplaceStarterCards", false, "true"));
            UseStarterCards = Convert.ToBoolean(Helpers.ReadNode(rootNode, "Cards/UseStarterCards", false, "true"));
            ClaimSPS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/Airdrops/CollectSPS", false, "false"));
            CheckForAirdropEvery = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/Airdrops/CheckForAirdropEvery", false, "5"));
            EnableRentals = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/UseRentalBot", false, "false"));
            PowerLimit = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/PowerLimit", false, "0"));
            BattleWhileRenting = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/BattleWhileRenting", false, "false"));
            DaysToRent = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/DaysToRent", false, "1");
            MaxTriesPerAccount = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/MaxTriesPerUser", false, "999999"));
            //RentForFocus = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentForFocus", false, "false"));
            //FireRentFile = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/FireRentFile", false, "");
            //WaterRentFile = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/WaterRentFile", false, "");
            //EarthRentFile = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/EarthRentFile", false, "");
            //LifeRentFile = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/LifeRentFile", false, "");
            //DeathRentFile = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/DeathRentFile", false, "");
            //DragonRentFile = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/DragonRentFile", false, "");
            UseRentFile = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentSpecificCards", false, "false"));
            RentFile = Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentFile", false, "false");
            RentForPower = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentForPower", false, "false"));
            RentGoldCardsOnly = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RentGoldOnly", false, "false"));
            CPperDEC = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/CPperDecLimit", false, "250"));
            MinimumPowerToRent = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/MinimumPowerToRent", false, "100"));
            RenewRentals = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RenewRentals", false, "false"));
            RenewHoursBeforeEnding = Convert.ToInt32(Helpers.ReadNode(rootNode, "ProFeatures/RentalBot/RenewHoursBeforeEnding", false, "2"));
            MainAccount = Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/MainAccount", false, "YourMainUser");
            AutoTransferAfterFocusClaim = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/AutoTransferAfterFocusClaim", false, "false"));
            TransferCards = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferCards", false, "false"));
            TransferDEC = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferDec", false, "false"));
            KeepDecAmount = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/KeepDecAmount", false, "15"));
            MinimumDecToTransfer = Convert.ToDouble(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/MininumDecToTransfer", false, "10"));
            TransferSPS = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferSps", false, "false"));
            TransferPacks = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/TransferBot/TransferPacks", false, "false"));
            UsePrivateApi = Convert.ToBoolean(Helpers.ReadNode(rootNode, "ProFeatures/UsePrivateAPi", false, "false"));
        }

        private void OnConfigChanged(string filename)
        {
            Thread.Sleep(2000);
            if (filename == _filename)
                LoadSettings();
        }

        public void CheckForUpdates()
        {

        }
    }
}
