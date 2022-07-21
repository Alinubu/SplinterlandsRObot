namespace SplinterlandsRObot.Player
{
    public class Balance
    {
        public string player { get; set; }
        public string token { get; set; }
        public double balance { get; set; }
        public int? last_reward_block { get; set; }
        public DateTime? last_reward_time { get; set; }
    }
}
