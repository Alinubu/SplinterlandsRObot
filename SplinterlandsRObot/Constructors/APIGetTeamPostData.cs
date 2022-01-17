using Newtonsoft.Json.Linq;

namespace SplinterlandsRObot.Constructors
{
    public class APIGetTeamPostData
    {
        public JToken matchDetails { get; set; }
        public string questColor { get; set; }
        public bool questCompleted { get; set; }
        public CardsCollection playerCards { get; set; }
        public string username { get; set; }
        public JToken? enemyData { get; set; }
    }
}
