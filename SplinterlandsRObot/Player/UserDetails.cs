using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Player.PlayerFocus;
using SplinterlandsRObot.Player.PlayerSeason;

namespace SplinterlandsRObot.Player
{
    public class UserDetails
    {
        public long timestamp { get; set; }
        public string name { get; set; }
        public string token { get; set; }
        public bool starter_pack_purchase { get; set; }
        public int rating { get; set; }
        public int? modern_rating { get; set; }
        public int max_rating { get; set; }
        public int? modern_max_rating { get; set; }
        public int battles { get; set; }
        public int? modern_battle { get; set; }
        public int wins { get; set; }
        public int? modern_wins { get; set; }
        public int current_streak { get; set; }
        public int? modern_current_streak { get; set; }
        public int longest_streak { get; set; }
        public int? modern_longest_streak { get; set; }
        public int max_rank { get; set; }
        public int? modern_max_rank { get; set; }
        public int champion_points { get; set; }
        public string capture_rate { get; set; }
        public int? last_reward_block { get; set; }
        public DateTime last_reward_time { get; set; }
        public int collection_power { get; set; }
        public int league { get; set; }
        public int? modern_league { get; set; }
        public List<Balance> balances { get; set; }
        public int season_max_league { get; set; }
        public int? modern_season_max_league { get; set; }
        public Focus? quest { get; set; }
        public SeasonReward? season_reward { get; set; }
        public Season? current_season_player { get; set; }
        public Season? current_modern_season_player { get; set; }
        public Season? previous_season_player { get; set; }
        public string? jwt_token { get; set; }
        public DateTime? jwt_expiration_dt { get; set; }
        public JToken? outstanding_match { get; set; }
    }
    public class SeasonReward
    {
        public int? chest_tier { get; set; }
        public string? format { get; set; }
        public int? max_league { get; set; }
        public int reward_packs { get; set; }
        public int? season { get; set; }
    }
}
