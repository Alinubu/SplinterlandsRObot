namespace SplinterlandsRObot.Models.Account
{
    public class QuestData
    {
        public int? chest_tier { get; set; }
        public DateTime? claim_date { get; set; }
        public string? claim_trx_id { get; set; }
        public int completed_items { get; set; }
        public int created_block { get; set; }
        public DateTime created_date { get; set; }
        public string id { get; set; }
        public int league { get; set; }
        public string name { get; set; }
        public string player { get; set; }
        public string? refresh_trx_id { get; set; }
        public int reward_qty { get; set; }
        public Rewards? rewards = null;
        public double rshares { get; set; }
        public int total_items { get; set; }
        public int earned_chests = 0;
    }

    public class Rewards
    {
        public string? type { get; set; }
        public int? quantity { get; set; }
        public string? potion_type { get { return potion_type; } set { potion_type = value; } }

    }
    public class TrxInfo
    {
        public string id { get; set; }
        public string block_id { get; set; }
        public string prev_block_id { get; set; }
        public string type { get; set; }
        public string player { get; set; }
        public RewardInfo data { get; set; }
        public bool success { get; set; }
        public object error { get; set; }
        public int block_num { get; set; }
        public DateTime created_date { get; set; }
        public QuestResult result { get; set; }
        public object steem_price { get; set; }
        public object sbd_price { get; set; }
    }
    public class QuestRewardData
    {
        public TrxInfo trx_info { get; set; }
    }
    public class RewardInfo
    {
        public string type { get; set; }
        public string quest_id { get; set; }
        public string app { get; set; }
        public string n { get; set; }
    }

    public class CardReward
    {
        public string uid { get; set; }
        public int card_detail_id { get; set; }
        public int xp { get; set; }
        public bool gold { get; set; }
        public int edition { get; set; }
    }

    public class QuestReward
    {
        public string type { get; set; }
        public int quantity { get; set; }
        public CardReward card { get; set; }
        public string potion_type { get; set; }
    }

    public class PotionsBoost
    {
        public object legendary { get; set; }
        public object gold { get; set; }
    }

    public class Calc
    {
        public int power { get; set; }
        public int adjusted_power { get; set; }
        public int rating { get; set; }
        public int adjusted_league { get; set; }
        public bool is_capped { get; set; }
    }

    public class QuestResult
    {
        public bool success { get; set; }
        public List<QuestReward> rewards { get; set; }
        public PotionsBoost potions { get; set; }
        public Calc calc { get; set; }
    }
}
