using SplinterlandsRObot.Player;

namespace SplinterlandsRObot.Models.Bot
{
    public class APISyncStatsPostData
    {
        public string PassKey { get; set; }
        public List<UserStats> UserStats { get; set; }
    }
}
