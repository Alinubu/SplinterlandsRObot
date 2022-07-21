using System.Xml;
using SplinterlandsRObot.Player;
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
    }
}
