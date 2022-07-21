namespace SplinterlandsRObot.Player
{
    public class UserStats
    {
        public string Account { get; set; }
        public Balances? Balance { get; set; }
        public double? RentCost { get; set; }
        public int? Wins { get; set; }
        public int? Draws { get; set; }
        public int? Losses { get; set; }
        public double? MatchRewards { get; set; }
        public double? TotalRewards { get; set; }
        public int? Rating { get; set; }
        public string? RatingChange { get; set; }
        public string? League { get; set; }
        public int? CollectionPower { get; set; }
        public string? Quest { get; set; }
        public string? HoursUntilNextQuest { get; set; }
        public string? Season { get; set; }
        public DateTime NextMatchIn { get; set; }
        public string? ErrorMessage { get; set; }

    }
}
