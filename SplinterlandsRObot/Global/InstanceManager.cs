﻿using System.Net;
using HiveAPI.CS;
using SplinterlandsRObot.Constructors;
using SplinterlandsRObot.Game;

namespace SplinterlandsRObot
{
    public static class InstanceManager
    {
        public static List<BotInstance> BotInstances = new();
        public static List<User> userList = new();
        public static List<UserStats> UsersStatistics = new();
        public static Dictionary<string,int> RentingQueue = new();
        public static object StartBattleLock = new();
        public static HttpClient HttpClient = new();
        public static CookieContainer CookieContainer = new();
        public static CHived oHived;
        public static void CreateUsersInstance()
        {
            userList = new Users().GetUsers();
        }

        public static void CreateBotInstances(List<User> userList)
        {
            oHived = new CHived(HttpClient, Constants.HIVE_NODE);
            int index = 0;
            foreach (User user in userList)
            {
                BotInstances.Add(new BotInstance(user, index));
                index++;
            }
        }
    }
}
