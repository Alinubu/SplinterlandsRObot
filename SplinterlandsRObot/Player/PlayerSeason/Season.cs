namespace SplinterlandsRObot.Player.PlayerSeason
{
    public class Season
    {
        public int season { get; set; }
        public string player { get; set; }
        public int rating { get; set; }
        public int battles { get; set; }
        public int wins { get; set; }
        public int max_rating { get; set; }
        public int longest_streak { get; set; }
        public string? reward_claim_tx { get; set; }
        public int league { get; set; }
        public int max_league { get; set; }
        public int? chest_tier { get; set; } = 0;
        public int rshares { get; set; }
    }
}
