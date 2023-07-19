using SplinterlandsRObot.Hive;
using SplinterlandsRObot.Player;
using Newtonsoft.Json.Linq;

namespace SplinterlandsRObot.Player.PlayerFocus
{
    public class Focus
    {
        public string id { get; set; }
        public string player { get; set; }
        public DateTime created_date { get; set; }
        public int created_block { get; set; }
        public string name { get; set; }
        public int total_items { get; set; }
        public int completed_items { get; set; }
        public string? claim_trx_id { get; set; }
        public DateTime? claim_date { get; set; }
        public int reward_qty { get; set; }
        public string? refresh_trx_id { get; set; }
        public object? rewards = null;
        public int? chest_tier { get; set; } = 0;
        public int rshares { get; set; }
        public int earned_chests = 0;

        
        public void CalculateEarnedChests()
        {
            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.quest[(int)chest_tier].@base;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.quest[(int)chest_tier].step_multiplier;
            int chests = 0;
            double fp_limit = baseRshares;

            while (rshares >= fp_limit)
            {
                chests++;
                fp_limit = (double)baseRshares + (double)fp_limit * (double)multiplier;
            }

            earned_chests = Math.Min(chests, SplinterlandsData.splinterlandsSettings.loot_chests.quest[(int)chest_tier].max);
        }
        internal int GetFocusPointsNeeded(int baseRshares, double multiplier)
        {
            int fp_limit = baseRshares;
            while (rshares >= fp_limit)
            {
                fp_limit = Convert.ToInt32(baseRshares + fp_limit * multiplier);
            }
            return fp_limit;
        }

        public bool CanFocusBeClaimed()
        {
            if (IsFocusCompleted() && !IsFocusClaimed() && earned_chests > 0)
                return true;
            return false;
        }

        public async Task<string> ClaimFocusRewards(User user)
        {
            try
            {
                string tx = new HiveService().ClaimQuest(user, id);
                Logs.LogMessage($"{user.Username}: Claimed Daily Focus reward. Tx:{tx}");

                return tx;
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error at claiming Daily Focus rewards: {ex}", Logs.LOG_WARNING);
            }
            return null;
        }

        public string CheckAndStartNewFocus(User user)
        {
            if (IsFocusCompleted())
            {
                if (IsFocusClaimed() && earned_chests > 0)
                {
                    Logs.LogMessage($"{user.Username}: New daily Focus available, requesting from Splinterlands...");
                    string tx = new HiveService().StartFocus(user);
                    if (tx != null)
                    {
                        return tx;
                    }
                    else
                    {
                        Logs.LogMessage($"{user.Username}: Error starting new Focus", Logs.LOG_WARNING);
                    }
                }
                else if (!IsFocusClaimed() && earned_chests == 0)
                {
                    Logs.LogMessage($"{user.Username}: New daily Focus available, requesting from Splinterlands...");
                    string tx = new HiveService().StartFocus(user);
                    if (tx != null)
                    {
                        return tx;
                    }
                    else
                    {
                        Logs.LogMessage($"{user.Username}: Error starting new Focus", Logs.LOG_WARNING);
                    }
                }
                else
                {
                    Logs.LogMessage($"{user.Username}: Cannot start a new Focus because the reward was not yet claimed.", Logs.LOG_WARNING);
                }
            }
            return null;
        }

        public string RequestNewFocus(User user)
        {
            if (!IsFocusCompleted() && !IsFocusClaimed())
            {
                string tx = new HiveService().NewQuest(user);
                if (tx != null)
                {
                    return tx;
                }
            }
            return null;
        }

        public bool IsFocusCompleted()
        {
            string focusResetTime = DateTime.UtcNow.ToString("yyyy-MM-dd") + " 00:00:00";
            if (created_date < DateTime.Parse(focusResetTime))
                return true;
            else 
                return false;
        }

        public bool IsFocusClaimed()
        {
            if (claim_trx_id != null)
                return true;
            return false;
        }

        public bool IsFocusRenewed()
        {
            if (refresh_trx_id != null)
                return true;
            return false;
        }

        public string GetFocusSplinter()
        {
            if (SplinterlandsData.splinterlandsSettings.daily_quests.Where(x => x.active == true && x.name == name).Any())
                return SplinterlandsData.splinterlandsSettings.daily_quests.Where(x => x.active == true && x.name == name).FirstOrDefault().data.value;
            else throw new Exception("Cannot determine Focus Splinter");
        }
        internal bool IsFocusPrio(Random random, Config config)
        {
            double rng = random.NextDouble();
            string focusColor = GetFocusSplinter();

            if (focusColor == "Fire" && config.FocusRateFire >= 0)
                return rng >= (config.FocusRateFire / 100) ? false : true;
            else if (focusColor == "Water" && config.FocusRateWater >= 0)
                return rng >= (config.FocusRateWater / 100) ? false : true;
            else if (focusColor == "Earth" && config.FocusRateEarth >= 0)
                return rng >= (config.FocusRateEarth / 100) ? false : true;
            else if (focusColor == "Life" && config.FocusRateLife >= 0)
                return rng >= (config.FocusRateLife / 100) ? false : true;
            else if (focusColor == "Death" && config.FocusRateDeath >= 0)
                return rng >= (config.FocusRateDeath / 100) ? false : true;
            else if (focusColor == "Dragon" && config.FocusRateDragon >= 0)
                return rng >= (config.FocusRateDragon / 100) ? false : true;

            return rng >= (config.FocusRate / 100) ? false : true;
        }
        public string GetFocusProgress()
        {
            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.quest[(int)chest_tier].@base;
            int maxChests = SplinterlandsData.splinterlandsSettings.loot_chests.quest[(int)chest_tier].max;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.quest[(int)chest_tier].step_multiplier;

            int neededRshares = GetFocusPointsNeeded(baseRshares, multiplier);

            string response = $"{earned_chests}/{maxChests}|{rshares}/{neededRshares}";
            return response;
        }
    }
}
