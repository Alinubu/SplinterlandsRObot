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
using SplinterlandsRObot.Models.WebSocket;

namespace SplinterlandsRObot.Game
{
    public class BotInstance
    {
        #region BotInstance Constructor
        private readonly object _activeLock = new();
        public bool CurrentlyActive { get; set; }
        private int InstanceIndex { get; set; }
        public User UserData { get; set; }
        public Config UserConfig { get; set; }
        public UserDetails UserDetails = new();
        private UserBalance UserBalance = new();
        private CardsCollection UserCards = new();
        private Focus focus = new();
        private Season season = new Season();
        private BattleState BattleState = new();
        private HiveActions HiveActions = new();
        private Splinterlands SplinterlandsAPI = new();
        private Bot BotAPI = new();
        private WebSocket WebSocket { get; set; }
        
        private Random random = new Random();

        private DateTime LastLogin = DateTime.MinValue;
        private DateTime LastAirdrop = DateTime.MinValue;
        private bool waitForECR = false;
        private bool focusCompleted = false;
        public string focusSplinter = "";
        private string focusProgress = "";
        private string lastRatingUpdate = "0";
        private string lastModernRatingUpdate = "0";
        private string lastDecUpdate = "0";
        private string lastRsharesUpdate = "0";
        private bool focusRenewed = false;
        private DateTime SleepUntil = DateTime.MinValue;
        private bool refreshCards = true;
        private bool reloadUser = false;
        private bool prioritizeFocus = false;
        JToken matchDetails = null;

        public BotInstance(User user, int instanceIndex)
        {
            UserData = user;
            InstanceIndex = instanceIndex;
            UserDetails = HiveActions.GetUserDetails(UserData.Username, UserData.Keys.PostingKey);
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
                ECR = 0
            };
            UserBalance.UpdateECR(UserDetails.balances);
            LastLogin = DateTime.Now;
            UserConfig = new Config(UserData.ConfigFile);
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
                Rating = (UserConfig.BattleMode == "modern" ? UserDetails.modern_rating : UserDetails.rating),
                RatingChange = "",
                League = UserConfig.BattleMode == "modern" ? 
                                    SplinterlandsData.splinterlandsSettings.leagues.modern[(int)UserDetails.modern_league].name
                                    : SplinterlandsData.splinterlandsSettings.leagues.wild[UserDetails.league].name,
                CollectionPower = UserDetails.collection_power,
                Quest = "",
                HoursUntilNextQuest = "N/A",
                NextMatchIn = SleepUntil,
                ErrorMessage = ""
            });
            WebSocket = new WebSocket(UserData.Username,UserDetails.token, this);
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
                    SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                    return SleepUntil;
                }

                if (!WebSocket.client.IsStarted)
                {
                    await WebSocket.WebsocketStart();
                    WebSocket.transactions.RemoveAll(transaction => transaction.processed == true);
                }
                
                if (refreshCards)
                {
                    UserCards = await SplinterlandsAPI.GetUserCardsCollection(UserData.Username, UserDetails.token);
                    refreshCards = false;
                }

                if (reloadUser)
                {
                    UserDetails = HiveActions.GetUserDetails(UserData.Username, UserData.Keys.PostingKey);
                    reloadUser = false;
                }

                UserBalance.UpdateECR(UserDetails.balances);

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

                    if (InstanceManager.FocusRentingQueue.ContainsKey(UserData.Username) && !UserConfig.BattleWhileRenting)
                    {
                        {
                            SleepUntil = DateTime.Now.AddMinutes(5);
                            return SleepUntil;
                        }
                    }
                }

                if (!Settings.DO_BATTLE)
                {
                    SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                    return SleepUntil;
                }

                if (UserConfig.LeagueAdvance) await AdvanceLeague();//Add ranked mode calculation

                //UserDetails.current_season_player.earned_chests = season.CalculateEarnedChests((int)UserDetails.current_season_player.chest_tier, (int)UserDetails.current_season_player.rshares);
                //InstanceManager.UsersStatistics[botInstance].Season = season.GetSeasonProgress((int)UserDetails.current_season_player.earned_chests, (int)UserDetails.current_season_player.chest_tier, (int)UserDetails.current_season_player.rshares);

                Logs.LogMessage($"{UserData.Username}: Current Energy Capture Rate is {UserBalance.ECR}%", supress: true);

                if (!waitForECR)
                {
                    if ((UserBalance.ECR) < UserConfig.EcrLimit)
                    {
                        Logs.LogMessage($"{UserData.Username}: ECR limit reached, moving to next account [{Math.Round(UserBalance.ECR)}%/{UserConfig.EcrLimit}%] ", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                        //await Task.Delay(1500); WHYYY?????
                        if (UserConfig.WaitToRechargeEcr)
                        {
                            waitForECR = true;
                            Logs.LogMessage($"{UserData.Username}: Recharge enabled, user will wait until ECR is {UserConfig.EcrRechargeLimit}%", Logs.LOG_ALERT);
                        }
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                        return SleepUntil;
                    }
                }
                else
                {
                    if ((UserBalance.ECR) < UserConfig.EcrRechargeLimit)
                    {
                        Logs.LogMessage($"{UserData.Username}: Recharge enabled, ECR {Math.Round(UserBalance.ECR)}%/{UserConfig.EcrRechargeLimit}%. Moving to next account", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                        return SleepUntil;
                    }
                    else
                    {
                        Logs.LogMessage($"{UserData.Username}: ECR Restored {Math.Round(UserBalance.ECR)}%/{UserConfig.EcrRechargeLimit}%", Logs.LOG_SUCCESS);
                        waitForECR = false;
                    }
                }

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
                            string qtx = await focus.ClaimQuestReward(UserDetails.quest, UserData);
                            if (qtx != null)
                            {
                                JToken response = null;
                                bool success = false;
                                (success, response) = await WaitForTransactionSuccess(qtx, 30);
                                if (success)
                                    OutputQuestResults(response.ToString());
                            }
                        }
                        Logs.LogMessage($"{UserData.Username}: Cannot find Focus name. Maybe old quest active, requesting a new Focus", Logs.LOG_ALERT);
                        string ftx = new HiveActions().StartFocus(UserData);
                        if (ftx != null)
                        {
                            JToken response = null;
                            bool success = false;
                            (success, response) = await WaitForTransactionSuccess(ftx, 30);
                            if (success)
                                UserDetails.quest = JsonConvert.DeserializeObject<Quest>(response["data"]["trx_info"]["result"].ToString());
                        }
                        
                        focusSplinter = focus.GetFocusSplinter(UserDetails.quest.name);
                        if (focusSplinter == "THISWILLBEREMOVEDATSOMEPOINT")
                        {
                            Logs.LogMessage($"{UserData.Username}: Cannot find Focus name. Maybe old quest ongoing, requesting a new Focus a different way", Logs.LOG_ALERT);

                            string nftx = focus.RequestNewFocus(UserData);
                            if (nftx != null)
                            {
                                JToken response = null;
                                bool success = false;
                                (success, response) = await WaitForTransactionSuccess(nftx, 30);
                                if (success)
                                    UserDetails.quest = JsonConvert.DeserializeObject<Quest>(response["data"]["trx_info"]["result"].ToString());
                                focusSplinter = focus.GetFocusSplinter(UserDetails.quest.name);
                            }
                        }
                    }

                    UserDetails.quest.earned_chests = focus.CalculateEarnedChests((int)UserDetails.quest.chest_tier, (int)UserDetails.quest.rshares);
                    focusRenewed = UserDetails.quest.refresh_trx_id != null ? true : false;

                    if (UserConfig.ClaimFocusChests)
                    {
                        if ((24 - (DateTime.Now - UserDetails.quest.created_date.ToLocalTime()).TotalHours) < 0 && UserDetails.quest.claim_trx_id == null && UserDetails.quest.earned_chests > 0)
                        {
                            Logs.LogMessage($"{UserData.Username}: 24h passed since Daily Focus was started. Claiming Rewards...", Logs.LOG_ALERT);
                            string qtx = await focus.ClaimQuestReward(UserDetails.quest, UserData);
                            if (qtx != null)
                            {
                                JToken response = null;
                                bool success = false;
                                (success, response) = await WaitForTransactionSuccess(qtx, 30);
                                if (success)
                                {
                                    UserDetails.quest.claim_trx_id = response["data"]["trx_info"]["id"].ToString();
                                    OutputQuestResults(response["data"].ToString());
                                    if (UserConfig.AutoTransferAfterFocusClaim)
                                        _ = Task.Run(async () => await new TransferAssets().TransferAssetsAsync(UserData).ConfigureAwait(false));
                                }
                                    
                            }
                        }
                    }

                    string cftx = await focus.CheckForNewFocus(UserDetails.quest, UserData);
                    if (cftx != null)
                    {
                        JToken response = null;
                        bool success = false;
                        (success, response) = await WaitForTransactionSuccess(cftx, 30);
                        if (success)
                        {
                            focusRenewed = false;
                            UserDetails.quest = JsonConvert.DeserializeObject<Quest>(response["data"]["trx_info"]["result"].ToString());
                        }
                            
                    }

                    focusProgress = focus.GetQuestProgress(UserDetails.quest.earned_chests, (int)UserDetails.quest.chest_tier, UserDetails.quest.rshares);

                    if (UserConfig.FocusEnabled)
                        prioritizeFocus = focus.IsFocusPrio(random, focusSplinter, UserConfig);

                    if (UserConfig.FocusEnabled && UserConfig.AvoidFocus && !focusRenewed)
                    {
                        if(UserConfig.FocusBlacklist.Length > 0 && UserConfig.FocusBlacklist.Any(x => x.Contains(focusSplinter)))
                        {
                            Logs.LogMessage($"{UserData.Username}: Daily Focus blacklisted, requesting a new one...", Logs.LOG_ALERT);
                            string nftx = focus.RequestNewFocus(UserData);
                            if (nftx != null)
                            {
                                JToken response = null;
                                bool success = false;
                                (success, response) = await WaitForTransactionSuccess(nftx, 30);
                                if (success)
                                {
                                    Logs.LogMessage($"{UserData.Username}: New Daily Focus received", Logs.LOG_SUCCESS);
                                    focusRenewed = true;
                                    UserDetails.quest = JsonConvert.DeserializeObject<Quest>(response["data"]["trx_info"]["result"].ToString());
                                    focusSplinter = focus.GetFocusSplinter(UserDetails.quest.name);
                                }
                                else
                                {
                                    Logs.LogMessage($"{UserData.Username}: Error requesting new Daily Focus", Logs.LOG_WARNING);
                                }
                            }
                        }
                    }

                    if (UserConfig.FocusEnabled)
                        Logs.LogMessage($"{UserData.Username}: Current Focus: {focusSplinter}", Logs.LOG_ALERT, true);

                    InstanceManager.UsersStatistics[botInstance].Quest = $"{focusSplinter}[{focusProgress}]";
                    InstanceManager.UsersStatistics[botInstance].HoursUntilNextQuest = (24 - (DateTime.Now - UserDetails.quest.created_date.ToLocalTime()).TotalHours).ToString();
                    InstanceManager.UsersStatistics[botInstance].Balance = UserBalance;
                    InstanceManager.UsersStatistics[botInstance].CollectionPower = UserDetails.collection_power;
                    InstanceManager.UsersStatistics[botInstance].League = UserConfig.BattleMode == "modern" ?
                                    SplinterlandsData.splinterlandsSettings.leagues.modern[(int)UserDetails.modern_league].name
                                    : SplinterlandsData.splinterlandsSettings.leagues.wild[UserDetails.league].name;
                }
                else
                {
                    Logs.LogMessage($"{UserData.Username}: Having issues requesting Daily Focus info", Logs.LOG_WARNING);
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
                matchDetails = null;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string tx = HiveActions.StartNewMatch(UserData, UserConfig.BattleMode);

                if (tx == "")
                {
                    var sleepTime = 5;
                    Logs.LogMessage($"{UserData.Username}: Error requesting new match(no response from battle api). Waiting {sleepTime} minutes.", Logs.LOG_WARNING);
                    SleepUntil = DateTime.Now.AddMinutes(sleepTime);
                    return SleepUntil;
                }

                if (tx == "error" || !WaitForTransactionSuccess(tx, 30).Result.Item1)
                {
                    var outstandingGame = await SplinterlandsAPI.GetOutstandingMatch(UserData.Username);
                    if (outstandingGame != "null")
                    {
                        tx = Helpers.DoQuickRegex("\"id\":\"(.*?)\"", outstandingGame);

                        if (tx == "")
                        {
                            var sleepTime = 5;
                            Logs.LogMessage($"{UserData.Username}: Error requesting new match(no response from battle api). Waiting {sleepTime} minutes.", Logs.LOG_WARNING);
                            SleepUntil = DateTime.Now.AddMinutes(sleepTime);
                            return SleepUntil;
                        }

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
                    }
                    Logs.LogMessage($"{UserData.Username}: New match found - Manacap: {matchDetails["mana_cap"]}; Ruleset: {matchDetails["ruleset"]}; Inactive splinters: {matchDetails["inactive"]}", Logs.LOG_ALERT);
                    
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

                if (!await BattleState.WaitForResultsReceived(210))
                    Logs.LogMessage($"{UserData.Username}: Error fetching battle result", Logs.LOG_WARNING);
                
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
        #region Update
        internal void UpdateMatchFound(bool s, JToken details)
        {
            matchDetails = details;
            BattleState.BattleStarted = s;
        }
        internal void UpdateOpponentSubmitTeam(bool s)
        {
            BattleState.OpponentTeamSubmitted = s;
        }
        internal void UpdateLeague(int league)
        {
            UserDetails.league = league;
        }

        internal void UpdateMaxLeague(int maxLeague)
        {
            UserDetails.season_max_league = maxLeague;
        }

        internal void UpdateSeasonRewardShares(int rshares)
        {
            if (UserDetails.current_season_player == null)
            {
                reloadUser = true;
            }
            else
            {
                UserDetails.current_season_player.rshares += rshares;
                lastRsharesUpdate = rshares.ToString();
            }
        }

        internal void UpdateRating(int rating)
        {
            lastRatingUpdate = (rating - UserDetails.rating).ToString();
            UserDetails.rating = rating;
        }

        internal void UpdateModernLeague(int league)
        {
            UserDetails.modern_league = league;
        }

        internal void UpdateModernMaxLeague(int maxLeague)
        {
            UserDetails.modern_season_max_league = maxLeague;
        }

        internal void UpdateModernSeasonRewardShares(int rshares)
        {
            if (UserDetails.current_season_player == null)
            {
                reloadUser = true;
            }
            else
            {
                UserDetails.current_modern_season_player.rshares += rshares;
                lastRsharesUpdate = rshares.ToString();
            }
        }

        internal void UpdateModernRating(int rating)
        {
            lastModernRatingUpdate = (rating - UserDetails.modern_rating).ToString();
            UserDetails.modern_rating = rating;
        }

        internal void UpdateCollectionPower(int cp)
        {
            UserDetails.collection_power = cp;
            refreshCards = true;
            Logs.LogMessage($"{UserData.Username}: CP change detected, refreshing cards collection on next loop", Logs.LOG_ALERT);
        }
        internal void UpdateBattleResults(int status, string winner)
        {
            string ratingUpdate = (UserConfig.BattleMode == "modern" ? lastModernRatingUpdate : lastRatingUpdate);
            if (winner == UserDetails.name)
            {
                if (Settings.DO_BATTLE)
                    Logs.LogMessage($"{UserData.Username}: Match Won {(status == 8 ? "(Enemy surrendered) " : "")}[Rating: {(UserConfig.BattleMode == "modern" ? UserDetails.modern_rating : UserDetails.rating)}(+{ratingUpdate}); Reward: { lastDecUpdate } DEC; RShares: {lastRsharesUpdate}]", Logs.LOG_SUCCESS);
                InstanceManager.UsersStatistics[InstanceIndex].Wins++;
                InstanceManager.UsersStatistics[InstanceIndex].MatchRewards = Convert.ToDouble(lastDecUpdate);
                InstanceManager.UsersStatistics[InstanceIndex].TotalRewards = InstanceManager.UsersStatistics[InstanceIndex].TotalRewards + Convert.ToDouble(lastDecUpdate);
                InstanceManager.UsersStatistics[InstanceIndex].RatingChange = "+" + ratingUpdate;
                InstanceManager.UsersStatistics[InstanceIndex].Rating = (UserConfig.BattleMode == "modern" ? UserDetails.modern_rating : UserDetails.rating);
            }
            else if (winner == "DRAW")
            {
                if (Settings.DO_BATTLE)
                    Logs.LogMessage($"{UserData.Username}: Match was a Draw [Rating: {(UserConfig.BattleMode == "modern" ? UserDetails.modern_rating : UserDetails.rating)}]", Logs.LOG_ALERT);
                InstanceManager.UsersStatistics[InstanceIndex].Draws++;
            }
            else
            {
                if (Settings.DO_BATTLE)
                    Logs.LogMessage($"{UserData.Username}: Match Lost [Rating: {(UserConfig.BattleMode == "modern" ? UserDetails.modern_rating : UserDetails.rating)}({ratingUpdate})]", Logs.LOG_WARNING);
                InstanceManager.UsersStatistics[InstanceIndex].Losses++;
                InstanceManager.UsersStatistics[InstanceIndex].RatingChange = ratingUpdate;
                InstanceManager.UsersStatistics[InstanceIndex].Rating = (UserConfig.BattleMode == "modern" ? UserDetails.modern_rating : UserDetails.rating);
            }
            BattleState.ResultsReceived = true;
        }

        internal void UpdateFocusInfo(string id, string player, DateTime created_date, int created_block, string name, int total_items, int completed_items, string? claim_trx_id, DateTime? claim_date, int reward_qty, string? refresh_trx_id, int chest_tier, int rshares)
        {
            UserDetails.quest.id = id;
            UserDetails.quest.player = player;
            UserDetails.quest.created_date = created_date;
            UserDetails.quest.created_block = created_block;
            UserDetails.quest.name = name;
            UserDetails.quest.total_items = total_items;
            UserDetails.quest.completed_items = completed_items;
            if (claim_trx_id != null)
                UserDetails.quest.claim_trx_id = claim_trx_id;
            if (claim_date != null)
                UserDetails.quest.claim_date = claim_date;
            UserDetails.quest.reward_qty = reward_qty;
            if (refresh_trx_id != null)
                UserDetails.quest.refresh_trx_id = refresh_trx_id;
            UserDetails.quest.chest_tier = chest_tier;
            UserDetails.quest.rshares = rshares;
        }

        internal void UpdateStakedSpsBalance(double spsp)
        {
            UserDetails.balances.Where(x=>x.token == "SPSP").First().balance = spsp;

        }

        internal void UpdateCreditsBalance(double credits)
        {
            UserDetails.balances.Where(x => x.token == "CREDITS").First().balance = credits;
        }

        internal void UpdatePacksBalance(double packs)
        {
            UserDetails.balances.Where(x => x.token == "CHAOS").First().balance = packs;
        }

        internal void UpdateGoldPotionsBalance(double gPotions)
        {
            UserDetails.balances.Where(x => x.token == "GOLD").First().balance = gPotions;
        }

        internal void UpdateLegendaryPotionsBalance(double lPotions)
        {
            UserDetails.balances.Where(x => x.token == "LEGENDARY").First().balance = lPotions;
        }

        internal void UpdateSpsBalance(double sps)
        {
            UserDetails.balances.Where(x => x.token == "SPS").First().balance = sps;
        }

        internal void UpdateDecBalance(double dec)
        {
            UserDetails.balances.Where(x => x.token == "DEC").First().balance = dec;
        }

        internal void UpdateLastReward(double lastDec)
        {
            lastDecUpdate = lastDec.ToString();
        }

        internal void UpdateECR(Balance ecr)
        {
            UserDetails.balances.First(x => x.token == "ECR").balance = ecr.balance;
            UserDetails.balances.First(x => x.token == "ECR").last_reward_block = ecr.last_reward_block;
            UserDetails.balances.First(x => x.token == "ECR").last_reward_time = ecr.last_reward_time;
        }
        #endregion
        
        private async Task<(bool, JToken)> WaitForTransactionSuccess(string tx, int secondsToWait)
        {
            if (tx.Length == 0)
            {
                return (false,null);
            }

            for (int i = 0; i < secondsToWait * 2; i++)
            {
                await Task.Delay(500);
                if (WebSocket.transactions.Where(x => x.message["data"]["trx_info"]["id"].ToString() == tx).Any())
                {
                    WebSocketTransactionMessage transaction = WebSocket.transactions.Where(x => x.message["data"]["trx_info"]["id"].ToString() == tx).First();
                    if ((bool)transaction.message["data"]["trx_info"]["success"])
                    {
                        return (true,transaction.message);
                    }
                    else
                    {
                        Logs.LogMessage($"{UserData.Username}: Transaction error: " + tx + " - " + (string)transaction.message["data"]["trx_info"]["error"], Logs.LOG_WARNING);
                        return (false, null);
                    }
                }
            }
            Logs.LogMessage($"{UserData.Username}: No response from websocket.", Logs.LOG_WARNING);
            return (false, null);
        }

        private void OutputQuestResults(string response)
        {
            string responseClean = response.Replace("\"{", "{").Replace("}\"", "}").Replace(@"\", "");
            Models.Account.QuestRewardData rewardData = JsonConvert.DeserializeObject<Models.Account.QuestRewardData>(responseClean);
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
                int highestPossibleLeague = UserConfig.BattleMode == "modern"?
                    GetMaxModernLeagueByRankAndPower((int)UserDetails.modern_rating - UserConfig.LeagueRatingThreshold, UserDetails.collection_power)
                    : GetMaxLeagueByRankAndPower(UserDetails.rating - UserConfig.LeagueRatingThreshold, UserDetails.collection_power);
                if (highestPossibleLeague > (UserConfig.BattleMode == "modern" ? UserDetails.modern_league : UserDetails.league) && highestPossibleLeague <= (UserConfig.MaxLeague == 0 ? 13 : UserConfig.MaxLeague))
                {
                    Logs.LogMessage($"{UserData.Username}: Advancing to higher league!", Logs.LOG_SUCCESS);

                    string tx = HiveActions.AdvanceLeague(UserData, UserConfig.BattleMode);
                    JToken response = null;
                    bool success = false;
                    (success, response) = await WaitForTransactionSuccess(tx, 45);
                    if (success)
                    {
                        JToken leagueData = JToken.Parse(response["data"]["trx_info"]["result"].ToString());
                        if (leagueData.ToString().Contains("modern_league"))
                            UpdateModernLeague((int)leagueData["modern_league"]);
                        else
                            UpdateLeague((int)leagueData["league"]);

                        if (leagueData.ToString().Contains("modern_season_max_league"))
                            UpdateModernMaxLeague((int)leagueData["modern_season_max_league"]);
                        else
                            UpdateMaxLeague((int)leagueData["season_max_league"]);

                        if (leagueData.ToString().Contains("modern_rating"))
                            UpdateModernRating((int)leagueData["modern_rating"]);
                        else
                            UpdateRating((int)leagueData["rating"]);
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
        private int GetMaxModernLeagueByRankAndPower(int rating, int power)
        {
            //champion
            if ((rating is >= 3700) && (power is >= 250000))
            {
                return 13;
            }
            //diamong
            if ((rating is >= 2800) && (power is >= 125000))
            {
                return 10;
            }
            //gold
            if ((rating is >= 1900) && (power is >= 50000))
            {
                return 7;
            }
            //silver
            if ((rating is >= 1000) && (power is >= 7500))
            {
                return 4;
            }

            return 0;
        }
        #endregion
    }
}
