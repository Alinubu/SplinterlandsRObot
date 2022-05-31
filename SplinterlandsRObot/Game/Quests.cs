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

            string response = $"C:{totalEarnedChests}/{maxChests}|FP:{rshares}/{neededRshares}";
            return response;
        }

        internal int CalculateEarnedChests(int chest_tier, double rshares)
        {
            int baseRshares = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].@base;
            double multiplier = SplinterlandsData.splinterlandsSettings.loot_chests.quest[chest_tier].step_multiplier;
            int chests = 0;

            if (rshares < baseRshares)
            {
                chests = 0;
            }
            else if (rshares > baseRshares && rshares < Convert.ToInt32(baseRshares + (baseRshares * multiplier)))
            {
                chests = 1;
            }
            else
            {
                chests = Convert.ToInt32(((rshares - baseRshares) / (baseRshares * multiplier)) + 1);
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
        public bool RequestNewQuest(QuestData questData, User user, string questColor, bool questCompleted)
        {
            if (questData != null && Settings.AVOID_SPECIFIC_QUESTS_LIST.Contains(questColor) && !questCompleted)
            {
                if (new HiveActions().NewQuest(user))
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> CheckForNewQuest(QuestData questData, User user, bool questCompleted)
        {
            if (questData != null)
            {
                if (questCompleted && (DateTime.Now - questData.created_date.ToLocalTime()).TotalHours > 23)
                {
                    if (questData.claim_trx_id != null)
                    {
                        Logs.LogMessage($"{user.Username}: New Quest available, requesting from Splinterlands...");
                        if (new HiveActions().StartQuest(user))
                        {
                            Logs.LogMessage($"{user.Username}: New Quest started", Logs.LOG_SUCCESS);
                            return true;
                        }
                        else
                        {
                            Logs.LogMessage($"{user.Username}: Error starting new Quest", Logs.LOG_WARNING);
                        }
                    }
                    else
                    {
                        Logs.LogMessage($"{user.Username}: Cannot start a new quest because the reward was not yet claimed.", Logs.LOG_WARNING);
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
                Logs.LogMessage($"{user.Username}: Cannot find Focus name. Maybe old quest active, requesting a new Focus", Logs.LOG_ALERT);
                if (new HiveActions().StartQuest(user))
                {
                    Thread.Sleep(10000);
                    return "THISWILLBEREMOVEDATSOMEPOINT";
                }
            }

            return "";
        }
    }
}
