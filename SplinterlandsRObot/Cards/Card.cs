using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplinterlandsRObot.Cards
{
    public class Card
    {
        public string player { get; set; }
        public string uid { get; set; }
        public string card_detail_id { get; set; }
        public int xp { get; set; }
        public bool gold { get; set; }
        public int edition { get; set; }
        public string? market_id { get; set; }
        public string? buy_price { get; set; }
        public string? price_limit { get; set; }
        public string? days_to_rent { get; set; }
        public string? currency { get; set; }
        public string? market_listing_type { get; set; }
        public int? market_listing_status { get; set; }
        public int? last_used_block { get; set; }
        public string? last_used_player { get; set; }
        public DateTime? last_used_date { get; set; }
        public object? last_transferred_block { get; set; }
        public object? last_transferred_date { get; set; }
        public int? alpha_xp { get; set; }
        public string? delegated_to { get; set; }
        public string? delegation_tx { get; set; }
        public object? skin { get; set; }
        public string? delegated_to_display_name { get; set; }
        public object? display_name { get; set; }
        public int? lock_days { get; set; }
        public object? unlock_date { get; set; }
        public int? level { get; set; }
        public bool? renew_allowed { get; set; }
    }
}
