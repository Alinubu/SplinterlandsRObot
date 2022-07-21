using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplinterlandsRObot.Models.WebSocket
{
    public class WebSocketTransactionMessage
    {
        public JToken message { get; set; }
        public bool processed { get; set; }
    }
}
