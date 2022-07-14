namespace SplinterlandsRObot.Models.Account
{
    public class UserBalance
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
        public double ECR { get; set; }

        public void UpdateECR(List<Balance> balances)
        {
            var values = balances.Where(x => x.token == "ECR").Any() ? balances.Where(x => x.token == "ECR").First() : null;
            if (values != null)
            {
                if (values.balance == 0)
                { ECR = 100; }
                else
                {
                    double rechargeRate = 0.0868;
                    double ecr = values.balance + (new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() - new DateTimeOffset((DateTime)values.last_reward_time).ToUnixTimeMilliseconds()) / 3000 * rechargeRate;
                    ECR = Math.Min(ecr, 10000) / 100;
                }
            }
            else { ECR = 0; }
        }
    }
}
