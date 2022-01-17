namespace SplinterlandsRObot.Constructors
{
    public class User
    {
        public string Username { get; set; }
        public PassCodes PassCodes { get; set; }
        public bool UseRentBot { get; set; }
        public int? PowerLimit { get; set; }
        public List<Card>? RentDetails { get; set; }
    }
    public class PassCodes
    {
        public string? ActiveKey { get; set; }
        public string PostingKey { get; set; }
        public string? AccessToken { get; set; }
        public string? RentBotKey { get; set; }
    }
}
