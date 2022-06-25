namespace SplinterlandsRObot.Models.Account
{
    public class Config
    {
        //User
        public bool UsePrivateApi { get; set; }
        public int SleepBetweenBattles { get; set; }
        public double EcrLimit { get; set; }
        public bool WaitToRechargeEcr { get; set; }
        public double EcrRechargeLimit { get; set; }
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
        //SPS
        public bool ClaimSPS { get; set; }
        public int CheckForAirdropEvery { get; set; }
        //Rentals
        public bool EnableRentals { get; set; }
        public int PowerLimit { get; set; }
        public bool BattleWhileRenting { get; set; }
        public bool UseRentFile { get; set; }
        public string RentFile { get; set; }
        //Rent for focus
        public bool RentForFocus { get; set; }
        public bool IgnorePowerLimit { get; set; }
        //Rent for Power
        public bool RentForPower { get; set; }
        public bool RentGoldCardsOnly { get; set; }
        public double CPperDEC { get; set; }
        public int DaysToRent { get; set; }
        public int MinimumPowerToRent { get; set; }
        //Renew rentals
        public bool RenewRentals { get; set; }
        public int RenewHoursBeforeEnding { get; set; }
        //Assets transfer
        public string MainAccount { get; set; }
        public bool TransferCards { get; set; }
        public bool TransferDEC { get; set; }
        public double MinimumDecToTransfer { get; set; }
        public double KeepDecAmount { get; set; }
        public bool TransferSPS { get; set; }
        public bool TransferPacks { get; set; }

        public void ParseConfig(string fileName)
        {

        }

        public void CheckForUpdates(string fileName)
        {

        }
    }
}
