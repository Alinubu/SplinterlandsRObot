namespace SplinterlandsRObot.Player
{
    public class User
    {
        public string Username { get; set; }
        public Keys Keys { get; set; }
        public string ConfigFile { get; set; }
    }
    public class Keys
    {
        public string? ActiveKey { get; set; }
        public string PostingKey { get; set; }
        public string? JwtToken { get; set; }
        public DateTime? JwtExpire { get; set; }
    }
}
