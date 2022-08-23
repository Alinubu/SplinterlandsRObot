using System.Xml;
using SplinterlandsRObot.Player;
using SplinterlandsRObot.Global;

namespace SplinterlandsRObot
{
    public class Users
    {
        public List<User> GetUsers()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FOLDER, "users.xml"));
            List<User> userList = new();
            int i = 0;
            foreach (XmlNode node in doc.DocumentElement.SelectNodes("user"))
            {
                userList.Add(
                    new User
                    {
                        Username = Helpers.ReadNode(node, "username", true),
                        Keys = new Keys()
                        {
                            ActiveKey = Helpers.ReadNode(node, "activeKEY", false, "none"),
                            PostingKey = Helpers.ReadNode(node, "postingKEY", true),
                            JwtToken = null,
                            JwtExpire = DateTime.MinValue
                        },
                        ConfigFile = Helpers.ReadNode(node, "ConfigFile", false, "config.xml")
                    });
                i++;
            }

            Logs.LogMessage($"Loaded {i} users.");

            return userList;
        }
    }
}
