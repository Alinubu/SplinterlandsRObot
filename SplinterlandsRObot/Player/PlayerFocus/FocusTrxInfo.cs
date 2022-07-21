namespace SplinterlandsRObot.Player.PlayerFocus
{
    public class FocusTrxInfo
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
        public FocusResult result { get; set; }
        public object steem_price { get; set; }
        public object sbd_price { get; set; }
    }
}
