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

        public bool ClaimQuestReward(QuestData questData, User user)
        {
            try
            {
                string tx = new HiveActions().ClaimQuest(user, questData.id);
                Logs.LogMessage($"{user.Username}: Claimed quest reward:{tx}");
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
                    Logs.LogMessage($"{user.Username}: New Quest available, requesting from Splinterlands...");
                    if (await new HiveActions().StartQuest(user))
                    {
                        //APICounter = 99;
                        Logs.LogMessage($"{user.Username}: New Quest started", Logs.LOG_SUCCESS);
                        return true;
                    }
                    else
                    {
                        Logs.LogMessage($"{user.Username}: Error starting new Quest", Logs.LOG_WARNING);
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
