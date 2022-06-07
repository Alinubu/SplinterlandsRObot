using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Models;
using SplinterlandsRObot.Net;

namespace SplinterlandsRObot.API
{
    public class Splinterlands
    {
        const string API_URL = "https://api2.splinterlands.com";
        const string SP_USER_DATA = "/players/details?name=";
        const string SP_QUEST_DATA = "/players/quests?username=";
        const string SP_CARDS_COLLECTION = "/cards/collection/@@_username_@@?v=@@_timestamp_@@&token=@@_accessToken_@@&username=@@_username_@@";
        const string SP_OUTSTANGING_MATCH = "/players/outstanding_match?username=";
        const string SP_AIRDROP_DATA = "/players/sps?v=1638714545572&token=@@_accessToken_@@&username=@@_username_@@";
        const string SP_PlAYER_BALANCE = "/players/balances?username=";
        const string SP_CLAIM_SEASON_REWARDS = "/players/login?name=@@_username_@@&ref=&browser_id=@@_browserid_@@&session_id=@@_sessionid_@@&sig=@@_signature_@@&ts=@@_timestamp_@@";
        const string SP_TRANSACTION_DETAILS = "/transactions/lookup?trx_id=";
        const string SP_MATCH_DETAILS = "/battle/status?id=";
        const string SP_MATCH_ENEMY_PICK = "/players/outstanding_match?username=";
        const string SP_MATCH_RESULTS = "/battle/history2?player=";
        const string SP_ACCESS_TOKEN = "/players/login?name=@@_username_@@&ref=&browser_id=@@_bid_@@&session_id=@@_sid_@@&sig=@@_signature_@@&ts=@@_timestamp_@@";
        const string SP_SPLINTERLANDS_CARDS = "/cards/get_details";
        const string SP_SPLINTERLANDS_SETTINGS = "/settings";

        public async Task<List<SplinterlandsCard>> GetSplinterlandsCards()
        {
            string result = "";
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_SPLINTERLANDS_CARDS);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<List<SplinterlandsCard>>(result); ;
        }
        public async Task<SplinterlandsSettings> GetSplinterlandsSettings()
        {
            string result = "";
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_SPLINTERLANDS_SETTINGS);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<SplinterlandsSettings>(result);
        }

        public async Task<UserDetails> GetUserDetails(string username)
        {
            string result = "";
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_USER_DATA + username);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            await Task.Delay(500);
            return JsonConvert.DeserializeObject<UserDetails>(result);
        }
        public async Task<string> GetUserAccesToken(string username, string bid, string sid, string signature, string ts)
        {
            string result = "";
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_ACCESS_TOKEN.Replace("@@_username_@@", username).Replace("@@_bid_@@", bid).Replace("@@_sid_@@", sid).Replace("@@_signature_@@", signature).Replace("@@_timestamp_@@",ts));
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }
        public async Task<QuestData> GetQuestData(string username)
        {
            string result = "";
            QuestData data = new QuestData();

            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_QUEST_DATA + username);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }

            if (!result.Contains("id"))
                return null;

            dynamic json = JValue.Parse(result);

            data.chest_tier = json[0]["chest_tier"];
            data.claim_date = json[0]["claim_date"];
            data.claim_trx_id = json[0]["claim_trx_id"];
            data.completed_items = json[0]["completed_items"];
            data.created_block = json[0]["created_block"];
            data.created_date = json[0]["created_date"];
            data.id = json[0]["id"];
            data.league = json[0]["league"];
            data.name = json[0]["name"];
            data.player = json[0]["player"];
            data.refresh_trx_id = json[0]["refresh_trx_id"];
            data.reward_qty = json[0]["reward_qty"];
            data.rewards = null;
            data.rshares = json[0]["rshares"];
            data.total_items = json[0]["total_items"];
            
            return data;
        }
        public async Task<CardsCollection> GetUserCardsCollection(string username, string accessToken)
        {
            string result = "";
            string ts = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_CARDS_COLLECTION.Replace("@@_username_@@", username).Replace("@@_timestamp_@@", ts).Replace("@@_accessToken_@@", accessToken));
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            await Task.Delay(500);

            return JsonConvert.DeserializeObject<CardsCollection>(result);
        }
        public async Task<string> GetOutstandingMatch(string username)
        {
            string result = "";

            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_OUTSTANGING_MATCH + username);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
        public async Task<string> GetTransactionDetails(string tx)
        {
            string result = "";

            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_TRANSACTION_DETAILS + tx);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
        public async Task<string> GetSeasonDetails(string username, string bid, string sid, string sig, string ts)
        {
            string result = "";
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_CLAIM_SEASON_REWARDS.Replace("@@_username_@@", username).Replace("@@_browserid_@@", bid).Replace("@@_sessionid_@@", sid).Replace("@@_signature_@@", sig).Replace("@@_timestamp_@@", ts));
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }
        public async Task<UserBalance> GetPlayerBalancesAsync(string username)
        {
            string result = "";
            JToken defaultValue = new JObject(
                new JProperty("balance", 0));
            UserBalance balance = new();
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_PlAYER_BALANCE + username);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            JArray balances = (JArray)JToken.Parse(result);

            var ECRBalance = balances.Where(x => (string)x["token"] == "ECR").FirstOrDefault(defaultValue);
            if ((int)ECRBalance["balance"] == 0)
            { balance.ECR = 100; }
            else
            {
                var captureRate = (int)ECRBalance["balance"];
                DateTime lastRewardTime = (DateTime)ECRBalance["last_reward_time"];
                double ecrRegen = 0.0868;
                double ecr = captureRate + (new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() - new DateTimeOffset(lastRewardTime).ToUnixTimeMilliseconds()) / 3000 * ecrRegen;
                balance.ECR = Math.Min(ecr, 10000) / 100;
            }

            var gPotionBalance = balances.Where(x => (string)x["token"] == "GOLD").FirstOrDefault(defaultValue);
            balance.GoldPotions = (int)gPotionBalance["balance"];

            var lPotionBalance = balances.Where(x => (string)x["token"] == "LEGENDARY").FirstOrDefault(defaultValue);
            balance.LegendaryPotions = (int)lPotionBalance["balance"];

            var qPotionBalance = balances.Where(x => (string)x["token"] == "QUEST").FirstOrDefault(defaultValue);
            balance.QuestPotions = (int)qPotionBalance["balance"];

            var packsBalance = balances.Where(x => (string)x["token"] == "CHAOS").FirstOrDefault(defaultValue);
            balance.Packs = (int)packsBalance["balance"];

            var voucherBalance = balances.Where(x => (string)x["token"] == "VOUCHER").FirstOrDefault(defaultValue);
            balance.Voucher = (double)voucherBalance["balance"];

            var decBalance = balances.Where(x => (string)x["token"] == "DEC").FirstOrDefault(defaultValue);
            balance.DEC = (double)decBalance["balance"];

            var creditsBalance = balances.Where(x => (string)x["token"] == "CREDITS").FirstOrDefault(defaultValue);
            balance.Credits = (double)creditsBalance["balance"];

            var stakedSPSBalance = balances.Where(x => (string)x["token"] == "SPSP").FirstOrDefault(defaultValue);
            balance.SPSP = (double)stakedSPSBalance["balance"];

            var SPSBalance = balances.Where(x => (string)x["token"] == "SPS").FirstOrDefault(defaultValue);
            balance.SPS = (double)SPSBalance["balance"];
            await Task.Delay(500);

            return balance;
        }
        #region Windows7 Legacy Mode
        public async Task<JToken> GetMatchDetails(string tx)
        {
            string result = "";
            HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_MATCH_DETAILS + tx);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return JToken.Parse(result);
        }

        public async Task<(bool enemyHasPicked, bool surrender)> CheckEnemyHasPickedAsync(string username, string tx)
        {
            try
            {
                string result = "";
                HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_MATCH_ENEMY_PICK + username);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }

                // Check for surrender
                if (result == "null")
                {
                    return (true, true);
                }
                var matchInfo = JToken.Parse(result);

                return matchInfo["opponent_team_hash"].Type != JTokenType.Null ? (true, false) : (false, false);
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{username}: Could not get ongoing game from splinterlands API: {ex.Message}", Logs.LOG_WARNING);
            }
            return (true, true);
        }

        public async Task<(int newRating, int ratingChange, decimal decReward, int result)> GetBattleResultAsync(string username, string tx)
        {
            try
            {
                string result = "";
                HttpResponseMessage response = await HttpWebRequest.client.GetAsync(API_URL + SP_MATCH_RESULTS + username);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }

                var matchHistory = JToken.Parse(result);

                // Battle not yet finished (= not yet shown in history)?
                if ((string)matchHistory["battles"][0]["battle_queue_id_1"] != tx && (string)matchHistory["battles"][0]["battle_queue_id_2"] != tx)
                {
                    return (-1, -1, -1, -1);
                }

                int gameResult = 0;
                if ((string)matchHistory["battles"][0]["winner"] == username)
                {
                    gameResult = 1;
                }
                else if ((string)matchHistory["battles"][0]["winner"] == "DRAW")
                {
                    gameResult = 2;
                }

                int newRating = (string)matchHistory["battles"][0]["player_1"] == username ? ((int)matchHistory["battles"][0]["player_1_rating_final"]) :
                    ((int)matchHistory["battles"][0]["player_2_rating_final"]);
                int ratingChange = (string)matchHistory["battles"][0]["player_1"] == username ? newRating - ((int)matchHistory["battles"][0]["player_1_rating_initial"]) :
                    newRating - ((int)matchHistory["battles"][0]["player_2_rating_initial"]);
                decimal decReward = (decimal)matchHistory["battles"][0]["reward_dec"];

                return (newRating, ratingChange, decReward, gameResult);
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{username}: Could not get battle results from splinterlands API: {ex.Message}", Logs.LOG_WARNING);
            }

            return (-1, -1, -1, -1);
        }
        #endregion
    }
}
