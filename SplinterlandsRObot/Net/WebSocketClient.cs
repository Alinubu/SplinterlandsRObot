using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Models.WebSocket;
using Websocket.Client;
using Websocket.Client.Models;
using SplinterlandsRObot.Global;
using SplinterlandsRObot.Game;

namespace SplinterlandsRObot.Classes.Net
{
    public class WebSocket
    {
        public WebsocketClient client;
        private BotInstance instance { get; set; }
        private string username { get; set; }
        private string accessToken { get; set; }
        public List<WebsoketTransactionMessage> transactions { get; set; }

        public WebSocket(string _username, string _accessToken, BotInstance _instance)
        {
            instance = _instance;
            username = _username;
            accessToken = _accessToken;
            transactions = new List<WebsoketTransactionMessage();
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
            //_ = WebsocketPingLoop().ConfigureAwait(false);
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
        public void WebsocketPing()
        {
            Logs.LogMessage($"{username}: ping", supress: true);
            client.Send("{\"type\":\"ping\"}");
        }
        private async Task WebsocketPingLoop()
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

                    WebsocketPing();
                }
                catch (Exception ex)
                {
                    Logs.LogMessage($"{username}: Error at WebSocket ping { ex }", supress: true);
                }
                finally
                {
                    await WebsocketStop("Ping Error");
                }
            }
        }
        public async Task<bool> WaitForStateChange(WebsocketMessages state, int secondsToWait = 0)
        {
            int maxI = secondsToWait > 0 ? secondsToWait : 1;
            for (int i = 0; i < maxI; i++)
            {
                if (secondsToWait > 0)
                {
                    await Task.Delay(1000);
                }
                if (states.ContainsKey(state))
                {
                    return true;
                }
            }
            return false;
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

            if (messageType == "rating_update")
            {
                if (json["data"].ToString().Contains("new_rating"))
                {
                    instance.UpdateRating((int)json["data"]["new_rating"]);
                }
                if (json["data"].ToString().Contains("new_league"))
                {
                    instance.UpdateLeague((int)json["data"]["new_league"]);
                }
                if (json["data"].ToString().Contains("new_max_league"))
                {
                    instance.UpdateMaxLeague((int)json["data"]["new_max_league"]);
                }
                if (json["data"].ToString().Contains("additional_season_rshares"))
                {
                    instance.UpdateSeasonRewardShares((int)json["data"]["additional_season_rshares"]);
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
                    instance.UpdateECR((double)json["data"]["ecr_update"]);
                }
            }
            else if (messageType == "balance_update")
            {
                if (json["data"]["token"].ToString() == "DEC" && json["data"]["type"])
                {
                    instance.((double)json["data"]["ecr_update"]);
                }
            }

            if (Enum.TryParse(json["id"].ToString(), out WebsocketMessages state))
            {
                Thread.Sleep(1000);
                if (states.ContainsKey(state))
                {
                    states[state] = json["data"];
                }
                else
                {
                    states.Add(state, json["data"]);
                }
            }
            else if (json["data"]["trx_info"] != null
                && !(bool)json["data"]["trx_info"]["success"])
            {
                Logs.LogMessage($"{username}: Transaction error: " + message.Text, Logs.LOG_ALERT);
            }
            else if (message.Text.Contains("Site Maintenance Warning"))
            {
                Logs.LogMessage("Site Maintenance Warning", Logs.LOG_WARNING);
            }
            else
            {
                Logs.LogMessage($"{username}: UNKNOWN Message received: {message.Text}", Logs.LOG_ALERT);
            }

            Logs.LogMessage($"{username}: Message received: {message.Text}", supress: true);
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
