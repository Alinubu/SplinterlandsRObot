namespace SplinterlandsRObot.Constructors
{
    public class QuestData
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
        public Rewards? rewards = null;
        public int league { get; set; }
    }

    public class Rewards
    {
        public string? type { get; set; }
        public int? quantity { get; set; }
        public string? potion_type { get { return potion_type; } set { potion_type = value; } }

    }
}
