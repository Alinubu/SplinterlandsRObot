namespace SplinterlandsRObot.Models
{
    public class EcrUpdate
    {
        public string id { get; set; }
        public string msg_id { get; set; }
        public EcrUpdateData data { get; set; }
    }
    public class EcrUpdateData
    {
        public int capture_rate { get; set; }
        public int last_reward_block { get; set; }
        public DateTime last_reward_time { get; set; }
    }
}
