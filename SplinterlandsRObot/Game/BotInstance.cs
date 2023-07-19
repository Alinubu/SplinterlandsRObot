using System.Diagnostics;
using Newtonsoft.Json.Linq;
using SplinterlandsRObot.Player;
using SplinterlandsRObot.Hive;
using SplinterlandsRObot.API;
using SplinterlandsRObot.Extensions;
using SplinterlandsRObot.Global;
using Newtonsoft.Json;
using SplinterlandsRObot.Models.Bot;
using Websocket.Client;
using Websocket.Client.Models;
using SplinterlandsRObot.Cards;
using SplinterlandsRObot.Player.PlayerFocus;
using SplinterlandsRObot.Player.PlayerSeason;

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
        public Balances UserBalance = new();
        public CardsCollection UserCards = new();
        private BattleState BattleState = new();
        private HiveService HiveService = new();
        private BattleService BattleService = new();
        private Splinterlands SplinterlandsAPI = new();
        private Bot BotAPI = new();
        private WebsocketClient _webSocket { get; set; }
        public List<JToken> _transactions = new();
        private Random random = new Random();
        private bool waitForECR = false;
        private bool focusCompleted = false;
        public string focusSplinter = "";
        private string focusProgress = "";
        private string lastRatingUpdate = "0";
        private string lastModernRatingUpdate = "0";
        private string lastDecUpdate = "0";
        private string lastRsharesUpdate = "0";
        private DateTime SleepUntil = DateTime.MinValue;
        private bool prioritizeFocus = false;
        JToken matchDetails = null;
        private DateTime LastAirdrop = DateTime.MinValue;
        private DateTime LastSPSclaim = DateTime.MinValue;
        private DateTime NextSPSUnstake = DateTime.MinValue;

        public BotInstance(User user, int instanceIndex)
        {
            UserData = user;
            InstanceIndex = instanceIndex;
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
                Rating = 0,
                RatingChange = "",
                League = "",
                CollectionPower = 0,
                Quest = "",
                HoursUntilNextQuest = "N/A",
                NextMatchIn = SleepUntil,
                ErrorMessage = ""
            });
            _webSocket = new WebsocketClient(new Uri(Constants.SPLINTERLANDS_WEBSOCKET_URL));
            _webSocket.ReconnectTimeout = null;
            _webSocket.MessageReceived.Subscribe(OnMessageReceived);
            _webSocket.ReconnectionHappened.Subscribe(OnReconnectionHappened);
            _webSocket.DisconnectionHappened.Subscribe(OnDisconnectionHappened);
            UserConfig = new Config(UserData.ConfigFile);
            InstanceManager._configs.Subscribe(OnConfigChanged);
        }

        private void OnConfigChanged(string _config)
        {
            if (_config == UserData.ConfigFile)
            {
                UserConfig = new Config(UserData.ConfigFile);
            }
        }
        #endregion

        public async Task<DateTime> Start(int botInstance)
        {
            lock (_activeLock)
            {
                if (CurrentlyActive)
                {
                    Logs.LogMessage($"{UserData.Username}: Account already active", Logs.LOG_ALERT, true);
                    return DateTime.Now.AddSeconds(30);
                }
                CurrentlyActive = true;
            }

            try
            {
                if (SleepUntil > DateTime.Now)
                {
                    Logs.LogMessage($"{UserData.Username}: Player benched until {SleepUntil}",supress: true);
                    return SleepUntil;
                }

                Logs.LogMessage($"{UserData.Username}: Updating player data.", Logs.LOG_ALERT, supress: true);
                if (!await UpdatePlayer())
                    return SleepUntil;

                if (InstanceManager.RentingQueue.ContainsKey(UserData.Username) && !UserConfig.BattleWhileRenting)
                {
                    SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                    return SleepUntil;
                }

                if (InstanceManager.isDecDistributorRunning)
                {
                    if (!InstanceManager.DecTransferQueue.ContainsKey(UserData.Username) && UserConfig.RequestDecFromMain)
                    {
                        if (UserBalance.DEC < UserConfig.RequestWhenDecBelow)
                            InstanceManager.DecTransferQueue.Add(UserData.Username, this);
                    }
                }

                if (!_webSocket.IsStarted)
                {
                    await WebsocketStart();
                    _transactions.Clear();
                }

                UserBalance.UpdateECR(UserDetails.balances);

                if (UserConfig.ClaimSPS && (DateTime.Now - LastAirdrop).TotalHours > UserConfig.CheckForAirdropEvery)
                    await new CollectSPS().ClaimSPS(UserData, UserDetails.token);

                if (UserConfig.ClaimSPSRewards && (DateTime.Now - LastSPSclaim).TotalHours > UserConfig.ClaimSPSRewardsEvery)
                    LastSPSclaim = await new CollectSPS().ClaimSPSRewards(UserData, UserDetails.token); 

                if (UserConfig.UnstakeSPS && DateTime.Now > NextSPSUnstake)
                    NextSPSUnstake = await new CollectSPS().SPSUnstake(UserData, UserDetails.token, UserConfig.MinimumSPSUnstakeAmount, UserConfig.UnstakeWeekly, UserBalance.SPSP - UserBalance.SPSP_OUT);

                if (UserConfig.AutoClaimSeasonRewards)
                {
                    if (UserDetails.season_reward is not null)
                    {
                        if (UserDetails.season_reward.reward_packs > 0)
                        {
                            if (UserDetails.season_reward.season is not null)
                            {
                                await HiveService.ClaimSeasonRewards(UserData, UserDetails.season_reward.season, UserConfig);
                            }
                        }
                    }
                }

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

                if (UserConfig.LeagueAdvance) await AdvanceLeague();

                InstanceManager.UsersStatistics[botInstance].Season = new SeasonProgress().SeasonChestsProgress(UserDetails.current_season_player, UserDetails.current_modern_season_player);

                Logs.LogMessage($"{UserData.Username}: Current Energy Capture Rate is {UserBalance.ECR}%", supress: true);

                if (UserConfig.FocusEnabled)
                {
                    Logs.LogMessage($"{UserData.Username}: Daily Focus enabled", Logs.LOG_SUCCESS, true);
                    if (UserDetails.quest is not null)
                    {
                        focusSplinter = UserDetails.quest.GetFocusSplinter();
                        UserDetails.quest.CalculateEarnedChests();

                        if (UserConfig.ClaimFocusChests)
                        {
                            if (UserDetails.quest.CanFocusBeClaimed())
                            {
                                Logs.LogMessage($"{UserData.Username}: Focus has ended. Claiming Rewards...", Logs.LOG_ALERT);
                                string focusClaimTx = await UserDetails.quest.ClaimFocusRewards(UserData);
                                if (focusClaimTx != null)
                                {
                                    JToken response = null;
                                    bool success = false;
                                    (success, response) = await WaitForTransactionSuccess(focusClaimTx, 60);
                                    if (success)
                                    {
                                        UserDetails.quest.claim_trx_id = response["data"]["trx_info"]["id"].ToString();
                                        OutputQuestResults(response["data"].ToString());
                                        if (UserConfig.AutoTransferAfterFocusClaim)
                                            _ = Task.Run(async () => await new TransferAssets().TransferAssetsAsync(UserData).ConfigureAwait(false));
                                    }
                                    else
                                    {
                                        Logs.LogMessage($"{UserData.Username}: Unable to claim Focus rewards, skipping account", Logs.LOG_WARNING);
                                        SleepUntil = DateTime.Now.AddMinutes(5);
                                        return SleepUntil;
                                    }
                                }
                            }
                        }

                        if (UserDetails.collection_power >= UserConfig.FocusStartMinimumCP)
                        {
                            string cftx = UserDetails.quest.CheckAndStartNewFocus(UserData);
                            if (cftx != null)
                            {
                                JToken response = null;
                                bool success = false;
                                (success, response) = await WaitForTransactionSuccess(cftx, 60);
                                if (success)
                                {
                                    UserDetails.quest = JsonConvert.DeserializeObject<Focus>(response["data"]["trx_info"]["result"].ToString());
                                    focusSplinter = UserDetails.quest.GetFocusSplinter();
                                    Logs.LogMessage($"{UserData.Username}: New Focus started [{focusSplinter}]", Logs.LOG_SUCCESS);
                                }

                            }

                            if (UserConfig.AvoidFocus && !UserDetails.quest.IsFocusRenewed())
                            {
                                if (UserConfig.FocusBlacklist.Length > 0 && UserConfig.FocusBlacklist.Any(x => x.Contains(focusSplinter)))
                                {
                                    Logs.LogMessage($"{UserData.Username}: Daily Focus [{focusSplinter}] blacklisted, requesting a new one...", Logs.LOG_ALERT);
                                    string nftx = UserDetails.quest.RequestNewFocus(UserData);
                                    if (nftx != null)
                                    {
                                        JToken response = null;
                                        bool success = false;
                                        (success, response) = await WaitForTransactionSuccess(nftx, 60);
                                        if (success)
                                        {
                                            UserDetails.quest = JsonConvert.DeserializeObject<Focus>(response["data"]["trx_info"]["result"].ToString());
                                            focusSplinter = UserDetails.quest.GetFocusSplinter();
                                            Logs.LogMessage($"{UserData.Username}: New Daily Focus received [{focusSplinter}]", Logs.LOG_SUCCESS);
                                        }
                                        else
                                        {
                                            Logs.LogMessage($"{UserData.Username}: Error requesting new Daily Focus", Logs.LOG_WARNING);
                                        }
                                    }
                                }
                            }
                        }
                        else { Logs.LogMessage($"{UserData.Username}: Minimum CP for starting or requesting a new Focus is not met", Logs.LOG_ALERT); }

                        focusProgress = UserDetails.quest.GetFocusProgress();

                        prioritizeFocus = UserConfig.BattleMode.ToLower() == "modern" ?
                                            (UserDetails.modern_rating < UserConfig.FocusMinimumRating ? false : UserDetails.quest.IsFocusPrio(random, UserConfig))
                                            : (UserDetails.rating < UserConfig.FocusMinimumRating ? false : UserDetails.quest.IsFocusPrio(random, UserConfig));


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
                        Logs.LogMessage($"{UserData.Username}: Having issues reading Daily Focus info", Logs.LOG_WARNING);
                    }
                }
                
                if (!waitForECR)
                {
                    if (UserBalance.ECR < UserConfig.EcrLimit)
                    {
                        Logs.LogMessage($"{UserData.Username}: ECR limit reached, moving to next account [{UserBalance.ECR}/{UserConfig.EcrLimit}] ", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                        await Task.Delay(1500);
                        if (UserConfig.WaitToRechargeEcr)
                        {
                            waitForECR = true;
                            Logs.LogMessage($"{UserData.Username}: Recharge enabled, user will wait until ECR is {UserConfig.EcrRechargeLimit}", Logs.LOG_ALERT);
                        }
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                        return SleepUntil;
                    }
                }
                else
                {
                    if (UserBalance.ECR < UserConfig.EcrRechargeLimit)
                    {
                        Logs.LogMessage($"{UserData.Username}: Recharge enabled, ECR {UserBalance.ECR}/{UserConfig.EcrRechargeLimit}. Moving to next account", Logs.LOG_ALERT);
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                        return SleepUntil;
                    }
                    else
                    {
                        Logs.LogMessage($"{UserData.Username}: ECR Restored {UserBalance.ECR}/{UserConfig.EcrRechargeLimit}", Logs.LOG_SUCCESS);
                        waitForECR = false;
                    }
                }

                await UpdatePlayerCards();

                if (!Settings.DO_BATTLE)
                {
                    SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                    return SleepUntil;
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
                bool canSubmitTeam = true;
                matchDetails = null;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string tx = await BattleService.StartBattle(UserData, UserConfig.BattleMode);

                if (tx == "")
                {
                    var sleepTime = 5;
                    Logs.LogMessage($"{UserData.Username}: Error requesting new match (no response from battle api). Waiting {sleepTime} minutes.", Logs.LOG_WARNING);
                    SleepUntil = DateTime.Now.AddMinutes(sleepTime);
                    return SleepUntil;
                }

                if (tx == "error" || !WaitForTransactionSuccess(tx, 300).Result.Item1)
                {
                    var outstandingGame = await SplinterlandsAPI.GetOutstandingMatch(UserData.Username, UserDetails.token);
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
                                team = await BotAPI.GetTeamFromAPI(matchDetails, focusSplinter, focusCompleted, UserCards, UserData.Username, (UserConfig.BattleMode == "modern" ? (int)UserDetails.modern_league : UserDetails.league), prioritizeFocus, UserConfig, true);
                            }
                            catch (Exception ex)
                            {
                                Logs.LogMessage($"{UserData.Username}: Error fetching team from private api. Trying on public api", Logs.LOG_ALERT);
                                team = await BotAPI.GetTeamFromAPI(matchDetails, focusSplinter, focusCompleted, UserCards, UserData.Username, (UserConfig.BattleMode == "modern" ? (int)UserDetails.modern_league : UserDetails.league), prioritizeFocus, UserConfig, false);
                            }
                        }
                        else
                        {
                            Logs.LogMessage($"{UserData.Username}: Fetching team from public api");
                            team = await BotAPI.GetTeamFromAPI(matchDetails, focusSplinter, focusCompleted, UserCards, UserData.Username, (UserConfig.BattleMode == "modern" ? (int)UserDetails.modern_league : UserDetails.league), prioritizeFocus, UserConfig, false);
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
                    var submittedTeam = await BattleService.SubmitTeam(tx, matchDetails, team, UserData, UserCards);
                    Logs.LogMessage($"{UserData.Username}: Team submitted. TX:{submittedTeam.tx}", supress: true);
                    if (!await BattleState.WaitForTeamSubmitted(15))
                    {
                        SleepUntil = DateTime.Now.AddMinutes(UserConfig.SleepBetweenBattles);
                    }

                    while (stopwatch.Elapsed.TotalSeconds < 145 && !surrender)
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
                        if (!await BattleService.RevealTeam(tx, matchDetails, team, submittedTeam.secret, UserData, UserCards))
                        {
                            Logs.LogMessage($"{UserData.Username}: Error revealing team. Trying again", Logs.LOG_WARNING);
                            if (!await BattleService.RevealTeam(tx, matchDetails, team, submittedTeam.secret, UserData, UserCards))
                                Logs.LogMessage($"{UserData.Username}: Error revealing team on second attempt", Logs.LOG_WARNING);
                        }
                            
                    }
                }
                else
                {
                    if (!await BattleService.SurrenderBattle(tx, UserData))
                        Logs.LogMessage($"{UserData.Username}: Error surrendering match.", Logs.LOG_WARNING);
                }

                if (!await BattleState.WaitForResultsReceived(300))
                    Logs.LogMessage($"{UserData.Username}: Error fetching battle result", Logs.LOG_WARNING);
                
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{UserData.Username}: {ex}{Environment.NewLine}Skipping Account", Logs.LOG_WARNING);
            }
            finally
            {
                await WebsocketStop("UserBenched");
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

        internal void UpdateRating(int rating)
        {
            lastRatingUpdate = (rating - UserDetails.rating).ToString();
            UserDetails.rating = rating;
        }
        internal void UpdateSeasonRewardShares(int rshares)
        {
            lastRsharesUpdate = rshares.ToString();
        }
        internal void UpdateModernLeague(int league)
        {
            UserDetails.modern_league = league;
        }

        internal void UpdateModernMaxLeague(int maxLeague)
        {
            UserDetails.modern_season_max_league = maxLeague;
        }

        internal void UpdateModernRating(int rating)
        {
            lastModernRatingUpdate = (rating - UserDetails.modern_rating).ToString();
            UserDetails.modern_rating = rating;
        }
        internal void UpdateModernSeasonRewardShares(int rshares)
        {
            lastRsharesUpdate = rshares.ToString();
        }
        internal void UpdateBattleResults(int status, string winner)
        {
            string ratingUpdate = (UserConfig.BattleMode == "modern" ? lastModernRatingUpdate : lastRatingUpdate);
            if (winner == UserDetails.name)
            {
                if (Settings.DO_BATTLE)
                    Logs.LogMessage($"{UserData.Username}: Match Won {(status == 8 ? "(Enemy surrendered) " : "")}[Rating: {(UserConfig.BattleMode == "modern" ? UserDetails.modern_rating : UserDetails.rating)}(+{ratingUpdate}); RShares: {lastRsharesUpdate}]", Logs.LOG_SUCCESS); /*Reward: { lastDecUpdate } DEC;*/
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

        public async Task<bool> UpdatePlayer(int wait = 0)
        {
            try
            {
                Thread.Sleep((wait == 0 ? random.Next(0, 700) : wait));
                var bid = "bid_" + Helpers.RandomString(20);
                var sid = "sid_" + Helpers.RandomString(20);
                var ts = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
                string signature = HiveService.SignTransaction(UserData.Username, UserData.Keys.PostingKey, ts);

                if (UserData.Keys.JwtToken is null || UserData.Keys.JwtExpire is null)
                {
                    UserDetails = await SplinterlandsAPI.LoginAccount(UserData.Username, bid, sid, signature, ts);
                }
                else if (UserData.Keys.JwtExpire < DateTime.Now)
                {
                    UserDetails = await SplinterlandsAPI.LoginAccount(UserData.Username, bid, sid, signature, ts);
                }

                UserData.Keys.JwtToken = UserDetails.jwt_token;
                UserData.Keys.JwtExpire = UserDetails.jwt_expiration_dt;

                UserDetails = await SplinterlandsAPI.UpdateAccount(UserData.Username, bid, sid, signature, ts, UserData.Keys.JwtToken);
                
                UserBalance = new Balances()
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
                    SPSP_IN = UserDetails.balances.Where(x => x.token == "SPSP-IN").Any() ? UserDetails.balances.Where(x => x.token == "SPSP-IN").First().balance: 0,
                    SPSP_OUT = UserDetails.balances.Where(x => x.token == "SPSP-OUT").Any() ? UserDetails.balances.Where(x => x.token == "SPSP-OUT").First().balance : 0,
                    ECR = 0
                };
                
                UserBalance.UpdateECR(UserDetails.balances);
                InstanceManager.UsersStatistics[InstanceIndex].Rating = (UserConfig.BattleMode == "modern" ? UserDetails.modern_rating : UserDetails.rating);
                InstanceManager.UsersStatistics[InstanceIndex].League = UserConfig.BattleMode == "modern" ?
                                    SplinterlandsData.splinterlandsSettings.leagues.modern[(int)UserDetails.modern_league].name
                                    : SplinterlandsData.splinterlandsSettings.leagues.wild[UserDetails.league].name;
                InstanceManager.UsersStatistics[InstanceIndex].CollectionPower = UserDetails.collection_power;
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{UserData.Username}: {ex.Message}. Skipping account for 5 minutes", Logs.LOG_WARNING);
                SleepUntil = DateTime.Now.AddMinutes(5);
                return false;
            }
            return true;
        }

        internal async Task UpdatePlayerCards()
        {
            try
            {
                UserCards = await SplinterlandsAPI.GetUserCardsCollection(UserData.Username, UserDetails.token);
                await Task.Delay(random.Next(0, 700));
            }
            catch (Exception ex)
            {
                Logs.LogMessage($"{UserData.Username}: {ex.Message}. Skipping account for 5 minutes", Logs.LOG_WARNING);
                SleepUntil = DateTime.Now.AddMinutes(5);
            }
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
                if (_transactions.Where(x => x["data"]["trx_info"]["id"].ToString() == tx).Any())
                {
                    JToken transaction = _transactions.Where(x => x["data"]["trx_info"]["id"].ToString() == tx).First();
                    if ((bool)transaction["data"]["trx_info"]["success"])
                    {
                        return (true,transaction);
                    }
                    else
                    {
                        Logs.LogMessage($"{UserData.Username}: Transaction error: " + tx + " - " + (string)transaction["data"]["trx_info"]["error"], Logs.LOG_WARNING);
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
            FocusRewardData rewardData = JsonConvert.DeserializeObject<FocusRewardData>(responseClean);
            if (rewardData.trx_info.result.success == true)
            {
                string[] rewards = new string[rewardData.trx_info.result.rewards.Count];
                int i = 0;
                foreach (FocusReward reward in rewardData.trx_info.result.rewards)
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
                    else if (reward.type == "merits")
                    {
                        rewards[i] = $"{reward.quantity} x Merits";
                    }
                    else if (reward.type == "sps")
                    {
                        rewards[i] = $"{reward.quantity} x SPS";
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
                int highestPossibleLeague = GetMaxLeagueByRank(UserDetails.rating - UserConfig.LeagueRatingThreshold);
                if (highestPossibleLeague > (UserConfig.BattleMode == "modern" ? UserDetails.modern_league : UserDetails.league) && highestPossibleLeague <= (UserConfig.MaxLeague == 0 ? 13 : UserConfig.MaxLeague))
                {
                    Logs.LogMessage($"{UserData.Username}: Advancing to higher league!", Logs.LOG_SUCCESS);

                    string tx = HiveService.AdvanceLeague(UserData, UserConfig.BattleMode);
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
        private int GetMaxLeagueByRank(int rating)
        {
            //champion
            if (rating is >= 3700)
            {
                return 13;
            }
            //diamong
            if (rating is >= 2800)
            {
                return 10;
            }
            //gold
            if (rating is >= 1900)
            {
                return 7;
            }
            //silver
            if (rating is >= 1000)
            {
                return 4;
            }

            return 0;
        }
        
        #endregion

        #region WebSocket
        public async Task WebsocketStart()
        {
            await _webSocket.Start();
            WebsocketAuthenticate();
            _ = WebsocketPing().ConfigureAwait(false);
        }
        private void WebsocketAuthenticate()
        {
            string sessionID = Helpers.RandomString(10);
            string message = "{\"type\":\"auth\",\"player\":\"" + UserData.Username + "\",\"access_token\":\"" + UserDetails.token + "\",\"session_id\":\"" + sessionID + "\"}";
            WebsocketSendMessage(message);
        }
        public void WebsocketSendMessage(string message)
        {
            _webSocket.Send(message);
        }
        public async Task WebsocketPing()
        {
            while (_webSocket.IsStarted)
            {
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Delay(20 * 1000);
                        if (!_webSocket.IsStarted)
                        {
                            return;
                        }
                    }

                    _webSocket.Send("{\"type\":\"ping\"}");
                    Logs.LogMessage($"{UserData.Username}: ping", supress: true);
                }
                catch (Exception ex)
                {
                    Logs.LogMessage($"{UserData.Username}: Error pinging WebSocket { ex }", supress: true);
                    await WebsocketStop("Ping Error");
                }
            }
        }
        public async Task WebsocketStop(string stopDescription)
        {
            await _webSocket.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, stopDescription);
        }
        public void WebsocketDispose()
        {
            _webSocket.Dispose();
        }
        private void OnMessageReceived(ResponseMessage message)
        {
            if (message.MessageType != System.Net.WebSockets.WebSocketMessageType.Text
                || !message.Text.Contains("\"id\""))
            {
                return;
            }
            JToken json = JToken.Parse(message.Text);

            Logs.LogMessage($"{UserData.Username}: WebMessage received: {json.ToString()}", Logs.LOG_INFO, supress: true);

            string messageType = json["id"].ToString();

            if (messageType == "transaction_complete")
            {
                _transactions.Add(json);
            }
            else if (messageType == "match_found")
            {
                UpdateMatchFound(true, json["data"]);
            }
            else if (messageType == "opponent_submit_team")
            {
                UpdateOpponentSubmitTeam(true);
            }
            else if (messageType == "rating_update")
            {
                if (json["data"].ToString().Contains("modern"))
                {
                    if (json["data"]["modern"].ToString().Contains("new_rating"))
                    {
                        UpdateModernRating((int)json["data"]["modern"]["new_rating"]);
                    }
                    if (json["data"]["modern"].ToString().Contains("new_league"))
                    {
                        UpdateModernLeague((int)json["data"]["modern"]["new_league"]);
                    }
                    if (json["data"]["modern"].ToString().Contains("new_max_league"))
                    {
                        UpdateModernMaxLeague((int)json["data"]["modern"]["new_max_league"]);
                    }
                    if (json["data"]["modern"].ToString().Contains("additional_season_rshares"))
                    {
                        UpdateModernSeasonRewardShares((int)json["data"]["modern"]["additional_season_rshares"]);
                    }
                }

                if (json["data"].ToString().Contains("wild"))
                {
                    if (json["data"]["wild"].ToString().Contains("new_rating"))
                    {
                        UpdateRating((int)json["data"]["wild"]["new_rating"]);
                    }
                    if (json["data"]["wild"].ToString().Contains("new_league"))
                    {
                        UpdateLeague((int)json["data"]["wild"]["new_league"]);
                    }
                    if (json["data"]["wild"].ToString().Contains("new_max_league"))
                    {
                        UpdateMaxLeague((int)json["data"]["wild"]["new_max_league"]);
                    }
                    if (json["data"]["wild"].ToString().Contains("additional_season_rshares"))
                    {
                        UpdateSeasonRewardShares((int)json["data"]["wild"]["additional_season_rshares"]);
                    }
                }
            }
            else if (messageType == "ecr_update")
            {
                if (json["data"].ToString().Contains("capture_rate"))
                {

                    UpdateECR(
                        new Balance()
                        {
                            balance = (double)json["data"]["capture_rate"],
                            token = "ECR",
                            last_reward_block = (int)json["data"]["last_reward_block"],
                            last_reward_time = (DateTime)json["data"]["last_reward_time"]
                        });
                }
            }
            else if (messageType == "balance_update")
            {
                if (json["data"]["token"].ToString() == "DEC")
                    if (json["data"]["type"].ToString() == "dec_reward")
                        UpdateLastReward((double)json["data"]["amount"]);
            }
            else if (messageType == "quest_progress")
            {
                UpdateFocusInfo(
                    (string)json["data"]["id"],
                    (string)json["data"]["player"],
                    (DateTime)json["data"]["created_date"],
                    (int)json["data"]["created_block"],
                    (string)json["data"]["name"],
                    (int)json["data"]["total_items"],
                    (int)json["data"]["completed_items"],
                    (string?)json["data"]["claim_trx_id"],
                    (DateTime?)json["data"]["claim_date"],
                    (int)json["data"]["reward_qty"],
                    (string?)json["data"]["refresh_trx_id"],
                    (int)json["data"]["chest_tier"],
                    (int)json["data"]["rshares"]
                    );
            }
            else if (messageType == "battle_result")
            {
                UpdateBattleResults((int)json["data"]["status"], json["data"]["winner"].ToString());
            }
            else if (messageType == "received_gifts")
            {
                //ToDo
            }
            else
            {
                Logs.LogMessage($"{UserData.Username}: UNKNOWN Message received: {message.Text}", Logs.LOG_ALERT, true);
            }
        }
        private void OnReconnectionHappened(ReconnectionInfo info)
        {
            Logs.LogMessage($"{UserData.Username}: Reconnection happened, type: {info.Type}", supress: true);
        }
        private void OnDisconnectionHappened(DisconnectionInfo disconnectionInfo)
        {
            if (disconnectionInfo.CloseStatusDescription != "UserBenched")
            {
                Logs.LogMessage($"{UserData.Username}: WebSocket disconnected: {disconnectionInfo.CloseStatusDescription}", Logs.LOG_WARNING, true);
                WebsocketStart().ConfigureAwait(true);
            }
        }
        #endregion
    }
}
