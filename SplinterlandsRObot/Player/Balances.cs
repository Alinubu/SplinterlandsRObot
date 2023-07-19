using DocumentFormat.OpenXml.Wordprocessing;
using SplinterlandsRObot.API;

namespace SplinterlandsRObot.Player
{
    public class Balances
    {
        public double Credits { get; set; }
        public double DEC { get; set; }
        public int LegendaryPotions { get; set; }
        public int GoldPotions { get; set; }
        public int QuestPotions { get; set; }
        public int Packs { get; set; }
        public double Voucher { get; set; }
        public double SPS { get; set; }
        public double SPSP { get; set; }
        public double SPSP_OUT { get; set; }
        public double SPSP_IN { get; set; }
        public double ECR { get; set; }

        public void UpdateECR(List<Balance> balances)
        {
            var values = balances.Where(x => x.token == "ECR").Any() ? balances.Where(x => x.token == "ECR").First() : null;
            if (values != null)
            {
                
                if (values.balance == 0)
                { ECR = 50; }
                else
                {
                    double msInOneHour = 1000 * 60 * 60;
                    double hourlyRechargeRate = 1;
                    double regeneratedEnergy = (new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() - new DateTimeOffset((DateTime)values.last_reward_time).ToUnixTimeMilliseconds()) / msInOneHour * hourlyRechargeRate;
                    double ecr = Math.Floor(values.balance + regeneratedEnergy);
                    ECR = Math.Min(ecr, 50);
                }
            }
            else { ECR = 0; }
        }
    }
}
