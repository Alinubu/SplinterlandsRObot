namespace SplinterlandsRObot.Models.Account
{
    public class UserDetails
    {
        public long timestamp { get; set; }
        public string name { get; set; }
        public string token { get; set; }
        public bool starter_pack_purchase { get; set; }
        public int rating { get; set; }
        public int max_rating { get; set; }
        public int battles { get; set; }
        public int wins { get; set; }
        public int current_streak { get; set; }
        public int longest_streak { get; set; }
        public int max_rank { get; set; }
        public int champion_points { get; set; }
        public int capture_rate { get; set; }
        public int last_reward_block { get; set; }
        public DateTime last_reward_time { get; set; }
        public int collection_power { get; set; }
        public int league { get; set; }
        public List<Balance> balances { get; set; }
        //public List<object> unrevealed_rewards { get; set; }
        public int season_max_league { get; set; }
        public Quest quest { get; set; }
        public SeasonReward? season_reward { get; set; }
        public CurrentSeasonPlayer? current_season_player { get; set; }
        public PreviousSeasonPlayer? previous_season_player { get; set; }
        public string jwt_token { get; set; }
        public DateTime jwt_expiration_dt { get; set; }
        public string? outstanding_match { get; set; }
    }
    public class Balance
    {
        public string player { get; set; }
        public string token { get; set; }
        public double balance { get; set; }
        public int? last_reward_block { get; set; }
        public DateTime? last_reward_time { get; set; }
    }
    public class CurrentSeasonPlayer
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
        public int chest_tier { get; set; }
        public int rshares { get; set; }
    }
    public class PreviousSeasonPlayer
    {
        public int season { get; set; }
        public string player { get; set; }
        public int rating { get; set; }
        public int battles { get; set; }
        public int wins { get; set; }
        public int max_rating { get; set; }
        public int longest_streak { get; set; }
        public string reward_claim_tx { get; set; }
        public int league { get; set; }
        public int max_league { get; set; }
        public object chest_tier { get; set; }
        public int rshares { get; set; }
    }

    
    public class SeasonReward
    {
        public int reward_packs { get; set; }
    }
}
