namespace SplinterlandsRObot.Models.Splinterlands
{
    public class SplinterlandsSettings
    {
        public double gold_percent { get; set; }
        public int market_fee { get; set; }
        public int num_editions { get; set; }
        public List<int> core_editions { get; set; }
        public List<int> starter_editions { get; set; }
        public List<int> soulbound_editions { get; set; }
        public List<double> rarity_pcts { get; set; }
        public List<List<int>> xp_levels { get; set; }
        public List<int> alpha_xp { get; set; }
        public List<int> gold_xp { get; set; }
        public List<int> beta_xp { get; set; }
        public List<int> beta_gold_xp { get; set; }
        public List<List<int>> combine_rates { get; set; }
        public List<List<int>> combine_rates_gold { get; set; }
        public List<League> leagues { get; set; }
        public int transfer_cooldown_blocks { get; set; }
        public DateTime untamed_edition_date { get; set; }
        public List<string> active_auth_ops { get; set; }
        public string version { get; set; }
        public int config_version { get; set; }
        public int battles_disabled { get; set; }
        public int new_rewards_season { get; set; }
        public LootChests loot_chests { get; set; }
        public List<DailyQuest> daily_quests { get; set; }
        public List<string> rpc_nodes { get; set; }
        public double dec_price { get; set; }
        public double sps_price { get; set; }
        public bool maintenance_mode { get; set; }
        public Season season { get; set; }
        public int last_block { get; set; }
        public long timestamp { get; set; }
        public ChainProps chain_props { get; set; }
        public List<string> api_ops { get; set; }
    }
    public class Boosts
    {
        public Bronze bronze { get; set; }
        public Silver silver { get; set; }
        public Gold gold { get; set; }
        public Diamond diamond { get; set; }
        public Champion champion { get; set; }
    }
    public class Bronze
    {
        public double rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }
    public class Silver
    {
        public int rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }
    public class Gold
    {
        public double rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }
    public class Diamond
    {
        public int rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }
    public class Champion
    {
        public int rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }
    public class ChainProps
    {
        public DateTime time { get; set; }
        public int ref_block_num { get; set; }
        public string ref_block_id { get; set; }
        public long ref_block_prefix { get; set; }
    }
    public class DailyQuest
    {
        public string name { get; set; }
        public bool active { get; set; }
        public string objective_type { get; set; }
        public int min_rating { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string color { get; set; }
        public string action { get; set; }
        public string splinter { get; set; }
        public string value { get; set; }
        public string description { get; set; }
    }
    
    public class League
    {
        public string name { get; set; }
        public string group { get; set; }
        public int league_limit { get; set; }
        public int level { get; set; }
        public int min_rating { get; set; }
        public int min_power { get; set; }
        public int season_rating_reset { get; set; }
    }
    public class LootChests
    {
        public List<QuestLoot> quest { get; set; }
        public List<Season> season { get; set; }
        public Boosts boosts { get; set; }
    }
    public class QuestLoot
    {
        public int @base { get; set;}
        public int max { get; set; }
        public double step_multiplier { get; set; }
    }
    public class Season
    {
        public int @base { get; set; }
        public double step_multiplier { get; set; }
        public int max { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public DateTime ends { get; set; }
        public List<string> reward_packs { get; set; }
        public object reset_block_num { get; set; }
    }
}
