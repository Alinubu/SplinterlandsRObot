using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Cards;
using SplinterlandsRObot.Models;
using SplinterlandsRObot.Player;
using SplinterlandsRObot.Models.Splinterlands;
using SplinterlandsRObot.Net;

namespace SplinterlandsRObot.API
{
    public class Splinterlands
    {
        const string API_URL = "https://api2.splinterlands.com";
        const string ORIGIN = "https://splinterlands.com";
        const string REFERER = "https://splinterlands.com";
        const string SP_USER_DATA = "players/details?name=";
        const string SP_CARDS_COLLECTION = "cards/collection/@@_username_@@?v=@@_timestamp_@@&token=@@_accessToken_@@&username=@@_username_@@";
        const string SP_OUTSTANGING_MATCH = "players/outstanding_match?username=@@_username_@@&token=@@_accessToken_@@";
        const string SP_PlAYER_BALANCE = "players/balances?username=";
        const string SP_TRANSACTION_DETAILS = "transactions/lookup?trx_id=";
        const string SP_SPLINTERLANDS_CARDS = "cards/get_details";
        const string SP_SPLINTERLANDS_SETTINGS = "settings";
        const string SP_LOGIN = "players/v2/login?name=@@_username_@@&ref=&browser_id=@@_bid_@@&session_id=@@_sid_@@&sig=@@_signature_@@&ts=@@_timestamp_@@";
        const string SP_UPDATE = "players/v2/update?name=@@_username_@@&ref=&browser_id=@@_bid_@@&session_id=@@_sid_@@&ts=@@_timestamp_@@&keychain=false&sig=@@_signature_@@&v=@@_timestamp_@@";
        WebClient client;

        public Splinterlands()
        {
            client = new WebClient(API_URL, ORIGIN, REFERER, Settings.PROXY_URL, Settings.PROXY_PORT, Settings.PROXY_USERNAME, Settings.PROXY_PASSWORD);
        }

        public async Task<List<SplinterlandsCard>> GetSplinterlandsCards()
        {
            string result = await client.GetAsync(SP_SPLINTERLANDS_CARDS);
            return JsonConvert.DeserializeObject<List<SplinterlandsCard>>(result); ;
        }
        public async Task<SplinterlandsSettings> GetSplinterlandsSettings()
        {
            string result = await client.GetAsync(SP_SPLINTERLANDS_SETTINGS);
            return JsonConvert.DeserializeObject<SplinterlandsSettings>(result);
        }

        public async Task<UserDetails> GetUserDetails(string username)
        {
            string result = await client.GetAsync(SP_USER_DATA + username);
            await Task.Delay(500);
            return JsonConvert.DeserializeObject<UserDetails>(result);
        }
        public async Task<UserDetails> LoginAccount(string username, string bid, string sid, string signature, string ts)
        {
            string result = await client.GetAsync(SP_LOGIN.Replace("@@_username_@@", username).Replace("@@_bid_@@", bid).Replace("@@_sid_@@", sid).Replace("@@_signature_@@", signature).Replace("@@_timestamp_@@", ts));
            if (result.Contains("Incorrect username / password combination"))
            {
                throw new Exception("Invalid username or posting key in users.xml");
            }
            return JsonConvert.DeserializeObject<UserDetails> (result);
        }

        public async Task<UserDetails> UpdateAccount(string username, string bid, string sid, string signature, string ts, string jwt_token)
        {
            string result = await client.GetAsync(SP_UPDATE.Replace("@@_username_@@", username).Replace("@@_bid_@@", bid).Replace("@@_sid_@@", sid).Replace("@@_signature_@@", signature).Replace("@@_timestamp_@@", ts), new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt_token));
            if (result.Contains("Incorrect username / password combination"))
            {
                throw new Exception("Invalid username or posting key in users.xml");
            }
            return JsonConvert.DeserializeObject<UserDetails> (result);
        }

        public async Task<CardsCollection> GetUserCardsCollection(string username, string accessToken)
        {
            string ts = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
            string result = await client.GetAsync(SP_CARDS_COLLECTION.Replace("@@_username_@@", username).Replace("@@_timestamp_@@", ts).Replace("@@_accessToken_@@", accessToken));
            
            await Task.Delay(500);

            return JsonConvert.DeserializeObject<CardsCollection>(result);
        }
        public async Task<string> GetOutstandingMatch(string username, string token)
        {
            return await client.GetAsync(SP_OUTSTANGING_MATCH.Replace("@@_username_@@", username).Replace("@@_accessToken_@@", token));
        }
        public async Task<string> GetTransactionDetails(string tx)
        {
            try
            {
                return await client.GetAsync(SP_TRANSACTION_DETAILS + tx);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<Balances> GetPlayerBalancesAsync(string username)
        {
            JToken defaultValue = new JObject(
                new JProperty("balance", 0));
            Balances balance = new();
            string result = await client.GetAsync(SP_PlAYER_BALANCE + username);

            JArray balances = (JArray)JToken.Parse(result);

            var ECRBalance = balances.Where(x => (string)x["token"] == "ECR").FirstOrDefault(defaultValue);
            var captureRate = Convert.ToDouble(ECRBalance["balance"].ToString());
            if (captureRate == 0)
            { balance.ECR = 50; }
            else
            {
                
                DateTime lastRewardTime = (DateTime)ECRBalance["last_reward_time"];
                double msInOneHour = 1000 * 60 * 60;
                double hourlyRechargeRate = 1;
                double ecr = Math.Floor(captureRate + (new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() - new DateTimeOffset(lastRewardTime).ToUnixTimeMilliseconds()) / msInOneHour * hourlyRechargeRate);
                balance.ECR = Math.Min(ecr, 50);
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
    }
}
