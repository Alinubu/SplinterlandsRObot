using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SplinterlandsRObot.Global
{
    public static class Helpers
    {
        public static string ReadNode(XmlNode parent, string childPath, bool required = false, string defaultValue = "")
        {
            
            if (parent.SelectSingleNode(childPath) != null)
            {
                if (parent.SelectSingleNode(childPath).InnerText != "")
                    return parent.SelectSingleNode(childPath).InnerText;
            }
            else
            {
                if (required)
                {
                    throw new Exception($"Value for {childPath} is not set or is missing. Check configuration files");
                }
                else
                {
                    if (childPath != "ConfigFile")
                        Logs.LogMessage($"Value for {childPath} is not set or is missing in configuration files. Default value set to [{defaultValue}]", Logs.LOG_ALERT);

                    return defaultValue;
                }
            }
            return defaultValue;
        }
        public static string GetMachineIdentifier()
        {
            string fileName = "passkey.txt";
            string id = "";
            if (File.Exists(fileName))
            {
                id = File.ReadLines(fileName).First();
            }

            if (id != "")
                return id;

            id = GenerateMD5Hash(
                Environment.CurrentDirectory +
                Environment.MachineName +
                Environment.OSVersion +
                Environment.WorkingSet +
                Environment.UserName +
                Environment.ProcessorCount +
                Environment.SystemPageSize +
                DateTime.Now.ToBinary() +
                Helpers.RandomString(20));

            File.AppendAllText(fileName, id);

            return id;
        }
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string DoQuickRegex(string Pattern, string Match)
        {
            Regex r = new Regex(Pattern, RegexOptions.Singleline);
            return r.Match(Match).Groups[1].Value;
        }
        public static string GenerateMD5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        internal static int GetCardEdition(string editionDescription)
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
