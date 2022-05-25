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
                    Logs.LogMessage($"Value for {childPath} is not set or is missing in configuration files. Default value set to [{defaultValue}]", Logs.LOG_ALERT);
                    return defaultValue;
                }
            }
            return defaultValue;
        }
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
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
    }
}
