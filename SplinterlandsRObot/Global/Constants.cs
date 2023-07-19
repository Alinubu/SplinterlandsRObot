namespace SplinterlandsRObot
{
    public static class Constants
    {
        public const string SPLINTERLANDS_WEBSOCKET_URL = "wss://ws2.splinterlands.com/";
        public const string APP_VERSION = "SplinterlandsRObot 1.2.4";
        public const string SPLINTERLANDS_BATTLE_API = "https://battle.splinterlands.com";
        public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0) Gecko/20100101 Firefox/93.0";
        public const string CONFIG_FOLDER = "Config";
        public static string[] NOT_CHANGEABLE_SETTINGS = new string[] { "MAX_THREADS", "API_URL", "HIVE_NODE" };
    }
}
