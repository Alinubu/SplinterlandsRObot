using Cryptography.ECDSA;
using HiveAPI.CS;
using System.Text;
using System.Xml;
using SplinterlandsRObot.Models.Account;
using SplinterlandsRObot.Hive;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Global;

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
                        Username = Helpers.ReadNode(node, "username", true),
                        Keys = new Keys()
                        {
                            ActiveKey = Helpers.ReadNode(node, "activeKEY", false, "none"),
                            PostingKey = Helpers.ReadNode(node, "postingKEY", true)
                        },
                        ConfigFile = Helpers.ReadNode(node, "ConfigFile", false, "config.xml")
                    });
            }

            return userList;
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
