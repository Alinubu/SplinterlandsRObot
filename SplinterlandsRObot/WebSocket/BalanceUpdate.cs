using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplinterlandsRObot.Models
{
    public class BalanceUpdate
    {
        public string id { get; set; }
        public string msg_id { get; set; }
        public BalanceUpdateData data { get; set; }
    }
    public class BalanceUpdateData
    {
        public string player { get; set; }
        public string token { get; set; }
        public double amount { get; set; }
        public double balance_start { get; set; }
        public double balance_end { get; set; }
        public int block_num { get; set; }
        public string trx_id { get; set; }
        public string type { get; set; }
        public DateTime created_date { get; set; }
    }
}
