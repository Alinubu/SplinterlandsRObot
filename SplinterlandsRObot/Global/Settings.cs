using SplinterlandsRObot.Global;
using System.Reflection;
using System.Xml;

namespace SplinterlandsRObot
{
    public static class Settings
    {
        public static bool DO_BATTLE { get; private set; }
        public static int MAX_THREADS { get; private set; }
        public static bool DEBUG_MODE { get; set; }
        public static string? API_URL { get; private set; }
        public static string? HIVE_NODE { get; private set; }

        public static void LoadSettings()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FOLDER, "settings.xml"));
            XmlNode rootNode = doc.SelectSingleNode("settings");
            DO_BATTLE = Convert.ToBoolean(Helpers.ReadNode(rootNode, "DoBattle", false, "true"));
            MAX_THREADS = Convert.ToInt32(Helpers.ReadNode(rootNode, "MaxThreads", false, "1"));
            DEBUG_MODE = Convert.ToBoolean(Helpers.ReadNode(rootNode, "DebugMode", false, "false"));
            API_URL = Helpers.ReadNode(rootNode, "ApiUrl", false, "http://api.splinterlandsrobot.com:5000");
            HIVE_NODE = Helpers.ReadNode(rootNode, "HiveNode", false, "https://anyx.io/");
        }

        public static void ChangeConfig(string settingName, string settingValue)
        {
            try
            {
                if (Constants.NOT_CHANGEABLE_SETTINGS.Contains(settingName))
                {
                    Logs.LogMessage($"Changing {settingName} is not allowed", Logs.LOG_WARNING);
                    return;
                }
                PropertyInfo settingInfo = typeof(Settings).GetProperty(settingName);
                settingInfo.SetValue(settingName, Convert.ChangeType(settingValue, settingInfo.PropertyType), null);
                Logs.LogMessage($"{settingName} has changed to: {settingValue}", Logs.LOG_SUCCESS);
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"Error changing {settingName}: {ex.Message}", Logs.LOG_WARNING);
            }
        }

        public static void CheckThreads()
        {
            if ( MAX_THREADS > InstanceManager.userList.Count) MAX_THREADS = 1;
        }
    }
}
