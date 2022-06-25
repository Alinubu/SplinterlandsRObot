using Newtonsoft.Json.Linq;

namespace SplinterlandsRObot.Models
{
    public class SplinterlandsCards
    {
        public List<SplinterlandsCard> cards { get; set; }
    }
    public class SplinterlandsCard
    {
        public int id { get; set; }
        public string name { get; set; }
        public string color { get; set; }
        public string type { get; set; }
        public object? sub_type { get; set; }
        public int rarity { get; set; }
        public int drop_rate { get; set; }
        public object? stats { get; set; }
        public bool is_starter { get; set; }
        public string editions { get; set; }
        public int? created_block_num { get; set; }
        public string? last_update_tx { get; set; }
        public int total_printed { get; set; }
        public bool is_promo { get; set; }
        public int? tier { get; set; }
        public object? distribution { get; set; }
    }
    public class Stats
    {
        public JToken mana { get; set; }
        public JToken attack { get; set; }
        public JToken ranged { get; set; }
        public JToken magic { get; set; }
        public JToken armor { get; set; }
        public JToken health { get; set; }
        public JToken speed { get; set; }
        public List<object> abilities { get; set; }
    }
    public class Distribution
    {
        public int card_detail_id { get; set; }
        public bool gold { get; set; }
        public int edition { get; set; }
        public string num_cards { get; set; }
        public string total_xp { get; set; }
        public JToken num_burned { get; set; }
        public JToken total_burned_xp { get; set; }
    }
}
