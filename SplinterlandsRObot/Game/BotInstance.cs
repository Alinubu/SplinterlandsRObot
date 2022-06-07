﻿using System.Diagnostics;
using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Models;
using SplinterlandsRObot.Classes.Net;
using SplinterlandsRObot.Hive;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Extensions;
using SplinterlandsRObot.Global;

namespace SplinterlandsRObot.Game
{
    public class BotInstance
    {
        #region BotInstance Constructors
        public bool CurrentlyActive { get; set; }
        private int InstanceIndex { get; set; }
        private User UserData { get; set; }
        private int APICounter { get; set; }
        private HiveActions HiveActions { get; set; }
        private WebSocketClient webSocketClient { get; set; }
        private Splinterlands SP_API;
        private Bot BOT_API;
        private DateTime LastCacheUpdate;
        private UserDetails userDetails;
        private UserBalance userBalance;
        private QuestData questData;
        private Quests quests;
        private bool questCompleted = false;
        private CardsCollection CardsCached;
        private double ECRLimit = 0;
        private string questColor = "";
        private string questProgress = "";
        private bool questRenewed = false;
        private string currentSeason = "";
        private bool waitToRechargeECR = false;
        private DateTime SleepUntil;
        private object _activeLock = new();
        private Random random = new Random();
        private bool prioritizeFocus = false;

        public BotInstance(User user, int instanceIndex)
        {
            UserData = user;
            InstanceIndex = instanceIndex;
            SleepUntil = DateTime.Now.AddMinutes((Settings.SLEEP_BETWEEN_BATTLES + 1) * -1);
            APICounter = 99;
            SP_API = new();
            BOT_API = new();
            LastCacheUpdate = DateTime.MinValue;
            userDetails = new();
            userBalance = new();
            questData = new();
            quests = new();
            CardsCached = new();
            HiveActions = new();
            InstanceManager.UsersStatistics.Add(new UserStats()
            {
                Account = UserData.Username,
                Balance = new(),
                RentCost = 0,
                Wins = 0,
                Draws = 0,
                Losses = 0,
                MatchRewards = 0,
                TotalRewards = 0,
                Rating = 0,
                RatingChange = "",
                League = "",
                Quest = "",
                HoursUntilNextQuest = "N/A",
                NextMatchIn = SleepUntil,
                ErrorMessage = ""
            });
            if (!Settings.WINDOWS7)
            {
                webSocketClient = new WebSocketClient(UserData.Username,UserData.PassCodes.AccessToken);
            }
        }
        #endregion

        public async Task<DateTime> DoBattleAsync(int botInstance)
        {
            lock (_activeLock)
            {
                if (CurrentlyActive)
                {
                    
                    Logs.LogMessage($"{UserData.Username} Skipped account because it is currently active", Logs.LOG_ALERT,true);
                    return DateTime.Now.AddSeconds(30);
                }
                CurrentlyActive = true;
                ECRLimit = UserData.ECROverride == 0 ? Settings.ECR_LIMIT : UserData.ECROverride;
            }

            try
            {
                //Check if user can be used
                if (SleepUntil > DateTime.Now)
                {
                    Logs.LogMessage($"{UserData.Username}: Account benched until {SleepUntil}",supress: true);
                    return SleepUntil;
                }

                if (InstanceManager.RentingQueue.ContainsKey(UserData.Username) && !Settings.BATTLE_WHILE_RENTING)
                {
                    SleepUntil = DateTime.Now.AddMinutes(5);
                    return SleepUntil;
                }

                if (!Settings.WINDOWS7)
                {
                    await webSocketClient.WebsocketStart();
                    webSocketClient.states.Clear();
                }

                APICounter++;
                if (APICounter >= 10 || (DateTime.Now - LastCacheUpdate).TotalMinutes >= Settings.HOLD_CACHE_FOR)
                {
                    if (Settings.DO_BATTLE)
                    {
                        APICounter = 0;
                        LastCacheUpdate = DateTime.Now;
                        userDetails = await SP_API.GetUserDetails(UserData.Username);
                        userBalance = await SP_API.GetPlayerBalancesAsync(UserData.Username);
                        CardsCached = await SP_API.GetUserCardsCollection(UserData.Username, UserData.PassCodes.AccessToken);
                        if (Settings.COLLECT_SPS && !Settings.WINDOWS7)
                        {
                            await new CollectSPS().ClaimSPS(UserData);
                        }

                        InstanceManager.UsersStatistics[botInstance].Balance = userBalance;
                        InstanceManager.UsersStatistics[botInstance].Account = UserData.Username;
                        InstanceManager.UsersStatistics[botInstance].Rating = userDetails.rating;
                        InstanceManager.UsersStatistics[botInstance].League = SplinterlandsData.splinterlandsSettings.leagues[userDetails.league].name;
                        InstanceManager.UsersStatistics[botInstance].CollectionPower = userDetails.collection_power;
                    }
                }

                if (Settings.USE_RENTAL_BOT)
                {
                    try
                    {
                        if (userDetails.collection_power < UserData.PowerLimit)
                        {
                            if (!InstanceManager.RentingQueue.ContainsKey(UserData.Username))
                            {
                                Logs.LogMessage($"{UserData.Username}: CP is below limit, user added to renting queue", Logs.LOG_ALERT);
                                InstanceManager.RentingQueue.Add(UserData.Username, (Settings.DO_BATTLE ? userDetails.collection_power : SP_API.GetUserDetails(UserData.Username).Result.collection_power));
                            }
                            else
                            {
                                Logs.LogMessage($"{UserData.Username}: CP is below limit, user already in renting queue", Logs.LOG_ALERT);
                            }
                                
                            if (!Settings.BATTLE_WHILE_RENTING)
                            {
                                SleepUntil = DateTime.Now.AddMinutes(5);
                                return SleepUntil;
                            }
                            APICounter = 99;
                        }
                        else
                        {
                            Logs.LogMessage($"{UserData.Username}: CP OK, account ready for battle.", Logs.LOG_SUCCESS);
                        }
                    }
                    catch (Exception)
                    {
                        Logs.LogMessage($"{UserData.Username} Rent Bot Error", Logs.LOG_WARNING);
                    }
                }

                if (!Settings.DO_BATTLE)
                {
                    SleepUntil = DateTime.Now.AddMinutes(5);
                    return SleepUntil;
                }

                if (Settings.LEAGUE_ADVANCE_TO_NEXT) await AdvanceLeague();

                if (Settings.DO_QUESTS)
                    Logs.LogMessage($"{UserData.Username}: Daily Focus enabled", Logs.LOG_SUCCESS, true);

                questData = await SP_API.GetQuestData(UserData.Username);
                
                if (questData != null)
                {
                    questColor = quests.GetQuestColor(questData.name, UserData);
                    if (questColor == "THISWILLBEREMOVEDATSOMEPOINT")
                    {
                        if (questData.completed_items > 0 && questData.completed_items == questData.total_items)
                        {
                            Logs.LogMessage($"{UserData.Username}: Old quest rewards found, trying to claim before requesting a new Focus", Logs.LOG_ALERT);
                            quests.ClaimQuestReward(questData,UserData,userDetails);
                        }
                        Logs.LogMessage($"{UserData.Username}: Cannot find Focus name. Maybe old quest active, requesting a new Focus", Logs.LOG_ALERT);
                        if (new HiveActions().StartQuest(UserData))
                        {
                            await Task.Delay(10000);

                        }
                        questData = await SP_API.GetQuestData(UserData.Username);
                        questColor = quests.GetQuestColor(questData.name, UserData);
                        if (questColor == "THISWILLBEREMOVEDATSOMEPOINT")
                        {
                            Logs.LogMessage($"{UserData.Username}: Cannot find Focus name. Maybe old quest ongoing, requesting a new Focus a different way", Logs.LOG_ALERT);
                            if (quests.RequestNewFocus(UserData))
                            {
                                await Task.Delay(10000);
                                questData = await SP_API.GetQuestData(UserData.Username);
                                questColor = quests.GetQuestColor(questData.name, UserData);
                            }
                        }
                    }

                    questData.earned_chests = quests.CalculateEarnedChests((int)questData.chest_tier, questData.rshares);
                    questRenewed = questData.refresh_trx_id != null ? true : false;

                    if (Settings.CLAIM_QUEST_REWARDS)
                    {
                        if ((24 - (DateTime.Now - questData.created_date.ToLocalTime()).TotalHours) < 0 && questData.claim_trx_id == null && questData.earned_chests > 0)
                        {
                            Logs.LogMessage($"{UserData.Username}: 24h passed since Daily Focus was started. Claiming Rewards...", Logs.LOG_ALERT);
                            if (await quests.ClaimQuestReward(questData, UserData, userDetails))
                            {
                                questData = await SP_API.GetQuestData(UserData.Username);
                                await Task.Delay(5000);
                            }
                        }
                    }

                    if (await quests.CheckForNewQuest(questData, UserData))
                    {
                        questRenewed = false;
                        questData = await SP_API.GetQuestData(UserData.Username);
                    }

                    questProgress = quests.GetQuestProgress(questData.earned_chests, (int)questData.chest_tier, questData.rshares);

                    if (Settings.DO_QUESTS)
                        prioritizeFocus = quests.IsFocusPrio(random, questColor);

                    if (Settings.DO_QUESTS && Settings.AVOID_SPECIFIC_QUESTS && !questRenewed)
                    {
                        if(Settings.AVOID_SPECIFIC_QUESTS_LIST.Length > 0 && Settings.AVOID_SPECIFIC_QUESTS_LIST.Any(x => x.Contains(questColor)))
                        {
                            Logs.LogMessage($"{UserData.Username}: Daily Focus blacklisted, requesting a new one...", Logs.LOG_ALERT);
                            if (quests.RequestNewQuest(questData,UserData,questColor))
                            {
                                Logs.LogMessage($"{UserData.Username}: New Daily Focus received", Logs.LOG_SUCCESS);
                                questRenewed = true;
                                questData = await SP_API.GetQuestData(UserData.Username);
                                questColor = quests.GetQuestColor(questData.name, UserData);
                                //Added this here just to be safe
                                if (questColor == "THISWILLBEREMOVEDATSOMEPOINT")
                                {
                                    questData = await SP_API.GetQuestData(UserData.Username);
                                    questColor = quests.GetQuestColor(questData.name, UserData);
                                }
                                APICounter = 99;
                            }
                            else
                            {
                                Logs.LogMessage($"{UserData.Username}: Error requesting new Daily Focus", Logs.LOG_WARNING);
                            }
                        }
                    }

                    if (Settings.DO_QUESTS)
                        Logs.LogMessage($"{UserData.Username}: Current Focus: {questColor}", Logs.LOG_ALERT, true);

                    InstanceManager.UsersStatistics[botInstance].Quest = $"{questColor}[{questProgress}]";
                    InstanceManager.UsersStatistics[botInstance].HoursUntilNextQuest = (24 - (DateTime.Now - questData.created_date.ToLocalTime()).TotalHours).ToString();
                }
                else
                {
                    Logs.LogMessage($"{UserData.Username}: Having issues requesting Daily Focus info", Logs.LOG_WARNING);
                }

                Logs.LogMessage($"{UserData.Username}: Current Energy Capture Rate is {userBalance.ECR}%", supress: true);
                
                if (!waitToRechargeECR)
                {
                    if ((userBalance.ECR) < ECRLimit)
                    {
                        Logs.LogMessage($"{UserData.Username}: ECR limit reached, moving to next account [{Math.Round(userBalance.ECR)}%/{Settings.ECR_LIMIT}%] ", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(5);
                        await Task.Delay(1500);
                        if (Settings.ECR_WAIT_TO_RECHARGE)
                        {
                            waitToRechargeECR = true;
                            Logs.LogMessage($"{UserData.Username}: Recharge enabled, user will wait until ECR is {Settings.ECR_RECHARGE_LIMIT}%", Logs.LOG_ALERT);
                        }
                        SleepUntil = DateTime.Now.AddMinutes(5);
                        return SleepUntil;
                    }
                }
                else
                {
                    if ((userBalance.ECR) < Settings.ECR_RECHARGE_LIMIT)
                    {
                        Logs.LogMessage($"{UserData.Username}: Recharge enabled, ECR {Math.Round(userBalance.ECR)}%/{Settings.ECR_RECHARGE_LIMIT}%. Moving to next account", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(10);
                        return SleepUntil;
                    }
                    else
                    {
                        Logs.LogMessage($"{UserData.Username}: ECR Restored {Math.Round(userBalance.ECR)}%/{Settings.ECR_RECHARGE_LIMIT}%", Logs.LOG_SUCCESS);
                        waitToRechargeECR = false;
                    }
                }

                //Check for api limit
                if (!Settings.USE_PRIVATE_API)
                {
                    //Check public api limit
                    if(!await BOT_API.CheckPublicAPILimit())
                    {
                        Logs.LogMessage($"{UserData.Username}: Public API Limit exceeded. Waiting 10 minutes", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(10);
                        return SleepUntil;
                    }
                }

                //Starting a new match
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string jsonResponsePlain = HiveActions.StartNewMatch(UserData);
                string tx = Helpers.DoQuickRegex("id\":\"(.*?)\"", jsonResponsePlain);
                bool submitTeam = true;
                JToken matchDetails = null;

                if (jsonResponsePlain == "" || !jsonResponsePlain.Contains("success") || !await WaitForTransactionSuccess(tx, 30))
                {
                    var outstandingGame = await SP_API.GetOutstandingMatch(UserData.Username);
                    if (outstandingGame != "null")
                    {
                        tx = Helpers.DoQuickRegex("\"id\":\"(.*?)\"", outstandingGame);
                        var teamHash = Helpers.DoQuickRegex("\"team_hash\":\"(.*?)\"", outstandingGame);
                        Logs.LogMessage($"{UserData.Username}: Found ongoing match: " + tx, Logs.LOG_WARNING);
                        if (teamHash.Length == 0)
                        {
                            Logs.LogMessage($"{UserData.Username}: No team submitted, proceeding with team submision", Logs.LOG_WARNING);
                            matchDetails = JToken.Parse(outstandingGame);
                        }
                        else
                        {
                            Logs.LogMessage($"{UserData.Username}: Team already submitted for this match", Logs.LOG_WARNING);
                            submitTeam = false;
                        }
                    }
                    else
                    {
                        var sleepTime = 5;
                        Logs.LogMessage($"{UserData.Username}: Error requesting new match({tx}). Waiting {sleepTime} minutes.", Logs.LOG_WARNING);
                        SleepUntil = DateTime.Now.AddMinutes(sleepTime);
                        return SleepUntil;
                    }
                }

                JToken team = new JObject();
                bool surrender = false;
                SleepUntil = DateTime.Now.AddMinutes(Settings.SLEEP_BETWEEN_BATTLES);

                if (submitTeam)
                {
                    if (matchDetails == null)
                    {
                        if (Settings.WINDOWS7)
                        {
                            matchDetails = await WaitForMatchDetails(tx);
                            if (matchDetails == null)
                            {
                                Logs.LogMessage($"{UserData.Username}: Banned from ranked? Sleeping for 10 minutes!", Logs.LOG_WARNING);
                                SleepUntil = DateTime.Now.AddMinutes(10);
                                return SleepUntil;
                            }
                        }
                        else
                        {
                            if (!await webSocketClient.WaitForStateChange(GameState.match_found, 185))
                            {
                                Logs.LogMessage($"{UserData.Username}: Check for Ranked mode Ban. Waiting 30 minutes!", Logs.LOG_WARNING);
                                SleepUntil = DateTime.Now.AddMinutes(30);
                                return SleepUntil;
                            }

                            matchDetails = webSocketClient.states[GameState.match_found];
                        }
                        Logs.LogMessage($"{UserData.Username}: New match found - Manacap: {matchDetails["mana_cap"]}; Ruleset: {matchDetails["ruleset"]}; Inactive splinters: {matchDetails["inactive"]}", Logs.LOG_ALERT);
                    }
                    
                    try
                    {
                        if (Settings.USE_PRIVATE_API)
                        {
                            Logs.LogMessage($"{UserData.Username}: Fetching team from private api");
                            try
                            {
                                team = await BOT_API.GetTeamFromAPI(matchDetails, questColor, questCompleted, CardsCached, UserData.Username, userDetails.league, prioritizeFocus, true);
                            }
                            catch (Exception ex)
                            {
                                Logs.LogMessage($"{UserData.Username}: Error fetching team from private api. Trying on public api", Logs.LOG_ALERT);
                                team = await BOT_API.GetTeamFromAPI(matchDetails, questColor, questCompleted, CardsCached, UserData.Username, userDetails.league, prioritizeFocus, false);
                            }
                        }
                        else
                        {
                            Logs.LogMessage($"{UserData.Username}: Fetching team from public api");
                            team = await BOT_API.GetTeamFromAPI(matchDetails, questColor, questCompleted, CardsCached, UserData.Username, userDetails.league, prioritizeFocus, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logs.LogMessage(ex.Message, Logs.LOG_WARNING,true);
                    }

                    if (!team.HasValues)
                    {
                        Logs.LogMessage($"{UserData.Username}: API couldn't find any team - Skipping Account", Logs.LOG_WARNING);
                        SleepUntil = DateTime.Now.AddMinutes(5);
                        return SleepUntil;
                    }
                    else
                    {
                        Logs.OutputTeam(UserData.Username, team["summonerName"].ToString(), team["card1Name"].ToString(), team["card2Name"].ToString(), team["card3Name"].ToString(), team["card4Name"].ToString(), team["card5Name"].ToString(), team["card6Name"].ToString());
                    }

                    await Task.Delay(new Random().Next(3000, 8000));
                    var submittedTeam = HiveActions.SubmitTeam(tx, matchDetails, team, UserData, CardsCached);
                    Logs.LogMessage($"{UserData.Username}: Team submitted. TX:{submittedTeam.tx}", supress: true);
                    if (!await WaitForTransactionSuccess(submittedTeam.tx, 15))
                    {
                        SleepUntil = DateTime.Now.AddMinutes(Settings.SLEEP_BETWEEN_BATTLES);
                    }

                    while (stopwatch.Elapsed.Seconds < 145 && !surrender)
                    {
                        if (Settings.WINDOWS7)
                        {
                            surrender = await WaitForEnemyPick(tx, stopwatch);
                            break;
                        }
                        else
                        {
                            if (await webSocketClient.WaitForStateChange(GameState.opponent_submit_team, 4))
                            {
                                break;
                            }
                            // if there already is a battle result now it's because the enemy surrendered or the game vanished
                            if (await webSocketClient.WaitForStateChange(GameState.battle_result) || await webSocketClient.WaitForStateChange(GameState.battle_cancelled))
                            {
                                surrender = true;
                                break;
                            }
                        }
                    }

                    stopwatch.Stop();
                    if (surrender)
                    {
                        Logs.LogMessage($"{UserData.Username}: Enemy probably surrendered", Logs.LOG_WARNING);
                    }
                    else
                    {
                        if (!HiveActions.RevealTeam(tx, matchDetails, team, submittedTeam.secret, UserData, CardsCached))
                        {
                            Logs.LogMessage($"{UserData.Username}: Error revealing team. Trying again", Logs.LOG_WARNING);
                            if (!HiveActions.RevealTeam(tx, matchDetails, team, submittedTeam.secret, UserData, CardsCached))
                                Logs.LogMessage($"{UserData.Username}: Error revealing team on second attempt", Logs.LOG_WARNING);
                        }
                            
                    }
                }
                else
                {
                    if (!HiveActions.SurrenderBattle(tx, UserData))
                        Logs.LogMessage($"{UserData.Username}: Error surrendering match.", Logs.LOG_WARNING);
                }

                Logs.LogMessage($"{UserData.Username}: Battle finished!");

                if (Settings.SHOW_BATTLE_RESULTS)
                {
                    await ShowBattleResult(tx, surrender);
                    InstanceManager.UsersStatistics[botInstance].Balance.ECR = Math.Round(userBalance.ECR != null ? (double)userBalance.ECR : 0, 2);
                }
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{UserData.Username}: {ex}{Environment.NewLine}Skipping Account", Logs.LOG_WARNING);
            }
            finally
            {
                if (!Settings.WINDOWS7) await webSocketClient.WebsocketStop("UserBenched");
                lock (_activeLock)
                {
                    CurrentlyActive = false;
                }
            }
            return SleepUntil;
        }

        private async Task ShowBattleResult(string tx, bool surrender)
        {
            if (Settings.WINDOWS7)
            {
                await ShowBattleResultLegacyAsync(tx);
                return;
            }

            if (!await webSocketClient.WaitForStateChange(GameState.battle_result, 210))
            {
                Logs.LogMessage($"{UserData.Username}: Error fetching battle result", Logs.LOG_WARNING);
            }
            else
            {
                decimal decReward = await webSocketClient.WaitForStateChange(GameState.balance_update, 10) ?
                    (decimal)webSocketClient.states[GameState.balance_update]["amount"] : 0;

                int newRating = await webSocketClient.WaitForStateChange(GameState.rating_update) ?
                    (int)webSocketClient.states[GameState.rating_update]["new_rating"] : userDetails.rating;
                int ratingChange = newRating - userDetails.rating;

                if (await webSocketClient.WaitForStateChange(GameState.ecr_update))
                {
                    userBalance.ECR = ((double)webSocketClient.states[GameState.ecr_update]["capture_rate"]) / 100;
                }
                if (await webSocketClient.WaitForStateChange(GameState.quest_progress))
                {
                    questData = await SP_API.GetQuestData(UserData.Username);
                }
                userDetails.rating = newRating;

                int battleResult = 0;
                if ((string)webSocketClient.states[GameState.battle_result]["winner"] == UserData.Username)
                {
                    battleResult = 1;
                }
                else if ((string)webSocketClient.states[GameState.battle_result]["winner"] == "DRAW")
                {
                    battleResult = 2;
                }

                switch (battleResult)
                {
                    case 2:
                        Logs.LogMessage($"{UserData.Username}: Match was a Draw [Rating: { newRating }]", Logs.LOG_ALERT);
                        InstanceManager.UsersStatistics[InstanceIndex].Draws++;
                        break;
                    case 1:
                        Logs.LogMessage($"{UserData.Username}: Match Won {(surrender ? "(Enemy surrendered) " : "")}[Rating: {newRating}(+{ratingChange.ToString()}); Reward: { decReward } DEC]", Logs.LOG_SUCCESS);
                        InstanceManager.UsersStatistics[InstanceIndex].Wins++;
                        InstanceManager.UsersStatistics[InstanceIndex].MatchRewards = Convert.ToDouble(decReward);
                        InstanceManager.UsersStatistics[InstanceIndex].TotalRewards = InstanceManager.UsersStatistics[InstanceIndex].TotalRewards + Convert.ToDouble(decReward);
                        InstanceManager.UsersStatistics[InstanceIndex].RatingChange = "+" + ratingChange.ToString();
                        InstanceManager.UsersStatistics[InstanceIndex].Rating = newRating;

                        break;
                    case 0:
                        Logs.LogMessage($"{UserData.Username}: Match Lost [Rating: {newRating}({ratingChange.ToString()})]", Logs.LOG_WARNING);
                        InstanceManager.UsersStatistics[InstanceIndex].Losses++;
                        InstanceManager.UsersStatistics[InstanceIndex].RatingChange = ratingChange.ToString();
                        InstanceManager.UsersStatistics[InstanceIndex].Rating = newRating;
                        break;
                    default:
                        break;
                }
            }
        }
        private async Task<bool> WaitForTransactionSuccess(string tx, int secondsToWait)
        {
            if (tx.Length == 0)
            {
                return false;
            }
            else if (Settings.WINDOWS7)
            {
                return true;
            }

            for (int i = 0; i < secondsToWait * 2; i++)
            {
                await Task.Delay(500);
                if (webSocketClient.states.ContainsKey(GameState.transaction_complete)
                    && (string)webSocketClient.states[GameState.transaction_complete]["trx_info"]["id"] == tx)
                {
                    if ((bool)webSocketClient.states[GameState.transaction_complete]["trx_info"]["success"])
                    {
                        return true;
                    }
                    else
                    {
                        // The specified battle has already been resolved
                        Logs.LogMessage($"{UserData.Username}: Transaction error: " + tx + " - " + (string)webSocketClient.states[GameState.transaction_complete]["trx_info"]["error"], Logs.LOG_WARNING);
                        return false;
                    }
                }
            }
            Logs.LogMessage($"{UserData.Username}: No response from websocket.", Logs.LOG_WARNING);
            return false;
        }

        #region Windows7 Legacy Mode
        private async Task ShowBattleResultLegacyAsync(string tx)
        {
            (int newRating, int ratingChange, decimal decReward, int result) battleResult = new();
            for (int i = 0; i < 14; i++)
            {
                await Task.Delay(6000);
                battleResult = await SP_API.GetBattleResultAsync(UserData.Username, tx);
                if (battleResult.result >= 0)
                {
                    break;
                }
                await Task.Delay(9000);
            }

            if (battleResult.result == -1)
            {
                Logs.LogMessage($"{UserData.Username}: Error fetching battle result");
                return;
            }

            switch (battleResult.result)
            {
                case 2:
                    Logs.LogMessage($"{UserData.Username}: Match was a Draw [Rating: { battleResult.newRating }]", Logs.LOG_ALERT);
                    InstanceManager.UsersStatistics[InstanceIndex].Draws++;
                    break;
                case 1:
                    Logs.LogMessage($"{UserData.Username}: Match Won [Rating: {battleResult.newRating}(+{battleResult.ratingChange.ToString()}); Reward: { battleResult.decReward } DEC]", Logs.LOG_SUCCESS);
                    InstanceManager.UsersStatistics[InstanceIndex].Wins++;
                    InstanceManager.UsersStatistics[InstanceIndex].MatchRewards = Convert.ToDouble(battleResult.decReward);
                    InstanceManager.UsersStatistics[InstanceIndex].TotalRewards = InstanceManager.UsersStatistics[InstanceIndex].TotalRewards + Convert.ToDouble(battleResult.decReward);
                    InstanceManager.UsersStatistics[InstanceIndex].RatingChange = "+" + battleResult.ratingChange.ToString();
                    InstanceManager.UsersStatistics[InstanceIndex].Rating = battleResult.newRating;

                    break;
                case 0:
                    Logs.LogMessage($"{UserData.Username}: Match Lost [Rating: {battleResult.newRating}({battleResult.ratingChange.ToString()})]", Logs.LOG_WARNING);
                    InstanceManager.UsersStatistics[InstanceIndex].Losses++;
                    InstanceManager.UsersStatistics[InstanceIndex].RatingChange = battleResult.ratingChange.ToString();
                    InstanceManager.UsersStatistics[InstanceIndex].Rating = battleResult.newRating;
                    break;
                default:
                    break;
            }
        }

        private async Task<JToken> WaitForMatchDetails(string trxId)
        {
            for (int i = 0; i < 9; i++) // 9 * 20 = 180, so 3mins
            {
                try
                {
                    await Task.Delay(7500);
                    JToken matchDetails = await SP_API.GetMatchDetails(trxId);
                    if (i > 2 && ((string)matchDetails).Contains("no battle"))
                    {
                        Logs.LogMessage($"{UserData.Username}: Cannot fetch match details: {matchDetails}", Logs.LOG_WARNING);
                        return null;
                    }
                    if (matchDetails["mana_cap"].Type != JTokenType.Null)
                    {
                        return matchDetails;
                    }
                    await Task.Delay(12500);
                }
                catch (Exception ex)
                {
                    if (i > 9)
                    {
                        Logs.LogMessage($"{UserData.Username}: Cannot fetch match details: {ex.Message}", Logs.LOG_WARNING);
                    }
                }
            }
            return null;
        }

        private async Task<bool> WaitForEnemyPick(string tx, Stopwatch stopwatch)
        {
            do
            {
                var enemyHasPicked = await SP_API.CheckEnemyHasPickedAsync(UserData.Username, tx);
                if (enemyHasPicked.enemyHasPicked)
                {
                    return enemyHasPicked.surrender;
                }
                await Task.Delay(stopwatch.Elapsed.TotalSeconds > 170 ? 2500 : 15000);
            } while (stopwatch.Elapsed.TotalSeconds < 179);
            return false;
        }
        #endregion

        #region League
        private async Task AdvanceLeague()
        {
            try
            {
                int highestPossibleLeague = GetMaxLeagueByRankAndPower(userDetails.rating - Settings.LEAGUE_RATING_THRESHOLD, userDetails.collection_power);
                if (highestPossibleLeague > userDetails.league && highestPossibleLeague <= (UserData.MaxLeague == 0 ? 13 : UserData.MaxLeague))
                {
                    Logs.LogMessage($"{UserData.Username}: Advancing to higher league!", Logs.LOG_SUCCESS);
                    APICounter = 100; // set api counter to 100 to reload details

                    string tx = HiveActions.AdvanceLeague(UserData);
                    if (await WaitForTransactionSuccess(tx, 45))
                    {
                        Logs.LogMessage($"{UserData.Username}: Advanced league: {tx}");
                    }

                    APICounter = 100; // set api counter to 100 to reload details
                }
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{UserData.Username}: Error at advancing league: {ex}");
            }
        }
        private int GetMaxLeagueByRankAndPower(int rating, int power)
        {
            //champion
            if ((rating is >= 3700) && (power is >= 500000))
            {
                return 13;
            }
            //diamong
            if ((rating is >= 2800) && (power is >= 250000))
            {
                return 10;
            }
            //gold
            if ((rating is >= 1900) && (power is >= 100000))
            {
                return 7;
            }
            //silver
            if ((rating is >= 1000) && (power is >= 15000))
            {
                return 4;
            }

            return 0;
        }
        #endregion
    }
}
