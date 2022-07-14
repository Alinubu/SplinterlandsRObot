using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Models.WebSocket;
using Websocket.Client;
using Websocket.Client.Models;
using SplinterlandsRObot.Global;
using SplinterlandsRObot.Game;
using Newtonsoft.Json;
using SplinterlandsRObot.Models.Account;

namespace SplinterlandsRObot.Classes.Net
{
    public class WebSocket
    {
        public WebsocketClient client;
        private BotInstance instance { get; set; }
        private string username { get; set; }
        private string accessToken { get; set; }
        public List<WebSocketTransactionMessage> transactions { get; set; }

        public WebSocket(string _username, string _accessToken, BotInstance _instance)
        {
            instance = _instance;
            username = _username;
            accessToken = _accessToken;
            transactions = new List<WebSocketTransactionMessage>();
            client = new WebsocketClient(new Uri(Constants.SPLINTERLANDS_WEBSOCKET_URL));
            client.ReconnectTimeout = null;
            client.MessageReceived.Subscribe(OnMessageReceived);
            client.ReconnectionHappened.Subscribe(OnReconnectionHappened);
            client.DisconnectionHappened.Subscribe(OnDisconnectionHappened);
        }

        public async Task WebsocketStart()
        {
            await client.Start();
            WebsocketAuthenticate();
            _ = WebsocketPing().ConfigureAwait(false);
        }
        private void WebsocketAuthenticate()
        {
            string sessionID = Helpers.RandomString(10);
            string message = "{\"type\":\"auth\",\"player\":\"" + username + "\",\"access_token\":\"" + accessToken + "\",\"session_id\":\"" + sessionID + "\"}";
            WebsocketSendMessage(message);
        }
        public void WebsocketSendMessage(string message)
        {
            client.Send(message);
        }
        public async Task WebsocketPing()
        {
            while (client.IsStarted)
            {
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Delay(20 * 1000);
                        if (!client.IsStarted)
                        {
                            return;
                        }
                    }

                    client.Send("{\"type\":\"ping\"}");
                    Logs.LogMessage($"{username}: ping", supress: true);
                }
                catch (Exception ex)
                {
                    Logs.LogMessage($"{username}: Error pinging WebSocket { ex }", supress: true);
                    await WebsocketStop("Ping Error");
                }
            }
        }
        public async Task WebsocketStop(string stopDescription)
        {
            await client.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, stopDescription);
        }
        public void WebsocketDispose()
        {
            client.Dispose();
        }
        private void OnMessageReceived(ResponseMessage message)
        {
            if (message.MessageType != System.Net.WebSockets.WebSocketMessageType.Text
                || !message.Text.Contains("\"id\""))
            {
                return;
            }
            JToken json = JToken.Parse(message.Text);
            

            string messageType = json["id"].ToString();

            if (messageType == "transaction_complete")
            {
                transactions.Add(new WebSocketTransactionMessage()
                {
                    message = json,
                    processed = false
                });
            }
            else if (messageType == "match_found")
            {
                instance.UpdateMatchFound(true, json["data"]);
            }
            else if (messageType == "opponent_submit_team")
            {
                instance.UpdateOpponentSubmitTeam(true);
            }
            else if (messageType == "rating_update")
            {
                if (json["data"].ToString().Contains("modern"))
                {
                    if (json["data"]["modern"].ToString().Contains("new_rating"))
                    {
                        instance.UpdateModernRating((int)json["data"]["modern"]["new_rating"]);
                    }
                    if (json["data"]["modern"].ToString().Contains("new_league"))
                    {
                        instance.UpdateModernLeague((int)json["data"]["modern"]["new_league"]);
                    }
                    if (json["data"]["modern"].ToString().Contains("new_max_league"))
                    {
                        instance.UpdateModernMaxLeague((int)json["data"]["modern"]["new_max_league"]);
                    }
                    //if (json["data"]["modern"].ToString().Contains("additional_season_rshares"))
                    //{
                    //    instance.UpdateModernSeasonRewardShares((int)json["data"]["modern"]["additional_season_rshares"]);
                    //}
                }
                else if (json["data"].ToString().Contains("wild"))
                {
                    if (json["data"]["wild"].ToString().Contains("new_rating"))
                    {
                        instance.UpdateRating((int)json["data"]["wild"]["new_rating"]);
                    }
                    if (json["data"]["wild"].ToString().Contains("new_league"))
                    {
                        instance.UpdateLeague((int)json["data"]["wild"]["new_league"]);
                    }
                    if (json["data"]["wild"].ToString().Contains("new_max_league"))
                    {
                        instance.UpdateMaxLeague((int)json["data"]["wild"]["new_max_league"]);
                    }
                    //if (json["data"]["wild"].ToString().Contains("additional_season_rshares"))
                    //{
                    //    instance.UpdateSeasonRewardShares((int)json["data"]["wild"]["additional_season_rshares"]);
                    //}
                }
                
                if (json["data"].ToString().Contains("new_collection_power"))
                {
                    instance.UpdateCollectionPower((int)json["data"]["new_collection_power"]);
                }
            }
            else if (messageType == "ecr_update")
            {
                if (json["data"].ToString().Contains("capture_rate"))
                {

                    instance.UpdateECR(
                        new Balance()
                        {
                            balance = (double)json["data"]["capture_rate"],
                            token = "ECR",
                            last_reward_block = (int)json["data"]["last_reward_block"],
                            last_reward_time = (DateTime)json["data"]["last_reward_time"]
                        });
                }
            }
            else if (messageType == "balance_update")
            {
                if (json["data"]["token"].ToString() == "DEC")
                {
                    if (json["data"]["type"].ToString() == "dec_reward")
                        instance.UpdateLastReward((double)json["data"]["amount"]);
                    instance.UpdateDecBalance((double)json["data"]["balance_end"]);
                }
                else if (json["data"]["token"].ToString() == "SPS")
                {
                    instance.UpdateSpsBalance((double)json["data"]["balance_end"]);
                }
                else if (json["data"]["token"].ToString() == "GOLD")
                {
                    instance.UpdateGoldPotionsBalance((double)json["data"]["balance_end"]);
                }
                else if (json["data"]["token"].ToString() == "LEGENDARY")
                {
                    instance.UpdateLegendaryPotionsBalance((double)json["data"]["balance_end"]);
                }
                else if (json["data"]["token"].ToString() == "CHAOS")
                {
                    instance.UpdatePacksBalance((double)json["data"]["balance_end"]);
                }
                else if (json["data"]["token"].ToString() == "CREDITS")
                {
                    instance.UpdateCreditsBalance((double)json["data"]["balance_end"]);
                }
                else if (json["data"]["token"].ToString() == "SPSP")
                {
                    instance.UpdateStakedSpsBalance((double)json["data"]["balance_end"]);
                }
            }
            else if (messageType == "quest_progress")
            {
                instance.UpdateFocusInfo(
                    (string)json["data"]["id"],
                    (string)json["data"]["player"],
                    (DateTime)json["data"]["created_date"],
                    (int)json["data"]["created_block"],
                    (string)json["data"]["name"],
                    (int)json["data"]["total_items"],
                    (int)json["data"]["completed_items"],
                    (string?)json["data"]["claim_trx_id"],
                    (DateTime?)json["data"]["claim_date"],
                    (int)json["data"]["reward_qty"],
                    (string?)json["data"]["refresh_trx_id"],
                    (int)json["data"]["chest_tier"],
                    (int)json["data"]["rshares"]
                    );
            }
            else if (messageType == "battle_result")
            {
                instance.UpdateBattleResults((int)json["data"]["status"], json["data"]["winner"].ToString());
            }
            else if (messageType == "received_gifts")
            {
                //ToDo
            }
            else
            {
                Logs.LogMessage($"{username}: UNKNOWN Message received: {message.Text}", Logs.LOG_ALERT, true);
            }
        }
        private void OnReconnectionHappened(ReconnectionInfo info)
        {
            Logs.LogMessage($"{username}: Reconnection happened, type: {info.Type}", supress: true);
        }
        private void OnDisconnectionHappened(DisconnectionInfo disconnectionInfo)
        {
            if(disconnectionInfo.CloseStatusDescription != "UserBenched")
            {
                Logs.LogMessage($"{username}: WebSocket disconnected: {disconnectionInfo.CloseStatusDescription}", Logs.LOG_WARNING,true);
                WebsocketStart().ConfigureAwait(true);
            }
        }
    }
}
