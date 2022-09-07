using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SplinterlandsRObot.Models.Bot
{
    public class BattleState
    {
        public bool BattleStarted = false;
        public bool TeamSubmitted = false;
        public bool OpponentTeamSubmitted = false;
        public bool TeamRevealed = false;
        public bool BattleCanceled = false;
        public bool ResultsReceived = false;

        public void Reset()
        {
            BattleStarted = false;
            TeamSubmitted = false;
            OpponentTeamSubmitted = false;
            TeamRevealed = false;
            BattleCanceled = false;
            ResultsReceived = false;
        }

        public async Task<bool> WaitForBattleStarted(int seconds = 1)
        {
            for (int i = 0; i < seconds; i++)
            {
                if (!BattleStarted)
                {
                    await Task.Delay(1000);
                }
                else return BattleStarted;
            }
            return false;
        }
        public async Task<bool> WaitForTeamSubmitted(int seconds = 1)
        {
            for (int i = 0; i < seconds; i++)
            {
                if (!TeamSubmitted)
                {
                    await Task.Delay(1000);
                }
                else return TeamSubmitted;
            }
            return false;
        }
        public async Task<bool> WaitForOpponentTeamSubmitted(int seconds = 1)
        {
            for (int i = 0; i < seconds; i++)
            {
                if (!OpponentTeamSubmitted)
                {
                    await Task.Delay(1000);
                }
                else return OpponentTeamSubmitted;
            }
            return false;
        }
        public async Task<bool> WaitForTeamRevealed(int seconds = 1)
        {
            for (int i = 0; i < seconds; i++)
            {
                if (!TeamRevealed)
                {
                    await Task.Delay(1000);
                }
                else return TeamRevealed;
            }
            return false;
        }
        public async Task<bool> WaitForBattleCanceled(int seconds = 1)
        {
            for (int i = 0; i < seconds; i++)
            {
                if (!BattleCanceled)
                {
                    await Task.Delay(1000);
                }
                else return BattleCanceled;
            }
            return false;
        }
        public async Task<bool> WaitForResultsReceived(int seconds = 1)
        {
            for (int i = 0; i < seconds; i++)
            {
                if (!ResultsReceived)
                {
                    await Task.Delay(1000);
                }
                else return ResultsReceived;
            }
            return false;
        }
    }
}
