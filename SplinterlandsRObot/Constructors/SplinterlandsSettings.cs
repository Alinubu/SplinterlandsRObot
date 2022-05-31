namespace SplinterlandsRObot.Constructors
{
    public class SplinterlandsSettings
    {
        public string asset_url { get; set; }
        public double gold_percent { get; set; }
        public int starter_pack_price { get; set; }
        public int booster_pack_price { get; set; }
        public int market_fee { get; set; }
        public int num_editions { get; set; }
        public List<int> core_editions { get; set; }
        public List<int> starter_editions { get; set; }
        public List<int> soulbound_editions { get; set; }
        public List<string> event_creation_whitelist { get; set; }
        public List<BatEventList> bat_event_list { get; set; }
        public int event_entry_fee_required { get; set; }
        public int max_event_entrants { get; set; }
        public int tournaments_creation_fee_dec { get; set; }
        public string account { get; set; }
        public bool stats { get; set; }
        public List<double> rarity_pcts { get; set; }
        public List<List<int>> xp_levels { get; set; }
        public List<int> alpha_xp { get; set; }
        public List<int> gold_xp { get; set; }
        public List<int> beta_xp { get; set; }
        public List<int> beta_gold_xp { get; set; }
        public List<List<int>> combine_rates { get; set; }
        public List<List<int>> combine_rates_gold { get; set; }
        public Battles battles { get; set; }
        public int multi_lb_start_season { get; set; }
        public LeaderboardPrizes leaderboard_prizes { get; set; }
        public List<League> leagues { get; set; }
        public Dec dec { get; set; }
        public Guilds guilds { get; set; }
        public object? barracks_perks { get; set; }
        public object? frays { get; set; }
        public List<SupportedCurrency> supported_currencies { get; set; }
        public int transfer_cooldown_blocks { get; set; }
        public DateTime untamed_edition_date { get; set; }
        public List<string> active_auth_ops { get; set; }
        public string version { get; set; }
        public int config_version { get; set; }
        public LandSale land_sale { get; set; }
        public ChaosLegion chaos_legion { get; set; }
        public List<Potion> potions { get; set; }
        public List<string> promotions { get; set; }
        public Sps sps { get; set; }
        public int battles_disabled { get; set; }
        public int new_rewards_season { get; set; }
        public LootChests loot_chests { get; set; }
        public List<DailyQuest> daily_quests { get; set; }
        public List<string> rpc_nodes { get; set; }
        public double dec_price { get; set; }
        public double sps_price { get; set; }
        public bool maintenance_mode { get; set; }
        public Season season { get; set; }
        public BrawlCycle brawl_cycle { get; set; }
        public List<Quest> quests { get; set; }
        public List<GuildStoreItem> guild_store_items { get; set; }
        public int last_block { get; set; }
        public long timestamp { get; set; }
        public ChainProps chain_props { get; set; }
        public int circle_payments_enabled { get; set; }
        public int transak_payments_enabled { get; set; }
        public int zendesk_enabled { get; set; }
        public int dec_max_buy_amount { get; set; }
        public int sps_max_buy_amount { get; set; }
        public bool show_special_store { get; set; }
        public string paypal_acct { get; set; }
        public string paypal_merchant_id { get; set; }
        public string paypal_client_id { get; set; }
        public bool paypal_sandbox { get; set; }
        public Ssc ssc { get; set; }
        public List<CardHoldingAccount> card_holding_accounts { get; set; }
        public Bridge bridge { get; set; }
        public Ethereum ethereum { get; set; }
        public Wax wax { get; set; }
        public SpsAirdrop sps_airdrop { get; set; }
        public List<string> api_ops { get; set; }
    }

    public class Abi
    {
        public string status { get; set; }
        public string message { get; set; }
        public string result { get; set; }
    }

    public class Airdrop
    {
        public string name { get; set; }
        public int id { get; set; }
        public double chance { get; set; }
        public int gold_guarantee { get; set; }
        public DateTime claim_date { get; set; }
        public double? gold_chance { get; set; }
    }

    public class Arena
    {
        public List<Cost> cost { get; set; }
    }

    public class Atomicassets
    {
        public string account { get; set; }
    }

    public class Barracks
    {
        public List<Cost> cost { get; set; }
    }

    public class BatEventList
    {
        public string id { get; set; }
        public int bat { get; set; }
    }

    public class Battles
    {
        public string asset_url { get; set; }
        public int default_expiration_seconds { get; set; }
        public int reveal_blocks { get; set; }
        public int win_streak_wins { get; set; }
        public List<Ruleset> rulesets { get; set; }
    }

    public class Bonuse
    {
        public int min { get; set; }
        public int bonus_pct { get; set; }
    }

    public class Boosts
    {
        public Bronze bronze { get; set; }
        public Silver silver { get; set; }
        public Gold gold { get; set; }
        public Diamond diamond { get; set; }
        public Champion champion { get; set; }
    }

    public class BrawlCycle
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime start { get; set; }
        public int status { get; set; }
        public object reset_block_num { get; set; }
        public DateTime end { get; set; }
    }

    public class Bridge
    {
        public Ethereum ethereum { get; set; }
        public Bsc bsc { get; set; }
    }

    public class Bronze
    {
        public double rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }

    public class Bsc
    {
        public object? DEC { get; set; }
        public object? SPS { get; set; }
    }

    public class CardHoldingAccount
    {
        public string blockchainName { get; set; }
        public string accountName { get; set; }
    }

    public class Cards
    {
        public Abi abi { get; set; }
        public string address { get; set; }
    }

    public class ChainProps
    {
        public DateTime time { get; set; }
        public int ref_block_num { get; set; }
        public string ref_block_id { get; set; }
        public long ref_block_prefix { get; set; }
    }

    public class Champion
    {
        public int rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }

    public class ChaosLegion
    {
        public DateTime pre_sale_start { get; set; }
        public List<Airdrop> airdrops { get; set; }
        public DateTime pre_sale_end { get; set; }
        public DateTime voucher_drop_start { get; set; }
        public DateTime sale2_end { get; set; }
        public DateTime sale3_start { get; set; }
        public int voucher_drop_duration { get; set; }
        public DateTime main_sale_start { get; set; }
        public int voucher_drop_rate { get; set; }
        public int pack_price { get; set; }
    }

    public class Contracts
    {
        public Cards cards { get; set; }
        public Crystals crystals { get; set; }
        public Payments payments { get; set; }
    }

    public class Cost
    {
        public string symbol { get; set; }
        public List<int> levels { get; set; }
        public int amount { get; set; }
    }

    public class Crystals
    {
        public Abi abi { get; set; }
        public string address { get; set; }
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

    public class Dec
    {
        public int tokens_per_block { get; set; }
        public int eth_withdrawal_fee { get; set; }
        public int gold_burn_bonus_2 { get; set; }
        public int curve_reduction { get; set; }
        public double beta_bonus { get; set; }
        public int curve_constant { get; set; }
        public int start_block { get; set; }
        public int reduction_blocks { get; set; }
        public int reduction_pct { get; set; }
        public int pool_size_blocks { get; set; }
        public double ecr_regen_rate { get; set; }
        public double ecr_reduction_rate { get; set; }
        public double alpha_bonus { get; set; }
        public double gold_bonus { get; set; }
        public double streak_bonus { get; set; }
        public double streak_bonus_max { get; set; }
        public List<int> burn_rate { get; set; }
        public List<int> untamed_burn_rate { get; set; }
        public int alpha_burn_bonus { get; set; }
        public int promo_burn_bonus { get; set; }
        public int gold_burn_bonus { get; set; }
        public double max_burn_bonus { get; set; }
        public int orbs_available { get; set; }
        public int orb_cost { get; set; }
        public int dice_available { get; set; }
        public int dice_cost { get; set; }
        public int mystery_potion_blocks { get; set; }
        public int pool_cut_pct { get; set; }
        public string prize_pool_account { get; set; }
    }

    public class Diamond
    {
        public int rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }

    public class Ethereum
    {
        public object? DEC { get; set; }
        public object? SPS { get; set; }
        public int withdrawal_fee { get; set; }
        public int sps_withdrawal_fee { get; set; }
        public Contracts contracts { get; set; }
    }

    public class External
    {
        public Token token { get; set; }
        public Atomicassets atomicassets { get; set; }
    }

    public class Gold
    {
        public double rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }

    public class GuildHall
    {
        public string symbol { get; set; }
        public List<int> levels { get; set; }
        public List<int> member_limit { get; set; }
    }

    public class Guilds
    {
        public List<double> crown_multiplier { get; set; }
        public List<int> shop_discount_pct { get; set; }
        public List<string> rank_names { get; set; }
        public GuildHall guild_hall { get; set; }
        public int brawl_prep_duration { get; set; }
        public GuildShop guild_shop { get; set; }
        public List<double> merit_multiplier { get; set; }
        public List<int> dec_bonus_pct { get; set; }
        public int creation_fee { get; set; }
        public int brawl_combat_duration { get; set; }
        public int brawl_results_duration { get; set; }
        public int brawl_cycle_end_offset { get; set; }
        public int brawl_staggered_start_interval { get; set; }
        public int current_fray_edition { get; set; }
        public Arena arena { get; set; }
        public List<double> crown_split_pct { get; set; }
        public Barracks barracks { get; set; }
        public QuestLodge quest_lodge { get; set; }
        public int max_brawl_size { get; set; }
        public int merit_constant { get; set; }
    }

    public class GuildShop
    {
        public List<Cost> cost { get; set; }
    }

    public class GuildStoreItem
    {
        public string name { get; set; }
        public string short_desc { get; set; }
        public int unlock_level { get; set; }
        public Cost cost { get; set; }
        public string icon { get; set; }
        public string icon_sm { get; set; }
        public string color { get; set; }
        public string unit_of_purchase { get; set; }
        public string symbol { get; set; }
        public string plural { get; set; }
    }

    public class LandSale
    {
        public int plot_price { get; set; }
        public int tract_price { get; set; }
        public int region_price { get; set; }
        public int plots_available { get; set; }
        public int plot_plots { get; set; }
        public int tract_plots { get; set; }
        public int region_plots { get; set; }
        public DateTime start_date { get; set; }
    }

    public class LeaderboardPrizes
    {
        public List<object> Novice { get; set; }
        public List<int> Bronze { get; set; }
        public List<int> Silver { get; set; }
        public List<int> Gold { get; set; }
        public List<int> Diamond { get; set; }
        public List<int> Champion { get; set; }
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

    public class Networks
    {
        public string eth { get; set; }
        public string bsc { get; set; }
    }

    public class Payments
    {
        public Abi abi { get; set; }
        public string address { get; set; }
    }

    public class Potion
    {
        public string id { get; set; }
        public string name { get; set; }
        public int item_id { get; set; }
        public int price_per_charge { get; set; }
        public int value { get; set; }
        public List<Bonuse> bonuses { get; set; }
    }

    //public class Quest
    //{
    //    public int @base { get; set; }
    //    public double step_multiplier { get; set; }
    //    public int max { get; set; }
    //}

    public class Quest
    {
        public string name { get; set; }
        public bool active { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string objective { get; set; }
        public string objective_short { get; set; }
        public string objective_type { get; set; }
        public int item_total { get; set; }
        public int reward_qty { get; set; }
        public int min_rating { get; set; }
        public List<string> match_types { get; set; }
        public List<int> reward_qty_by_league { get; set; }
        public Data data { get; set; }
        public string icon { get; set; }
    }

    public class QuestLodge
    {
        public string symbol { get; set; }
        public List<int> levels { get; set; }
    }

    public class Ruleset
    {
        public bool active { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public List<string> invalid { get; set; }
        public int? weight { get; set; }
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

    public class Silver
    {
        public int rarity_boost { get; set; }
        public int token_multiplier { get; set; }
    }

    public class Sps
    {
        public int unstaking_interval_seconds { get; set; }
        public StakingRewardsVoucher staking_rewards_voucher { get; set; }
        public StakingRewards staking_rewards { get; set; }
        public int unstaking_periods { get; set; }
        public int staking_rewards_voucher_last_reward_block { get; set; }
        public double staking_rewards_voucher_acc_tokens_per_share { get; set; }
        public double staking_rewards_acc_tokens_per_share { get; set; }
        public int staking_rewards_last_reward_block { get; set; }
    }

    public class SpsAirdrop
    {
        public DateTime start_date { get; set; }
        public int current_airdrop_day { get; set; }
        public double sps_per_day { get; set; }
    }

    public class Ssc
    {
        public string rpc_url { get; set; }
        public string chain_id { get; set; }
        public string hive_rpc_url { get; set; }
        public string hive_chain_id { get; set; }
        public string alpha_token { get; set; }
        public string beta_token { get; set; }
        public string pack_holding_account { get; set; }
    }

    public class StakingRewards
    {
        public double tokens_per_block { get; set; }
        public int reduction_blocks { get; set; }
        public int reduction_pct { get; set; }
        public int start_block { get; set; }
    }

    public class StakingRewardsVoucher
    {
        public double tokens_per_block { get; set; }
        public int start_block { get; set; }
    }

    public class SupportedCurrency
    {
        public string name { get; set; }
        public string currency { get; set; }
        public string type { get; set; }
        public bool tournament_enabled { get; set; }
        public bool payment_enabled { get; set; }
        public double usd_value { get; set; }
        public int precision { get; set; }
        public string contract_address { get; set; }
        public string payment_address { get; set; }
        public Networks networks { get; set; }
        public string token_id { get; set; }
        public string asset_name { get; set; }
        public string symbol { get; set; }
    }

    public class Token
    {
        public string account { get; set; }
    }

    public class Wax
    {
        public bool login_enabled { get; set; }
        public string client_id { get; set; }
        public string auth_url { get; set; }
        public External external { get; set; }
    }


}
