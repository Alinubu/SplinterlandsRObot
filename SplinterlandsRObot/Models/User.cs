namespace SplinterlandsRObot.Models
{
    public class User
    {
        public string Username { get; set; }
        public PassCodes PassCodes { get; set; }
        public bool UseRentBot { get; set; }
        public int? PowerLimit { get; set; }
        public double ECROverride { get; set; }
        public int MaxLeague { get; set; }
        public string RentFile { get; set; }
    }
    public class PassCodes
    {
        public string? ActiveKey { get; set; }
        public string PostingKey { get; set; }
        public string? AccessToken { get; set; }
        public string? RentBotKey { get; set; }
    }
}
