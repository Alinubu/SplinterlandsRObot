using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Constructors;
using SplinterlandsRObot.Extensions;
using SplinterlandsRObot.Net;

namespace SplinterlandsRObot.API
{
    public class Bot
    {
        const string BOT_PUBLIC_API_GET_TEAM = "/api/public/PublicTeam/";
        const string BOT_PRIVATE_API_GET_TEAM = "/api/private/PrivateTeam/";
        const string BOT_PUBLIC_API_CHECK_LIMIT = "/api/public/CheckPublicAPILimit";
        const string BOT_STATS_SYNC = "/api/stats/SyncBotStats";

        public async Task<bool> CheckPublicAPILimit()
        {
            string result = "";
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(Settings.API_URL + BOT_PUBLIC_API_CHECK_LIMIT);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return Convert.ToBoolean(result);
        }
        public async Task<JToken?> GetTeamFromAPI(JToken matchDetails, string questColor, bool questCompleted, CardsCollection playerCards, string user, int league, bool usePrivateApi)
        {

            APIGetTeamPostData data = new APIGetTeamPostData()
            {
                matchDetails = matchDetails,
                questDetails = new JObject() { new JProperty("quest_color", questColor), new JProperty("quest_completed", questCompleted), new JProperty("do_quest", Settings.DO_QUESTS) },
                playerCards = playerCards,
                preferredSummoners = Settings.PREFERRED_SUMMONERS,
                username = user,
                league = league,
                replaceStarterCards = Settings.REPLACE_STARTER_CARDS
            };

            Uri url = new Uri(String.Format(Settings.API_URL + (usePrivateApi ? BOT_PRIVATE_API_GET_TEAM : BOT_PUBLIC_API_GET_TEAM)));

            JObject obj = new JObject()
            {
                new JProperty("json",JsonConvert.SerializeObject(data))
            };

            string json = JsonConvert.SerializeObject(obj);

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await HttpWebRequest.client.PostAsync(url, content);

            string responseString = await response.Content.ReadAsStringAsync();

            if (responseString.Contains("API Limit exceeded"))
            {
                Logs.LogMessage($"{user}: Public api limit Reached", Logs.LOG_ALERT);
                return null;
            }
            else if (responseString.Contains("Timeout, api overloaded"))
            {
                Logs.LogMessage($"{user}: Api timout, contact support", Logs.LOG_WARNING);
                return null;
            }
            else if (responseString.Contains("User not allowed to use Premium features. Please subscribe"))
            {
                Logs.LogMessage($"{user}: User not allowed to use Premium features. Please subscribe", Logs.LOG_WARNING);
                return null;
            }

            dynamic result = JValue.Parse(responseString);

            JToken token = new JObject()
            {
                new JProperty("summoner", result["summoner"]),
                new JProperty("summonerName", result["summonerName"]),
                new JProperty("card1",result["card1"]),
                new JProperty("card1Name",result["card1Name"]),
                new JProperty("card2",result["card2"]),
                new JProperty("card2Name",result["card2Name"]),
                new JProperty("card3",result["card3"]),
                new JProperty("card3Name",result["card3Name"]),
                new JProperty("card4",result["card4"]),
                new JProperty("card4Name",result["card4Name"]),
                new JProperty("card5",result["card5"]),
                new JProperty("card5Name",result["card5Name"]),
                new JProperty("card6",result["card6"]),
                new JProperty("card6Name",result["card6Name"]),
                new JProperty("teamHash",result["teamHash"])
            };

            return token;
        }

        public async Task SyncUserStats(string passKey, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(300000);
                APISyncStatsPostData data = new APISyncStatsPostData()
                {
                    PassKey = passKey,
                    UserStats = InstanceManager.UsersStatistics.ToList()
                };

                Uri url = new Uri(String.Format(Settings.API_URL + BOT_STATS_SYNC));

                JObject obj = new JObject()
                {
                    new JProperty("json",JsonConvert.SerializeObject(data))
                };

                string json = JsonConvert.SerializeObject(obj);

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await HttpWebRequest.client.PostAsync(url, content);

                string responseString = await response.Content.ReadAsStringAsync();

                if (responseString.Contains("Error syncing stats"))
                {
                    Logs.LogMessage("[UserStatsSync]: Error syncing stats", Logs.LOG_ALERT);
                }
                else
                {
                    Logs.LogMessage("[UserStatsSync]: Sync completed.", supress: true);
                }
            }
        }
    }
}
