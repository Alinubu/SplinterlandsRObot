﻿using Newtonsoft.Json.Linq;

namespace SplinterlandsRObot.Models
{
    public class APIGetTeamPostData
    {
        public JToken matchDetails { get; set; }
        
        public JToken questDetails { get; set; }
        public CardsCollection playerCards { get; set; }
        public string preferredSummoners { get; set; }
        public string username { get; set; }
        public int league { get; set; }
        public bool replaceStarterCards { get; set; }
        public bool useStarterCards { get; set; }
        public bool prioritizeFocus { get; set; }
    }
}