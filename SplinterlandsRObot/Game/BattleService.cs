using HiveAPI.CS;
using SplinterlandsRObot.Global;
using SplinterlandsRObot.Hive;
using SplinterlandsRObot.Player;
using SplinterlandsRObot.Net;
using static HiveAPI.CS.CHived;
using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Cards;

namespace SplinterlandsRObot.Game
{
    public class BattleService
    {
        HiveService hive;
        WebClient client;
        private const string BATTLE = "battle/battle_tx";
        private const string ORIGIN = "https://splinterlands.com";
        private const string REFERER = "https://splinterlands.com";

        public BattleService()
        {
            hive = new HiveService();
            client = new WebClient(Constants.SPLINTERLANDS_BATTLE_API, ORIGIN, REFERER,Settings.PROXY_URL,Settings.PROXY_PORT, Settings.PROXY_USERNAME, Settings.PROXY_PASSWORD);
        }

        public async Task<string> StartBattle(User user, string battleMode)
        {
            string matchType = battleMode == "modern" ? "Modern Ranked" : "Wild Ranked";
            string n = Helpers.RandomString(10);
            string json = "{\"match_type\":\"" + matchType + "\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = hive.CreateCustomJson(user, false, true, "sm_find_match", json);

            try
            {
                Logs.LogMessage($"{user.Username}: Finding match...");
                CtransactionData oTransaction = hive.CreateTransaction(custom_Json, user.Keys.PostingKey);
                string postData = hive.ParseTransactionData(oTransaction);
                string response = await client.PostAsync(postData, BATTLE);
                Logs.LogMessage($"{user.Username}: {response}", Logs.LOG_ALERT, supress: true);
                if (response == "")
                    return "";
                if (!response.Contains("success"))
                    return "error";

                string responseTx = Helpers.DoQuickRegex("id\":\"(.*?)\"", response);
                return responseTx;
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error at finding match: " + ex.ToString(), Logs.LOG_WARNING);
            }
            return "";
        }

        public async Task<(string secret, string tx)> SubmitTeam(string tx, JToken matchDetails, JToken team, User user, CardsCollection CardsCached)
        {
            try
            {
                string summoner = team["summoner"].ToString();
                string monsters = "";
                for (int i = 1; i <= 6; i++)
                {
                    string monster = team[$"card{i}"].ToString();

                    if (monster != "")
                    {
                        if (monster.Length == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    monsters += "\"" + monster + "\",";
                }
                monsters = monsters[..^1];

                string secret = Helpers.RandomString(10);
                string n = Helpers.RandomString(10);

                string monsterClean = monsters.Replace("\"", "");

                string teamHash = Helpers.GenerateMD5Hash(summoner + "," + monsterClean + "," + secret);

                string json = "{\"trx_id\":\"" + tx + "\",\"team_hash\":\"" + teamHash + "\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

                COperations.custom_json custom_Json = hive.CreateCustomJson(user, false, true, "sm_submit_team", json);

                Logs.LogMessage($"{user.Username}: Submitting team...");
                CtransactionData oTransaction = hive.CreateTransaction(custom_Json, user.Keys.PostingKey);
                string postData = hive.ParseTransactionData(oTransaction);
                string response = await client.PostAsync(postData, BATTLE);
                string responseTx = Helpers.DoQuickRegex("id\":\"(.*?)\"", response);
                return (secret, responseTx);
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error at submitting team: " + ex.ToString(), Logs.LOG_WARNING);
            }
            return ("", "");
        }

        public async Task<bool> RevealTeam(string tx, JToken matchDetails, JToken team, string secret, User user, CardsCollection CardsCached)
        {
            try
            {
                string summoner = team["summoner"].ToString();
                string monsters = "";
                for (int i = 1; i <= 6; i++)
                {
                    string monster = team[$"card{i}"].ToString();

                    if (monster != "")
                    {
                        if (monster.Length == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    monsters += "\"" + monster + "\",";
                }
                monsters = monsters[..^1];

                string n = Helpers.RandomString(10);

                string monsterClean = monsters.Replace("\"", "");


                string json = "{\"trx_id\":\"" + tx + "\",\"summoner\":\"" + summoner + "\",\"monsters\":[" + monsters + "],\"secret\":\"" + secret + "\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

                COperations.custom_json custom_Json = hive.CreateCustomJson(user, false, true, "sm_team_reveal", json);

                Logs.LogMessage($"{user.Username}: Revealing team...");
                CtransactionData oTransaction = hive.CreateTransaction(custom_Json, user.Keys.PostingKey);
                string postData = hive.ParseTransactionData(oTransaction);
                string response = await client.PostAsync(postData, BATTLE);
                string responseTx = Helpers.DoQuickRegex("id\":\"(.*?)\"", response);

                if (responseTx == "")
                    return false;
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error at revealing team: " + ex.ToString(), Logs.LOG_WARNING);
                return false;
            }
            return true;
        }

        public async Task<bool> SurrenderBattle(string battleId, User user)
        {
            try
            {
                string n = Helpers.RandomString(10);
                string json = "{\"battle_queue_id\":\"" + battleId + "\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";
                Logs.LogMessage($"{user.Username}: Surrendering battle...");
                COperations.custom_json custom_json = hive.CreateCustomJson(user, false, true, "sm_surrender", json);
                CtransactionData oTransaction = hive.CreateTransaction(custom_json, user.Keys.PostingKey);
                string postData = hive.ParseTransactionData(oTransaction);
                string response = await client.PostAsync(postData, BATTLE);
                string responseTx = Helpers.DoQuickRegex("id\":\"(.*?)\"", response);

                if (responseTx == "")
                    return false;
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error surrendering battle: " + ex.ToString(), Logs.LOG_WARNING);
                return false;
            }

            return true;
        }
    }
}
