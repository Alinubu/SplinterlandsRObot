namespace SplinterlandsRObot.Models
{
    public class QuestProgress
    {
        public string id { get; set; }
        public string msg_id { get; set; }
        public QuestProgressData data { get; set; }
    }
    public class QuestProgressData
    {
        public string id { get; set; }
        public string player { get; set; }
        public DateTime created_date { get; set; }
        public int created_block { get; set; }
        public string name { get; set; }
        public int total_items { get; set; }
        public int completed_items { get; set; }
        public string? claim_trx_id { get; set; }
        public DateTime? claim_date { get; set; }
        public int reward_qty { get; set; }
        public string? refresh_trx_id { get; set; }
        public object? rewards { get; set; }
        public int chest_tier { get; set; }
        public int rshares { get; set; }
    }
}
