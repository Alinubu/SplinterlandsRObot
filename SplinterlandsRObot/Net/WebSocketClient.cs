using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Models;
using Websocket.Client;
using Websocket.Client.Models;
using SplinterlandsRObot.Hive;
using SplinterlandsRObot.Global;

namespace SplinterlandsRObot.Classes.Net
{
    public class WebSocketClient
    {
        public WebsocketClient websocketClient;
        private string username { get; set; }
        private string accessToken { get; set; }
        public Dictionary<GameState,JToken> states { get; set; }

        public WebSocketClient(string _username, string _accessToken)
        {
            username = _username;
            accessToken = _accessToken;
            states = new Dictionary<GameState, JToken>();
            websocketClient = new WebsocketClient(new Uri(Constants.SPLINTERLANDS_WEBSOCKET_URL));
            websocketClient.ReconnectTimeout = new TimeSpan(0, 5, 0);
            websocketClient.MessageReceived.Subscribe(OnMessageReceived);
            websocketClient.ReconnectionHappened.Subscribe(OnReconnectionHappened);
            websocketClient.DisconnectionHappened.Subscribe(OnDisconnectionHappened);
        }

        public async Task WebsocketStart()
        {
            await websocketClient.Start();
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
            websocketClient.Send(message);
        }
        public void WebsocketPing()
        {
            Logs.LogMessage($"{username}: ping", supress: true);
            websocketClient.Send("{\"type\":\"ping\"}");
        }
        private async Task WebsocketPingLoop()
        {
            while (websocketClient.IsStarted)
            {
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Delay(20 * 1000);
                        if (!websocketClient.IsStarted)
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
        public async Task<bool> WaitForStateChange(GameState state, int secondsToWait = 0)
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
            await websocketClient.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, stopDescription);
        }
        public void WebsocketDispose()
        {
            websocketClient.Dispose();
        }
        private void OnMessageReceived(ResponseMessage message)
        {
            if (message.MessageType != System.Net.WebSockets.WebSocketMessageType.Text
                || !message.Text.Contains("\"id\""))
            {
                return;
            }
            JToken json = JToken.Parse(message.Text);
            if (Enum.TryParse(json["id"].ToString(), out GameState state))
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
