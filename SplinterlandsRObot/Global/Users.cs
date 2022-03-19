using Cryptography.ECDSA;
using HiveAPI.CS;
using System.Text;
using System.Xml;
using SplinterlandsRObot.Constructors;
using SplinterlandsRObot.Hive;
using SplinterlandsRObot.API;

namespace SplinterlandsRObot
{
    public class Users
    {
        public List<User> GetUsers()
        {
            Logs.LogMessage("Loading users data, this may take a while...");
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FOLDER, "users.xml"));
            List<User> userList = new();

            foreach (XmlNode node in doc.DocumentElement.SelectNodes("user"))
            {
                userList.Add(
                    new User
                    {
                        Username = node.SelectSingleNode("username").InnerText,
                        PassCodes = new PassCodes
                        {
                            ActiveKey = node.SelectSingleNode("activeKEY").InnerText != "" ? node.SelectSingleNode("activeKEY").InnerText : "",
                            PostingKey = node.SelectSingleNode("postingKEY").InnerText,
                            AccessToken = Task.Run(() => GetAccessToken(node.SelectSingleNode("username").InnerText, node.SelectSingleNode("postingKEY").InnerText)).Result
                        },
                        PowerLimit = node.SelectSingleNode("powerLimit").InnerText != "" ? Convert.ToInt32(node.SelectSingleNode("powerLimit").InnerText) : 0,
                        ECROverride = Convert.ToDouble(node.SelectSingleNode("ECROverride").InnerText),
                        MaxLeague = Convert.ToInt32(node.SelectSingleNode("MaxLeague").InnerText),
                        RentFile = node.SelectSingleNode("RentFile").InnerText
                    });
                Thread.Sleep(625);
            }

            return userList;
        }
        private async Task<string> GetAccessToken(string username, string postingkey)
        {
            HiveActions hive = new();
            Splinterlands sp_api = new();
            var bid = "bid_" + hive.RandomString(20);
            var sid = "sid_" + hive.RandomString(20);
            var ts = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
            var hash = Sha256Manager.GetHash(Encoding.ASCII.GetBytes(username + ts));
            var sig = Secp256K1Manager.SignCompressedCompact(hash, CBase58.DecodePrivateWif(postingkey));
            var signature = Hex.ToString(sig);
            var response = await sp_api.GetUserAccesToken(username, bid, sid, signature, ts);

            var token = hive.DoQuickRegex("\"name\":\"" + username + "\",\"token\":\"([A-Z0-9]{10})\"", response);
            if (token.Length > 0)
            {
                Logs.LogMessage($"{username}: User data loaded.", Logs.LOG_SUCCESS);
            }
            return token;
        }
        internal int GetCardEdition(string editionDescription)
        {
            switch (editionDescription.ToLower())
            {
                case "alpha":
                    return 0;
                case "beta":
                    return 1;
                case "promo":
                    return 2;
                case "reward":
                    return 3;
                case "untamed":
                    return 4;
                case "dice":
                    return 5;
                case "gladius":
                    return 6;
                case "chaos legion":
                    return 7;
                default:
                    return -1;
            }
        }
    }
}
