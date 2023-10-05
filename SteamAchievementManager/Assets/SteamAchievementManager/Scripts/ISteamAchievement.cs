namespace SteamAchievement
{
    public interface ISteamAchievement
    {
        public string AchievementKey { get; }
        public string StatsKey {get; }
        public string Progress { get; }
        public double Duration { get; }
        public ApiType ApiType { get; }
    }
}
