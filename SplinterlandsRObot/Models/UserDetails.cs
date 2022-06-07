namespace SplinterlandsRObot.Models
{
    public class UserDetails
    {
        public string name { get; set; }
        public DateTime join_date { get; set; }
        public int rating { get; set; }
        public int battles { get; set; }
        public int wins { get; set; }
        public int current_streak { get; set; }
        public int longest_streak { get; set; }
        public int max_rating { get; set; }
        public int max_rank { get; set; }
        public int champion_points { get; set; }
        public double? capture_rate { get; set; }
        public int? last_reward_block { get; set; }
        public object? guild { get; set; }
        public bool starter_pack_purchase { get; set; }
        public int avatar_id { get; set; }
        public object? display_name { get; set; }
        public object? title_pre { get; set; }
        public object? title_post { get; set; }
        public int collection_power { get; set; }
        public int league { get; set; }
        public bool adv_msg_sent { get; set; }
        public SeasonDetails season_details { get; set; }
    }

    public class SeasonDetails
    {
    }
}
