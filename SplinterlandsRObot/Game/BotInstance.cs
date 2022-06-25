using System.Diagnostics;
using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Models.Account;
using SplinterlandsRObot.Classes.Net;
using SplinterlandsRObot.Hive;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Extensions;
using SplinterlandsRObot.Global;
using Newtonsoft.Json;
using SplinterlandsRObot.Models.Bot;

namespace SplinterlandsRObot.Game
{
    public class BotInstance
    {
        #region BotInstance Constructor
        private object _activeLock = new();
        public bool CurrentlyActive { get; set; }
        private int InstanceIndex { get; set; }
        private User UserData { get; set; }
        private Config UserConfig { get; set; }
        private UserDetails UserDetails = new();
        private UserBalance UserBalance = new();
        private CardsCollection UserCards = new();
        private Focus focus = new();
        private BattleState BattleState = new();
        private HiveActions HiveActions = new();
        private Splinterlands SplinterlandsAPI = new();
        private Bot BotAPI = new();
        private WebSocket WebSocket { get; set; }
        
        private Random random = new Random();

        private DateTime LastLogin = DateTime.MinValue;
        private DateTime LastAirdrop = DateTime.MinValue;
        private double ECR = 0;
        private bool waitForECR = false;
        private bool focusCompleted = false;
        private string focusSplinter = "";
        private string focusProgress = "";
        private string lastRatingUpdate = "0";
        private bool focusRenewed = false;
        private DateTime SleepUntil = DateTime.MinValue;
        private bool refreshCards = true;
        private bool prioritizeFocus = false;
        JToken matchDetails = null;

        public BotInstance(User user, int instanceIndex)
        {
            UserData = user;
            InstanceIndex = instanceIndex;
            UserDetails = HiveActions.GetUserDetails(UserData.Username, UserData.Keys.PostingKey);
            ECR = ComputeECR(UserDetails.balances);
            UserBalance = new UserBalance()
            {
                Credits = UserDetails.balances.Where(x => x.token == "CREDITS").Any() ? UserDetails.balances.Where(x => x.token == "CREDITS").First().balance : 0,
                DEC = UserDetails.balances.Where(x => x.token == "DEC").Any() ? UserDetails.balances.Where(x => x.token == "DEC").First().balance : 0,
                LegendaryPotions = UserDetails.balances.Where(x => x.token == "LEGENDARY").Any() ? (int)UserDetails.balances.Where(x => x.token == "LEGENDARY").First().balance : 0,
                GoldPotions = UserDetails.balances.Where(x => x.token == "GOLD").Any() ? (int)UserDetails.balances.Where(x => x.token == "GOLD").First().balance : 0,
                QuestPotions = UserDetails.balances.Where(x => x.token == "QUEST").Any() ? (int)UserDetails.balances.Where(x => x.token == "QUEST").First().balance : 0,
                Packs = UserDetails.balances.Where(x => x.token == "CHAOS").Any() ? (int)UserDetails.balances.Where(x => x.token == "CHAOS").First().balance : 0,
                Voucher = UserDetails.balances.Where(x => x.token == "VOUCHER").Any() ? UserDetails.balances.Where(x => x.token == "VOUCHER").First().balance : 0,
                SPS = UserDetails.balances.Where(x => x.token == "SPS").Any() ? UserDetails.balances.Where(x => x.token == "SPS").First().balance : 0,
                SPSP = UserDetails.balances.Where(x => x.token == "SPSP").Any() ? UserDetails.balances.Where(x => x.token == "SPSP").First().balance : 0,
                ECR = ECR
            };
            LastLogin = DateTime.Now;
            InstanceManager.UsersStatistics.Add(new UserStats()
            {
                Account = UserData.Username,
                Balance = UserBalance,
                RentCost = 0,
                Wins = 0,
                Draws = 0,
                Losses = 0,
                MatchRewards = 0,
                TotalRewards = 0,
                Rating = UserDetails.rating,
                RatingChange = "",
                League = SplinterlandsData.splinterlandsSettings.leagues[UserDetails.league].name,
                CollectionPower = UserDetails.collection_power,
                Focus = "",
                HoursUntilNextQuest = "N/A",
                NextMatchIn = SleepUntil,
                ErrorMessage = ""
            });
            
            WebSocket = new WebSocket(UserData.Username,UserDetails.token, this);
        }

        internal void UpdateLeague(int v)
        {
            throw new NotImplementedException();
        }

        internal void UpdateMaxLeague(int v)
        {
            throw new NotImplementedException();
        }

        internal void UpdateSeasonRewardShares(int v)
        {
            throw new NotImplementedException();
        }

        internal void UpdateRating(int v)
        {
            throw new NotImplementedException();
        }

        internal void UpdateCollectionPower(int v)
        {
            throw new NotImplementedException();
        }
        #endregion

        public async Task<DateTime> DoBattleAsync(int botInstance)
        {
            lock (_activeLock)
            {
                if (CurrentlyActive)
                {
                    Logs.LogMessage($"{UserData.Username}: Account already in a battle", Logs.LOG_ALERT, true);
                    return DateTime.Now.AddSeconds(30);
                }
                CurrentlyActive = true;
            }

            try
            {
                if (SleepUntil > DateTime.Now)
                {
                    Logs.LogMessage($"{UserData.Username}: Account benched until {SleepUntil}",supress: true);
                    return SleepUntil;
                }

                if (InstanceManager.RentingQueue.ContainsKey(UserData.Username) && !UserConfig.BattleWhileRenting)
                {
                    SleepUntil = DateTime.Now.AddMinutes(5);
                    return SleepUntil;
                }

                if (!WebSocket.client.IsStarted)
                {
                    await WebSocket.WebsocketStart();
                    WebSocket.states.Clear();
                }
                
                if (refreshCards)
                    UserCards = await SplinterlandsAPI.GetUserCardsCollection(UserData.Username, UserDetails.token);
                if (UserConfig.ClaimSPS && (DateTime.Now - LastAirdrop).TotalHours > UserConfig.CheckForAirdropEvery)
                    await new CollectSPS().ClaimSPS(UserData, UserDetails.token);

                if (UserConfig.EnableRentals)
                {
                    if (UserDetails.collection_power < UserConfig.PowerLimit)
                    {
                        if (!InstanceManager.RentingQueue.ContainsKey(UserData.Username))
                        {
                            Logs.LogMessage($"{UserData.Username}: CP is below limit, user added to renting queue", Logs.LOG_ALERT);
                            InstanceManager.RentingQueue.Add(UserData.Username, UserDetails.collection_power);
                        }
                        else
                        {
                            Logs.LogMessage($"{UserData.Username}: CP is below limit, user already in renting queue", Logs.LOG_ALERT);
                        }
                                
                        if (!UserConfig.BattleWhileRenting)
                        {
                            SleepUntil = DateTime.Now.AddMinutes(5);
                            return SleepUntil;
                        }
                    }
                }

                if (!Settings.DO_BATTLE)
                {
                    SleepUntil = DateTime.Now.AddMinutes(5);
                    return SleepUntil;
                }

                if (UserConfig.LeagueAdvance) await AdvanceLeague();

                if (UserConfig.FocusEnabled)
                    Logs.LogMessage($"{UserData.Username}: Daily Focus enabled", Logs.LOG_SUCCESS, true);

                if (UserDetails.quest != null)
                {
                    focusSplinter = focus.GetFocusSplinter(UserDetails.quest.name);
                    if (focusSplinter == "THISWILLBEREMOVEDATSOMEPOINT")
                    {
                        if (UserDetails.quest.completed_items > 0 && UserDetails.quest.completed_items == UserDetails.quest.total_items)
                        {
                            Logs.LogMessage($"{UserData.Username}: Old quest rewards found, trying to claim before requesting a new Focus", Logs.LOG_ALERT);
                            focus.ClaimQuestReward(UserDetails.quest, UserData);
                            //ADD SOCKET CHECK FOR CLAIM TX
                        }
                        Logs.LogMessage($"{UserData.Username}: Cannot find Focus name. Maybe old quest active, requesting a new Focus", Logs.LOG_ALERT);
                        if (new HiveActions().StartFocus(UserData))
                        {
                            //ADD SOCKET CHECK FOR NEW FOCUS TX

                        }
                        
                        focusSplinter = focus.GetFocusSplinter(UserDetails.quest.name);
                        if (focusSplinter == "THISWILLBEREMOVEDATSOMEPOINT")
                        {
                            Logs.LogMessage($"{UserData.Username}: Cannot find Focus name. Maybe old quest ongoing, requesting a new Focus a different way", Logs.LOG_ALERT);
                            if (focus.RequestNewFocus(UserData))
                            {
                                //ADD SOCKET CHECK FOR NEW FOCUS TX
                                focusSplinter = focus.GetFocusSplinter(UserDetails.quest.name);
                            }
                        }
                    }

                    UserDetails.quest.earned_chests = focus.CalculateEarnedChests((int)UserDetails.quest.chest_tier, UserDetails.quest.rshares);
                    focusRenewed = UserDetails.quest.refresh_trx_id != null ? true : false;

                    if (UserConfig.ClaimFocusChests)
                    {
                        if ((24 - (DateTime.Now - UserDetails.quest.created_date.ToLocalTime()).TotalHours) < 0 && UserDetails.quest.claim_trx_id == null && UserDetails.quest.earned_chests > 0)
                        {
                            Logs.LogMessage($"{UserData.Username}: 24h passed since Daily Focus was started. Claiming Rewards...", Logs.LOG_ALERT);
                            if (await focus.ClaimQuestReward(UserDetails.quest, UserData))
                            {
                                //ADD SOCKET CHECK FOR CLAIM TX
                            }
                        }
                    }

                    if (await focus.CheckForNewFocus(UserDetails.quest, UserData))
                    {
                        focusRenewed = false;
                        //ADD SOCKET CHECK FOR NEW FOCUS TX
                    }

                    focusProgress = focus.GetQuestProgress(UserDetails.quest.earned_chests, (int)UserDetails.quest.chest_tier, UserDetails.quest.rshares);

                    if (UserConfig.FocusEnabled)
                        prioritizeFocus = focus.IsFocusPrio(random, focusSplinter);

                    if (UserConfig.FocusEnabled && UserConfig.AvoidFocus && !focusRenewed)
                    {
                        if(UserConfig.FocusBlacklist.Length > 0 && UserConfig.FocusBlacklist.Any(x => x.Contains(focusSplinter)))
                        {
                            Logs.LogMessage($"{UserData.Username}: Daily Focus blacklisted, requesting a new one...", Logs.LOG_ALERT);
                            if (focus.RequestNewFocus(UserDetails.quest, UserData))
                            {
                                Logs.LogMessage($"{UserData.Username}: New Daily Focus received", Logs.LOG_SUCCESS);
                                focusRenewed = true;

                                //ADD CHECK FOR NEW FOCUS TX
                                
                                focusSplinter = focus.GetFocusSplinter(UserDetails.quest.name);
                            }
                            else
                            {
                                Logs.LogMessage($"{UserData.Username}: Error requesting new Daily Focus", Logs.LOG_WARNING);
                            }
                        }
                    }

                    if (UserConfig.FocusEnabled)
                        Logs.LogMessage($"{UserData.Username}: Current Focus: {focusSplinter}", Logs.LOG_ALERT, true);

                    InstanceManager.UsersStatistics[botInstance].Focus = $"{focusSplinter}[{focusProgress}]";
                    InstanceManager.UsersStatistics[botInstance].HoursUntilNextQuest = (24 - (DateTime.Now - UserDetails.quest.created_date.ToLocalTime()).TotalHours).ToString();
                }
                else
                {
                    Logs.LogMessage($"{UserData.Username}: Having issues requesting Daily Focus info", Logs.LOG_WARNING);
                }

                Logs.LogMessage($"{UserData.Username}: Current Energy Capture Rate is {UserBalance.ECR}%", supress: true);

                if (!waitForECR)
                {
                    if ((UserBalance.ECR) < UserConfig.EcrLimit)
                    {
                        Logs.LogMessage($"{UserData.Username}: ECR limit reached, moving to next account [{Math.Round(UserBalance.ECR)}%/{UserConfig.EcrLimit}%] ", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(5);
                        await Task.Delay(1500);
                        if (UserConfig.WaitToRechargeEcr)
                        {
                            waitForECR = true;
                            Logs.LogMessage($"{UserData.Username}: Recharge enabled, user will wait until ECR is {UserConfig.EcrRechargeLimit}%", Logs.LOG_ALERT);
                        }
                        SleepUntil = DateTime.Now.AddMinutes(5);
                        return SleepUntil;
                    }
                }
                else
                {
                    if ((UserBalance.ECR) < UserConfig.EcrRechargeLimit)
                    {
                        Logs.LogMessage($"{UserData.Username}: Recharge enabled, ECR {Math.Round(UserBalance.ECR)}%/{UserConfig.EcrRechargeLimit}%. Moving to next account", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(10);
                        return SleepUntil;
                    }
                    else
                    {
                        Logs.LogMessage($"{UserData.Username}: ECR Restored {Math.Round(UserBalance.ECR)}%/{UserConfig.EcrRechargeLimit}%", Logs.LOG_SUCCESS);
                        waitForECR = false;
                    }
                }

                if (!UserConfig.UsePrivateApi)
                {
                    //Check public api limit
                    if(!await BotAPI.CheckPublicAPILimit())
                    {
                        Logs.LogMessage($"{UserData.Username}: Public API Limit exceeded. Waiting 10 minutes", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(10);
                        return SleepUntil;
                    }
                }

                BattleState.Reset();
                //Starting a new match
                bool canSubmitTeam = true;
                
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string tx = HiveActions.StartNewMatch(UserData); //maybe remve tx

                

                if (tx == "" || !await BattleState.WaitForBattleStarted(30))
                {
                    var outstandingGame = await SplinterlandsAPI.GetOutstandingMatch(UserData.Username);
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
                            canSubmitTeam = false;
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
                matchDetails = null;
                bool surrender = false;
                SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);

                if (canSubmitTeam)
                {
                    if (matchDetails == null)
                    {
                        if (!await BattleState.WaitForBattleStarted(185))
                        {
                            Logs.LogMessage($"{UserData.Username}: Check for Ranked mode Ban. Waiting 30 minutes!", Logs.LOG_WARNING);
                            SleepUntil = DateTime.Now.AddMinutes(30);
                            return SleepUntil;
                        }

                        Logs.LogMessage($"{UserData.Username}: New match found - Manacap: {matchDetails["mana_cap"]}; Ruleset: {matchDetails["ruleset"]}; Inactive splinters: {matchDetails["inactive"]}", Logs.LOG_ALERT);
                    }
                    
                    try
                    {
                        if (UserConfig.UsePrivateApi)
                        {
                            Logs.LogMessage($"{UserData.Username}: Fetching team from private api");
                            try
                            {
                                team = await BotAPI.GetTeamFromAPI(matchDetails, focusSplinter, focusCompleted, UserCards, UserData.Username, UserDetails.league, prioritizeFocus, UserConfig, true);
                            }
                            catch (Exception ex)
                            {
                                Logs.LogMessage($"{UserData.Username}: Error fetching team from private api. Trying on public api", Logs.LOG_ALERT);
                                team = await BotAPI.GetTeamFromAPI(matchDetails, focusSplinter, focusCompleted, UserCards, UserData.Username, UserDetails.league, prioritizeFocus, UserConfig, false);
                            }
                        }
                        else
                        {
                            Logs.LogMessage($"{UserData.Username}: Fetching team from public api");
                            team = await BotAPI.GetTeamFromAPI(matchDetails, focusSplinter, focusCompleted, UserCards, UserData.Username, UserDetails.league, prioritizeFocus, UserConfig, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logs.LogMessage(ex.Message, Logs.LOG_WARNING,true);
                    }

                    if (!team.HasValues)
                    {
                        Logs.LogMessage($"{UserData.Username}: API couldn't find any team - Skipping Account", Logs.LOG_WARNING);
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                        return SleepUntil;
                    }
                    else
                    {
                        Logs.OutputTeam(UserData.Username, team["summonerName"].ToString(), team["card1Name"].ToString(), team["card2Name"].ToString(), team["card3Name"].ToString(), team["card4Name"].ToString(), team["card5Name"].ToString(), team["card6Name"].ToString());
                    }

                    await Task.Delay(new Random().Next(3000, 8000));
                    var submittedTeam = HiveActions.SubmitTeam(tx, matchDetails, team, UserData, UserCards);
                    Logs.LogMessage($"{UserData.Username}: Team submitted. TX:{submittedTeam.tx}", supress: true);
                    if (!await BattleState.WaitForTeamSubmitted(15))
                    {
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                    }

                    while (stopwatch.Elapsed.Seconds < 145 && !surrender)
                    {
                        if (await BattleState.WaitForOpponentTeamSubmitted(5))
                        {
                            break;
                        }
                        
                        if (await BattleState.WaitForResultsReceived() || await BattleState.WaitForBattleCanceled())
                        {
                            surrender = true;
                            break;
                        }
                    }

                    stopwatch.Stop();
                    if (surrender)
                    {
                        Logs.LogMessage($"{UserData.Username}: Enemy probably surrendered", Logs.LOG_WARNING);
                    }
                    else
                    {
                        if (!HiveActions.RevealTeam(tx, matchDetails, team, submittedTeam.secret, UserData, UserCards))
                        {
                            Logs.LogMessage($"{UserData.Username}: Error revealing team. Trying again", Logs.LOG_WARNING);
                            if (!HiveActions.RevealTeam(tx, matchDetails, team, submittedTeam.secret, UserData, UserCards))
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

                if (await BattleState.WaitForResultsReceived(210))
                {
                    InstanceManager.UsersStatistics[botInstance].Balance.ECR = Math.Round(UserBalance.ECR != null ? (double)UserBalance.ECR : 0, 2);
                }
                else { Logs.LogMessage($"{UserData.Username}: Error fetching battle result", Logs.LOG_WARNING); }
                
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{UserData.Username}: {ex}{Environment.NewLine}Skipping Account", Logs.LOG_WARNING);
            }
            finally
            {
                lock (_activeLock)
                {
                    CurrentlyActive = false;
                }
            }
            return SleepUntil;
        }

        internal void UpdateECR(double v)
        {
            throw new NotImplementedException();
        }

        private async Task ShowBattleResult(string tx, bool surrender)
        {
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

            for (int i = 0; i < secondsToWait * 2; i++)
            {
                await Task.Delay(500);
                if (WebSocket.states.ContainsKey(GameState.transaction_complete)
                    && (string)webSocketClient.states[GameState.transaction_complete]["trx_info"]["id"] == tx)
                {
                    if ((bool)webSocketClient.states[GameState.transaction_complete]["trx_info"]["success"])
                    {
                        return true;
                    }
                    else
                    {
                        Logs.LogMessage($"{UserData.Username}: Transaction error: " + tx + " - " + (string)webSocketClient.states[GameState.transaction_complete]["trx_info"]["error"], Logs.LOG_WARNING);
                        return false;
                    }
                }
            }
            Logs.LogMessage($"{UserData.Username}: No response from websocket.", Logs.LOG_WARNING);
            return false;
        }

        private double ComputeECR(List<Balance> balances)
        {
            var values = balances.Where(x => x.token == "ECR").Any() ? balances.Where(x => x.token == "ECR").First() : null;
            if (values != null)
            {
                if (values.balance == 0)
                { return 100; }
                else
                {
                    double rechargeRate = 0.0868;
                    double ecr = values.balance + (new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() - new DateTimeOffset((DateTime)values.last_reward_time).ToUnixTimeMilliseconds()) / 3000 * rechargeRate;
                    return Math.Min(ecr, 10000) / 100;
                }
            }

            return 0;
        }

        private void OutputQuestResults(string response)
        {
            Models.Account.QuestRewardData rewardData = JsonConvert.DeserializeObject<Models.Account.QuestRewardData>(response);
            if (rewardData.trx_info.result.success == true)
            {
                string[] rewards = new string[rewardData.trx_info.result.rewards.Count];
                int i = 0;
                foreach (Models.Account.QuestReward reward in rewardData.trx_info.result.rewards)
                {
                    if (reward.type == "reward_card")
                    {
                        rewards[i] = $"{reward.quantity} x {(reward.card.gold ? "(Gold)" : "")} {SplinterlandsData.splinterlandsCards.Where(x => x.id == reward.card.card_detail_id).First().name}";
                    }
                    else if (reward.type == "potion")
                    {
                        rewards[i] = $"{reward.quantity} x {(reward.potion_type == "gold" ? "Gold" : "Legedary")} Potion";
                    }
                    else if (reward.type == "credits")
                    {
                        rewards[i] = $"{reward.quantity} x Credits";
                    }
                    else if (reward.type == "dec")
                    {
                        rewards[i] = $"{reward.quantity} x DEC";
                    }
                    else if (reward.type == "pack")
                    {
                        rewards[i] = $"{reward.quantity} x Packs";
                    }
                    i++;
                }

                Logs.OutputQuestRewards(rewardData.trx_info.player, rewards);
            }
        }

        #region League
        private async Task AdvanceLeague()
        {
            try
            {
                int highestPossibleLeague = GetMaxLeagueByRankAndPower(UserDetails.rating - UserConfig.LeagueRatingThreshold, UserDetails.collection_power);
                if (highestPossibleLeague > UserDetails.league && highestPossibleLeague <= (UserConfig.MaxLeague == 0 ? 13 : UserConfig.MaxLeague))
                {
                    Logs.LogMessage($"{UserData.Username}: Advancing to higher league!", Logs.LOG_SUCCESS);

                    string tx = HiveActions.AdvanceLeague(UserData);
                    if (await WaitForTransactionSuccess(tx, 45))
                    {
                        Logs.LogMessage($"{UserData.Username}: Advanced league: {tx}");
                    }
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
