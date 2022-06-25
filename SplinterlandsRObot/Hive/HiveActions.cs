using System.Text;
using HiveAPI.CS;
using static HiveAPI.CS.CHived;
using SplinterlandsRObot.Net;
using SplinterlandsRObot.Models.Account;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cryptography.ECDSA;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Global;

namespace SplinterlandsRObot.Hive
{
    public class HiveActions
    {
        CHived hive = new CHived(InstanceManager.HttpClient, Settings.HIVE_NODE);
        private object lk = new();
        public UserDetails GetUserDetails(string username, string postingkey)
        {
            HiveActions hive = new();
            Splinterlands sp_api = new();
            var bid = "bid_" + Helpers.RandomString(20);
            var sid = "sid_" + Helpers.RandomString(20);
            var ts = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
            var hash = Sha256Manager.GetHash(Encoding.ASCII.GetBytes(username + ts));
            var sig = Secp256K1Manager.SignCompressedCompact(hash, CBase58.DecodePrivateWif(postingkey));
            var signature = Hex.ToString(sig);
            var response = sp_api.GetUserAccesToken(username, bid, sid, signature, ts).Result;
            if (response.Contains("Incorrect username / password combination"))
            {
                throw new Exception("Invalid username or posting key in users.xml");
            }
            UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(response);
            return userDetails;
        }
        public string StartNewMatch(User user)
        {
            string n = Helpers.RandomString(10);
            string json = "{\"match_type\":\"Ranked\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_find_match", json);

            try
            {
                Logs.LogMessage($"{user.Username}: Finding match...");
                CtransactionData oTransaction = hive.CreateTransaction(new object[] { custom_Json }, new string[] { user.Keys.PostingKey });
                var postData = GetStringForSplinterlandsAPI(oTransaction);
                var response = HttpWebRequest.WebRequestPost(postData, Constants.SPLINTERLANDS_BATTLE_API, Referer: "https://splinterlands.com/");
                if (response == "" || !response.Contains("success"))
                    return "";

                string responseTx = Helpers.DoQuickRegex("id\":\"(.*?)\"", response);
                return responseTx;
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error at finding match: " + ex.ToString(), Logs.LOG_WARNING);
            }
            return "";
        }

        public (string secret, string tx) SubmitTeam(string tx, JToken matchDetails, JToken team, User user, CardsCollection CardsCached)
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

                COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_submit_team", json);

                Logs.LogMessage($"{user.Username}: Submitting team...");
                CtransactionData oTransaction = hive.CreateTransaction(new object[] { custom_Json }, new string[] { user.PassCodes.PostingKey });
                var postData = GetStringForSplinterlandsAPI(oTransaction);
                var response = HttpWebRequest.WebRequestPost(postData, Constants.SPLINTERLANDS_BATTLE_API);
                string responseTx = Helpers.DoQuickRegex("id\":\"(.*?)\"", response);
                return (secret, responseTx);
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{user.Username}: Error at submitting team: " + ex.ToString(), Logs.LOG_WARNING);
            }
            return ("", "");
        }

        public bool RevealTeam(string tx, JToken matchDetails, JToken team, string secret, User user, CardsCollection CardsCached)
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

                COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_team_reveal", json);

                Logs.LogMessage($"{user.Username}: Revealing team...");
                CtransactionData oTransaction = hive.CreateTransaction(new object[] { custom_Json }, new string[] { user.PassCodes.PostingKey });
                var postData = GetStringForSplinterlandsAPI(oTransaction);
                var response = HttpWebRequest.WebRequestPost(postData, Constants.SPLINTERLANDS_BATTLE_API, Referer: "https://splinterlands.com/");
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

        public bool SurrenderBattle(string battleId, User user)
        {
            try
            {
                string n = Helpers.RandomString(10);
                string json = "{\"battle_queue_id\":\"" + battleId + "\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";
                Logs.LogMessage($"{user.Username}: Surrendering battle...");
                COperations.custom_json custom_json = CreateCustomJson(user, false, true, "sm_surrender", json);
                CtransactionData oTransaction = hive.CreateTransaction(new object[] { custom_json }, new string[] { user.PassCodes.PostingKey });
                var postData = GetStringForSplinterlandsAPI(oTransaction);
                var response = HttpWebRequest.WebRequestPost(postData, Constants.SPLINTERLANDS_BATTLE_API, Referer: "https://splinterlands.com/");
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

        public COperations.custom_json CreateCustomJson(User user, bool activeKey, bool postingKey, string methodName, string json)
        {
            COperations.custom_json customJsonOperation = new COperations.custom_json
            {
                required_auths = activeKey ? new string[] { user.Username } : new string[0],
                required_posting_auths = postingKey ? new string[] { user.Username } : new string[0],
                id = methodName,
                json = json
            };
            return customJsonOperation;
        }

        public async Task ClaimSeasonRewards()
        {
            Splinterlands sp_api = new();
            try
            {
                foreach (User user in InstanceManager.userList)
                {
                    Logs.LogMessage($"[Season Rewards] {user.Username}: Checking for season rewards... ", supress: true);
                    var bid = "bid_" + Helpers.RandomString(20);
                    var sid = "sid_" + Helpers.RandomString(20);
                    var ts = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
                    var hash = Sha256Manager.GetHash(Encoding.ASCII.GetBytes(user.Username + ts));
                    var sig = Secp256K1Manager.SignCompressedCompact(hash, CBase58.DecodePrivateWif(user.PassCodes.PostingKey));
                    var signature = Hex.ToString(sig);
                    var response = await sp_api.GetSeasonDetails(user.Username, bid, sid, signature, ts);

                    var seasonReward = Helpers.DoQuickRegex("\"season_reward\":(.*?)},\"", response);
                    if (seasonReward == "{\"reward_packs\":0")
                    {
                        Logs.LogMessage($"{user.Username}: No season reward available!", Logs.LOG_ALERT, true);
                    }
                    else
                    {
                        var season = Helpers.DoQuickRegex("\"season\":(.*?),\"", seasonReward);
                        if (season.Length <= 1)
                        {
                            Logs.LogMessage($"{user.Username}: Error at claiming season rewards: Could not read season!", Logs.LOG_WARNING);
                        }
                        else
                        {
                            Logs.LogMessage($"{user.Username}: Season rewards available.", Logs.LOG_ALERT);
                            string n = Helpers.RandomString(10);
                            string json = "{\"type\":\"league_season\",\"season\":\"" + season + "\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

                            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_claim_reward", json);

                            CtransactionData oTransaction = hive.CreateTransaction(new object[] { custom_Json }, new string[] { user.PassCodes.PostingKey });
                            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.PassCodes.PostingKey });
                            
                            await Task.Delay(15000);
                            var rewardsRaw = await sp_api.GetTransactionDetails(tx);
                            if (rewardsRaw.Contains(" not found"))
                            {
                                continue;
                            }
                            else if (rewardsRaw.Contains("has already claimed their rewards from the specified season"))
                            {
                                Logs.LogMessage($"[Season Rewards] {user.Username}: Rewards already claimed!", Logs.LOG_ALERT);
                            }
                            var rewards = JToken.Parse(rewardsRaw)["trx_info"]["result"];


                            if (!((string)rewards).Contains("success\":true"))
                            {
                                Logs.LogMessage($"[Season Rewards] {user.Username}: Error at claiming season rewards: " + (string)rewards, Logs.LOG_WARNING);

                            }
                            else if (((string)rewards).Contains("success\":true"))
                            {
                                Logs.LogMessage($"[Season Rewards] {user.Username}: Successfully claimed season rewards!", Logs.LOG_SUCCESS);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"[Season Rewards] Error at claiming season reward: {ex.Message}", Logs.LOG_WARNING);
            }
        }

        private string GetStringForSplinterlandsAPI(CtransactionData oTransaction)
        {
            string json = JsonConvert.SerializeObject(oTransaction.tx);
            string postData = "signed_tx=" + json.Replace("operations\":[{", "operations\":[[\"custom_json\",{")
                .Replace(",\"opid\":18}", "}]");
            return postData;
        }

        internal string AdvanceLeague(User user)
        {
            string n = Helpers.RandomString(10);
            string json = "{\"notify\":\"false\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_advance_league", json);

            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.PassCodes.PostingKey });
            return tx;
        }

        internal string ClaimQuest(User user, string questId)
        {
            string n = Helpers.RandomString(10);
            string json = "{\"type\":\"quest\",\"quest_id\":\"" + questId + "\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_claim_reward", json);

            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.Keys.PostingKey });
            return tx;
        }

        internal bool NewQuest(User user)
        {
            string n = Helpers.RandomString(10);
            string json = "{\"type\":\"daily\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_refresh_quest", json);

            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.PassCodes.PostingKey });
            if (tx != null)
                return true;

            return false;
        }

        internal bool StartFocus(User user)
        {
            string n = Helpers.RandomString(10);
            string json = "{\"type\":\"daily\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_start_quest", json);

            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.Keys.PostingKey });
            if (tx != null)
                return true;

            return false;
        }
    }
}
