namespace SplinterlandsRObot.Player.PlayerFocus
{
    public class FocusResult
    {
        public bool success { get; set; }
        public List<FocusReward> rewards { get; set; }
        public object? potions { get; set; }
        public object? calc { get; set; }
    }
}
