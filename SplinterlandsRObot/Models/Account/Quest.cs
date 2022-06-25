﻿namespace SplinterlandsRObot.Models.Account
{
    public class Quest
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
        public int chest_tier { get; set; }
        public int rshares { get; set; }
        public int earned_chests = 0;
    }
}
