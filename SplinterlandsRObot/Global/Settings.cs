using SplinterlandsRObot.Global;
using System.Reflection;
using System.Xml;

namespace SplinterlandsRObot
{
    public static class Settings
    {
        public static bool DO_BATTLE { get; private set; }
        public static int MAX_THREADS { get; private set; }
        public static int HOLD_CACHE_FOR { get; set; }
        public static bool DEBUG_MODE { get; set; }
        public static string? API_URL { get; private set; }
        public static string? HIVE_NODE { get; private set; }

        public static void LoadSettings()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FOLDER, "config.xml"));
            XmlNode rootNode = doc.SelectSingleNode("config");
            DO_BATTLE = Convert.ToBoolean(Helpers.ReadNode(doc.SelectSingleNode("config"), "DoBattle", false, "true"));
            MAX_THREADS = Convert.ToInt32(Helpers.ReadNode(rootNode, "MaxThreads", false, "1"));
            HOLD_CACHE_FOR = Convert.ToInt32(Helpers.ReadNode(rootNode, "HoldCacheFor", false, "30"));
            DEBUG_MODE = Convert.ToBoolean(Helpers.ReadNode(rootNode, "DebugMode", false, "false"));
            API_URL = Helpers.ReadNode(rootNode, "ApiUrl", false, "http://api.splinterlandsrobot.com:5000");
            HIVE_NODE = Helpers.ReadNode(rootNode, "HiveNode", false, "https://anyx.io/");
        }

        public static void CheckThreads()
        {
            if ( MAX_THREADS > InstanceManager.userList.Count) MAX_THREADS = 1;
        }
    }
}
