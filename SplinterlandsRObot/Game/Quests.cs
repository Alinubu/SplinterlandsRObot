using Newtonsoft.Json;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Constructors;
using SplinterlandsRObot.Hive;

namespace SplinterlandsRObot.Game
{
    public class Quests
    {
        public string GetQuestProgress(int totalCompletedItems, int totalQuestItems)
        {
            string response;

            if (totalCompletedItems < totalQuestItems)
            {
                response = "InProgress:" + totalCompletedItems.ToString() + "/" + totalQuestItems.ToString();
            }
            else if (totalCompletedItems == totalQuestItems)
            {
                response = "Completed:" + totalCompletedItems.ToString() + "/" + totalQuestItems.ToString();
            }
            else
            {
                response = "";
            }
            return response;
        }

        public async Task<bool> ClaimQuestReward(QuestData questData, User user, UserDetails userDetails)
        {
            try
            {
                if (Settings.DONT_CLAIM_QUEST_NEAR_NEXT_LEAGUE)
                {
                    int leagueRating = SplinterlandsData.splinterlandsSettings.leagues[userDetails.league + 1].min_rating;
                    int leaguePower = SplinterlandsData.splinterlandsSettings.leagues[userDetails.league + 1].min_power;

                    if(userDetails.rating >= (leagueRating - 100) && userDetails.collection_power >= leaguePower)
                    {
                        Logs.LogMessage($"{user.Username}: Account rating is near a higher league, quest will not be claimed!", Logs.LOG_ALERT);
                        return false;
                    }
                }
                string tx = new HiveActions().ClaimQuest(user, questData.id);
                Logs.LogMessage($"{user.Username}: Claimed quest reward. Tx:{tx}");

                if (Settings.SHOW_QUEST_REWARDS)
                {
                    Thread.Sleep(15000);
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
                Logs.LogMessage($"{user.Username}: Error at claiming quest rewards: {ex}", Logs.LOG_WARNING);
            }
            return false;
        }
        public async Task<bool> RequestNewQuest(QuestData questData, User user, string questColor, bool questCompleted)
        {
            if (questData != null && Settings.AVOID_SPECIFIC_QUESTS_LIST.Contains(questColor) && !questCompleted)
            {
                if (await new HiveActions().NewQuest(user))
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
                        if (await new HiveActions().StartQuest(user))
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

        public string GetQuestColor(string questName)
        {
            switch (questName)
            {
                case "Defend the Borders":
                    return "Life";
                case "Pirate Attacks":
                    return "Water";
                case "High Priority Targets":
                    return "Snipe";
                case "Lyanna's Call":
                    return "Earth";
                case "Stir the Volcano":
                    return "Fire";
                case "Rising Dead":
                    return "Death";
                case "Stubborn Mercenaries":
                    return "Neutral";
                case "Gloridax Revenge":
                    return "Dragon";
                case "Stealth Mission":
                    return "Sneak";
                default: return "";
            }
        }
    }
}
