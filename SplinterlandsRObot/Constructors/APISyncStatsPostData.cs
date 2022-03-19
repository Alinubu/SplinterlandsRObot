using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplinterlandsRObot.Constructors
{
    public class APISyncStatsPostData
    {
        public string PassKey { get; set; }
        public List<UserStats> UserStats { get; set; }
    }
}
