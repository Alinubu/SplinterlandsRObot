namespace SplinterlandsRObot.Player.PlayerSeason
{
    public class SeasonProgress
    {
        public string SeasonChestsProgress(Season? wild, Season? modern)
        {
            int seasonChests = 0;
            double totalRshars = ((modern != null ? modern.rshares : 0) + (wild != null ? wild.rshares : 0));

            seasonChests = CalculateEarnedChests(
                    Math.Max(
                        modern != null ? (int)modern.chest_tier : 0,
                        wild != null ? (int)wild.chest_tier : 0
                        ),
                    totalRshars
                    );            
            return seasonChests.ToString();
        }
        internal int CalculateEarnedChests(int chest_tier, double rshares)
        {
            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.season[chest_tier].@base;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.season[chest_tier].step_multiplier;
            int maxChests = SplinterlandsData.splinterlandsSettings.loot_chests.season[chest_tier].max;
            int chests = 0;
            double sp_limit = (double)baseRshares;

            while (rshares >= Math.Ceiling(sp_limit))
            {
                chests++;
                sp_limit = baseRshares + sp_limit * multiplier;
            }

            return Math.Min(chests, maxChests);
        }
    }
}
