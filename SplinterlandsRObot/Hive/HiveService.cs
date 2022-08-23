using System.Text;
using HiveAPI.CS;
using static HiveAPI.CS.CHived;
using SplinterlandsRObot.Net;
using SplinterlandsRObot.Player;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cryptography.ECDSA;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Global;
using SplinterlandsRObot.Cards;
using SplinterlandsRObot.Extensions;

namespace SplinterlandsRObot.Hive
{
    public class HiveService
    {
        CHived hive = new CHived(InstanceManager.HttpClient, Settings.HIVE_NODE);
        private object lk = new();

        public string SignTransaction(string username, string postingKey, string ts)
        {
            var hash = Sha256Manager.GetHash(Encoding.ASCII.GetBytes(username + ts));
            var sig = Secp256K1Manager.SignCompressedCompact(hash, CBase58.DecodePrivateWif(postingKey));
            var signature = Hex.ToString(sig);
            return signature;
        }

        public CtransactionData CreateTransaction(object transactionData, string postingKey)
        {
            return hive.CreateTransaction(new object[] { transactionData }, new string[] { postingKey });
        }

        public string ParseTransactionData(CtransactionData oTransaction)
        {
            string json = JsonConvert.SerializeObject(oTransaction.tx);
            string data = "signed_tx=" + json.Replace("operations\":[{", "operations\":[[\"custom_json\",{")
                .Replace(",\"opid\":18}", "}]");
            return data;
        }

        public async Task<UserDetails> GetUserDetails(string username, string postingkey)
        {
            Splinterlands sp_api = new();
            var bid = "bid_" + Helpers.RandomString(20);
            var sid = "sid_" + Helpers.RandomString(20);
            var ts = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
            var hash = Sha256Manager.GetHash(Encoding.ASCII.GetBytes(username + ts));
            var sig = Secp256K1Manager.SignCompressedCompact(hash, CBase58.DecodePrivateWif(postingkey));
            var signature = Hex.ToString(sig);
            UserDetails userDetails = await sp_api.LoginAccount(username, bid, sid, signature, ts);
            if (userDetails == null)
            {
                throw new Exception("Error loading user details. No response received");
            }
            Thread.Sleep(655);
            return userDetails;
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

        public async Task ClaimSeasonRewards(User? _user = null, int? _season = null, Config? _config = null)
        {
            try
            {   
                if (_user == null)
                {
                    foreach (User user in InstanceManager.userList)
                    {
                        Logs.LogMessage($"[Season Rewards] {user.Username}: Checking for season rewards... ", supress: true);
                        var bid = "bid_" + Helpers.RandomString(20);
                        var sid = "sid_" + Helpers.RandomString(20);
                        var ts = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
                        var hash = Sha256Manager.GetHash(Encoding.ASCII.GetBytes(user.Username + ts));
                        var sig = Secp256K1Manager.SignCompressedCompact(hash, CBase58.DecodePrivateWif(user.Keys.PostingKey));
                        var signature = Hex.ToString(sig);
                        UserDetails response = await GetUserDetails(user.Username, user.Keys.PostingKey);

                        if (response.season_reward is null)
                        {
                            Logs.LogMessage($"{user.Username}: Error at claiming season rewards: Could not read season!", Logs.LOG_WARNING);
                            continue;
                        }

                        if (response.season_reward.reward_packs == 0)
                            Logs.LogMessage($"{user.Username}: No season reward available!", Logs.LOG_ALERT);
                        else
                        {
                            if (response.season_reward.season is not null)
                            {
                                if (await SendSeasonClaimTransaction(user, (int)response.season_reward.season))
                                {
                                    Config config = InstanceManager.BotInstances.Where(x => x.UserData.Username == user.Username).First().UserConfig;

                                    if (config is not null)
                                    {
                                        if (config.AutoTransferAfterSeasonClaim)
                                        {
                                            await new TransferAssets().TransferAssetsAsync(user);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                else
                {
                    if (await SendSeasonClaimTransaction(_user, (int)_season))
                    {
                        if (_config is not null)
                        {
                            if (_config.AutoTransferAfterSeasonClaim)
                            {
                                await new TransferAssets().TransferAssetsAsync(_user);
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

        public async Task<bool> SendSeasonClaimTransaction(User user, int season)
        {
            Splinterlands sp_api = new();
            Logs.LogMessage($"{user.Username}: Season rewards available.", Logs.LOG_ALERT);
            string n = Helpers.RandomString(10);
            string json = "{\"type\":\"league_season\",\"season\":\"" + season + "\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_claim_reward", json);

            CtransactionData oTransaction = hive.CreateTransaction(new object[] { custom_Json }, new string[] { user.Keys.PostingKey });
            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.Keys.PostingKey });

            await Task.Delay(15000);
            var rewardsRaw = await sp_api.GetTransactionDetails(tx);
            if (rewardsRaw.Contains(" not found"))
            {
                return false;
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
                return true;
            }
            return false;
        }

        private string GetStringForSplinterlandsAPI(CtransactionData oTransaction)
        {
            string json = JsonConvert.SerializeObject(oTransaction.tx);
            string postData = "signed_tx=" + json.Replace("operations\":[{", "operations\":[[\"custom_json\",{")
                .Replace(",\"opid\":18}", "}]");
            return postData;
        }

        internal string AdvanceLeague(User user, string format)
        {
            string n = Helpers.RandomString(10);
            string leagueFormat = format == "modern" ? ",\"format\":\"modern\"" : string.Empty;
            string json = "{\"notify\":\"false\"" + leagueFormat + ",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_advance_league", json);

            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.Keys.PostingKey });
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

        internal string NewQuest(User user)
        {
            string n = Helpers.RandomString(10);
            string json = "{\"type\":\"daily\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_refresh_quest", json);

            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.Keys.PostingKey });
            if (tx != null)
                return tx;

            return null;
        }

        internal string StartFocus(User user)
        {
            string n = Helpers.RandomString(10);
            string json = "{\"type\":\"daily\",\"app\":\"" + Constants.APP_VERSION + "\",\"n\":\"" + n + "\"}";

            COperations.custom_json custom_Json = CreateCustomJson(user, false, true, "sm_start_quest", json);

            string tx = hive.broadcast_transaction(new object[] { custom_Json }, new string[] { user.Keys.PostingKey });
            if (tx != null)
                return tx;

            return null;
        }
    }
}
