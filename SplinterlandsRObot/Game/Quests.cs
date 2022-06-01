using Newtonsoft.Json;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Constructors;
using SplinterlandsRObot.Hive;

namespace SplinterlandsRObot.Game
{
    public class Quests
    {
        public string GetQuestProgress(int totalEarnedChests, int chest_tier, double rshares)
        {
            

            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].@base;
            int maxChests = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].max;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].step_multiplier;

            int neededRshares = totalEarnedChests == 0 ? baseRshares : totalEarnedChests == 1 ? Convert.ToInt32(baseRshares + (baseRshares * multiplier)) : Convert.ToInt32((totalEarnedChests - 1) * (baseRshares * multiplier) + baseRshares);

            string response = $"{totalEarnedChests}/{maxChests}|{rshares}/{neededRshares}";
            return response;
        }

        internal int CalculateEarnedChests(int chest_tier, double rshares)
        {
            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].@base;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].step_multiplier;
            int chests = 0;
            int fp_limit = baseRshares;

            while (rshares > fp_limit)
            {
                chests++;
                fp_limit = Convert.ToInt32(baseRshares + fp_limit * multiplier);
            }

            return chests;
        }

        public async Task<bool> ClaimQuestReward(QuestData questData, User user, UserDetails userDetails)
        {
            try
            {
                string tx = new HiveActions().ClaimQuest(user, questData.id);
                Logs.LogMessage($"{user.Username}: Claimed Daily Focus reward. Tx:{tx}");

                if (Settings.SHOW_QUEST_REWARDS)
                {
                    await Task.Delay(15000);
                    string txResponse = await new Splinterlands().GetTransactionDetails(tx);
                    string responseClean = txResponse.Replace("\"{", "{").Replace("}\"", "}").Replace(@"\", "");

                    QuestRewardData rewardData = JsonConvert.DeserializeObject<QuestRewardData>(responseClean);
                    if (rewardData.trx_info.result.success == true)
                    {
                        string[] rewards = new string[rewardData.trx_info.result.rewards.Count];
                        int i = 0;
                        foreach (QuestReward reward in rewardData.trx_info.result.rewards)
                        {
                            if (reward.type == "reward_card")
                            {
                                rewards[i] = $"{reward.quantity} x {(reward.card.gold ? "(Gold)" : "")} {SplinterlandsData.splinterlandsCards.Where(x => x.id == reward.card.card_detail_id).First().name}";
                            }
                            else if (reward.type == "potion")
                            {
                                rewards[i] = $"{reward.quantity} x {(reward.potion_type == "gold" ? "Gold" : "Legedary")} Potion";
                            }
                            else if (reward.type == "credits")
                            {
                                rewards[i] = $"{reward.quantity} x Credits";
                            }
                            else if (reward.type == "dec")
                            {
                                rewards[i] = $"{reward.quantity} x DEC";
                            }
                            else if (reward.type == "pack")
                            {
                                rewards[i] = $"{reward.quantity} x Packs";
                            }
                            i++;
                        }

                        Logs.OutputQuestRewards(user.Username, rewards);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error at claiming Daily Focus rewards: {ex}", Logs.LOG_WARNING);
            }
            return false;
        }
        public bool RequestNewQuest(QuestData questData, User user, string questColor)
        {
            bool questCompleted = false;
            if ((DateTime.Now - questData.created_date.ToLocalTime()).TotalHours > 24 && questData.claim_trx_id != null)
                questCompleted = true;
            else if ((DateTime.Now - questData.created_date.ToLocalTime()).TotalHours < 24 && questData.claim_trx_id == null)
                questCompleted = true;

            if (questData != null && Settings.AVOID_SPECIFIC_QUESTS_LIST.Contains(questColor) && !questCompleted)
            {
                if (new HiveActions().NewQuest(user))
                {
                    return true;
                }
            }
            return false;
        }

        public bool RequestNewFocus(User user)
        {
            if (new HiveActions().NewQuest(user))
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CheckForNewQuest(QuestData questData, User user)
        {
            if (questData != null)
            {
                if ((DateTime.Now - questData.created_date.ToLocalTime()).TotalHours > 24)
                {
                    if (questData.claim_trx_id != null && questData.earned_chests > 0)
                    {
                        Logs.LogMessage($"{user.Username}: New daily Focus available, requesting from Splinterlands...");
                        if (new HiveActions().StartQuest(user))
                        {
                            Logs.LogMessage($"{user.Username}: New Focus started", Logs.LOG_SUCCESS);
                            return true;
                        }
                        else
                        {
                            Logs.LogMessage($"{user.Username}: Error starting new Focus", Logs.LOG_WARNING);
                        }
                    }
                    else if (questData.claim_trx_id == null && questData.earned_chests == 0)
                    {
                        Logs.LogMessage($"{user.Username}: New daily Focus available, requesting from Splinterlands...");
                        if (new HiveActions().StartQuest(user))
                        {
                            Logs.LogMessage($"{user.Username}: New Focus started", Logs.LOG_SUCCESS);
                            return true;
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
            return false;
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

        public string GetQuestColor(string questName, User user)
        {
            if (SplinterlandsData.splinterlandsSettings.daily_quests.Where(x => x.active == true && x.name == questName).Any())
                return SplinterlandsData.splinterlandsSettings.daily_quests.Where(x => x.active == true && x.name == questName).FirstOrDefault().data.value;
            else
            {
                return "THISWILLBEREMOVEDATSOMEPOINT";
            }

            return "";
        }

        internal bool IsFocusPrio(Random random, string focusColor)
        {
            double rng = random.NextDouble();

            if (focusColor == "Fire" && Settings.SPLINTER_FOCUS_FIRE >= 0)
                return rng >= (Settings.SPLINTER_FOCUS_FIRE/100) ? false : true;
            else if (focusColor == "Water" && Settings.SPLINTER_FOCUS_WATER >= 0)
                return rng >= (Settings.SPLINTER_FOCUS_WATER / 100) ? false : true;
            else if (focusColor == "Earth" && Settings.SPLINTER_FOCUS_EARTH >= 0)
                return rng >= (Settings.SPLINTER_FOCUS_EARTH / 100) ? false : true;
            else if (focusColor == "Life" && Settings.SPLINTER_FOCUS_LIFE >= 0)
                return rng >= (Settings.SPLINTER_FOCUS_LIFE / 100) ? false : true;
            else if (focusColor == "Death" && Settings.SPLINTER_FOCUS_DEATH >= 0)
                return rng >= (Settings.SPLINTER_FOCUS_DEATH / 100) ? false : true;
            else if (focusColor == "Dragon" && Settings.SPLINTER_FOCUS_DRAGON >= 0)
                return rng >= (Settings.SPLINTER_FOCUS_DRAGON / 100) ? false : true;

            return rng >= (Settings.FOCUS_RATE / 100) ? false : true;
        }
    }
}
