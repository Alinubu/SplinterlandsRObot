namespace SplinterlandsRObot.Models
{
    public class RatingUpdate
    {
        public string id { get; set; }
        public string msg_id { get; set; }
        public RatingUpdateData data { get; set; }
    }
    public class RatingUpdateData
    {
        public int new_rating { get; set; }
        public int new_league { get; set; }
        public int new_max_league { get; set; }
        public int additional_season_rshares { get; set; }
    }
}
