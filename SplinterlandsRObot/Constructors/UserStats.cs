namespace SplinterlandsRObot.Constructors
{
    public class UserStats
    {
        public string Account { get; set; }
        public double? ECR { get; set; }
        public int? Wins { get; set; }
        public int? Draws { get; set; }
        public int? Losses { get; set; }
        public double? MatchRewards { get; set; }
        public double? TotalRewards { get; set; }
        public int? Rating { get; set; }
        public string? RatingChange { get; set; }
        public int? CollectionPower { get; set; }
        public string? Quest { get; set; }
        public DateTime NextMatchIn { get; set; }
        public string ErrorMessage { get; set; }

    }
}
