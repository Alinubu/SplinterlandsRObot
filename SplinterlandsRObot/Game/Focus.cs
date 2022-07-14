using SplinterlandsRObot.Hive;
using SplinterlandsRObot.Models.Account;

namespace SplinterlandsRObot.Game
{
    public class Focus
    {
        public string GetQuestProgress(int totalEarnedChests, int chest_tier, double rshares)
        {
            

            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].@base;
            int maxChests = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].max;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].step_multiplier;

            int neededRshares = GetFocusPointsNeeded(baseRshares, multiplier, rshares);

            string response = $"{totalEarnedChests}/{maxChests}|{rshares}/{neededRshares}]";
            return response;
        }

        internal int CalculateEarnedChests(int chest_tier, double rshares)
        {
            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].@base;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].step_multiplier;
            int chests = 0;
            int fp_limit = baseRshares;

            while (rshares >= fp_limit)
            {
                chests++;
                fp_limit = Convert.ToInt32(baseRshares + fp_limit * multiplier);
            }

            return Math.Min(chests,30);
        }

        internal int GetFocusPointsNeeded(int baseRshares, double multiplier, double rshares)
        {
            int fp_limit = baseRshares;
            while (rshares >= fp_limit)
            {
                fp_limit = Convert.ToInt32(baseRshares + fp_limit * multiplier);
            }
            return fp_limit;
        }

        public async Task<string> ClaimQuestReward(Quest questData, User user)
        {
            try
            {
                string tx = new HiveActions().ClaimQuest(user, questData.id);
                Logs.LogMessage($"{user.Username}: Claimed Daily Focus reward. Tx:{tx}");

                return tx;
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error at claiming Daily Focus rewards: {ex}", Logs.LOG_WARNING);
            }
            return null;
        }

        public string RequestNewFocus(Quest questData, User user)
        {
            bool focusCompleted = false;
            if ((DateTime.Now - questData.created_date.ToLocalTime()).TotalHours > 24 && questData.claim_trx_id != null)
                focusCompleted = true;
            else if ((DateTime.Now - questData.created_date.ToLocalTime()).TotalHours < 24 && questData.claim_trx_id == null)
                focusCompleted = false;

            if (questData != null && !focusCompleted)
            {
                string tx = new HiveActions().NewQuest(user);
                if (tx != null)
                {
                    return tx;
                }
            }
            return null;
        }

        public string RequestNewFocus(User user)
        {
            string tx = new HiveActions().NewQuest(user);
            if (tx != null)
            {
                return tx;
            }
            return null;
        }

        public async Task<string> CheckForNewFocus(Quest questData, User user)
        {
            if (questData != null)
            {
                if ((DateTime.Now - questData.created_date.ToLocalTime()).TotalHours > 24)
                {
                    if (questData.claim_trx_id != null && questData.earned_chests > 0)
                    {
                        Logs.LogMessage($"{user.Username}: New daily Focus available, requesting from Splinterlands...");
                        string tx = new HiveActions().StartFocus(user);
                        if (tx != null)
                        {
                            Logs.LogMessage($"{user.Username}: New Focus started", Logs.LOG_SUCCESS);
                            return tx;
                        }
                        else
                        {
                            Logs.LogMessage($"{user.Username}: Error starting new Focus", Logs.LOG_WARNING);
                        }
                    }
                    else if (questData.claim_trx_id == null && questData.earned_chests == 0)
                    {
                        Logs.LogMessage($"{user.Username}: New daily Focus available, requesting from Splinterlands...");
                        string tx = new HiveActions().StartFocus(user);
                        if (tx != null)
                        {
                            Logs.LogMessage($"{user.Username}: New Focus started", Logs.LOG_SUCCESS);
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
            }
            return null;
        }

        public string GetCardsColorByQuestType(string questTitle)
        {
            string questType = questTitle.Split(' ').GetValue(0).ToString();

            string response = questType.ToUpper() switch
            {
                "FIRE" => "Red",
                "WATER" => "Blue",
                "LIFE" => "White",
                "DEATH" => "Black",
                "EARTH" => "Green",
                "DRAGON" => "Gold",
                _ => "XXX",
            };
            return response;
        }

        public string GetFocusSplinter(string questName)
        {
            if (SplinterlandsData.splinterlandsSettings.daily_quests.Where(x => x.active == true && x.name == questName).Any())
                return SplinterlandsData.splinterlandsSettings.daily_quests.Where(x => x.active == true && x.name == questName).FirstOrDefault().data.value;
            else
            {
                return "THISWILLBEREMOVEDATSOMEPOINT";
            }

            return "";
        }

        internal bool IsFocusPrio(Random random, string focusColor, Config config)
        {
            double rng = random.NextDouble();

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
    }
}
