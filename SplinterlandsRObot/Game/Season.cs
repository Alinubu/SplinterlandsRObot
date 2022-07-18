using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplinterlandsRObot.Game
{
    public class Season
    {
        public string GetSeasonProgress(int totalEarnedChests, int chest_tier, double rshares)
        {
            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.season[chest_tier].@base;
            int maxChests = SplinterlandsData.splinterlandsSettings.loot_chests.season[chest_tier].max;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.season[chest_tier].step_multiplier;

            int neededRshares = GetSeasonPointsNeeded(baseRshares, multiplier, rshares);

            string response = $"{totalEarnedChests}/{maxChests}|{rshares}/{neededRshares}";
            return response;
        }

        internal int GetSeasonPointsNeeded(int baseRshares, double multiplier, double rshares)
        {
            int fp_limit = baseRshares;
            while (rshares >= fp_limit)
            {
                fp_limit = Convert.ToInt32(baseRshares + fp_limit * multiplier);
            }
            return fp_limit;
        }

        internal int CalculateEarnedChests(int chest_tier, double rshares)
        {
            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.season[chest_tier].@base;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.season[chest_tier].step_multiplier;
            int chests = 0;
            int sp_limit = baseRshares;

            while (rshares >= sp_limit)
            {
                chests++;
                sp_limit = Convert.ToInt32(baseRshares + sp_limit * multiplier);
            }

            return Math.Min(chests, 150);
        }
    }
}
