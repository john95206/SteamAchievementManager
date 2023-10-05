namespace SteamAchievement.Example
{
	public class SteamAchievement : ISteamAchievement
	{
		public SteamAchievement(AchievementKeyType key, ApiType apiType, string progress = "", double duration = double.MinValue)
		{
			_achievementKey = key;
			_progress = progress;
			_apiType = apiType;
			_duration = duration;

			SetStatsKeyIfNeed();
		}

		public string AchievementKey => _achievementKey.ToString();
		private AchievementKeyType _achievementKey;
		public string StatsKey => _statsKey;
		private string _statsKey;
		public string Progress => _progress;
		private string _progress;
		public double Duration => _duration;
		private double _duration;
		private ApiType _apiType;
		public ApiType ApiType => _apiType;

		protected void SetStatsKeyIfNeed()
		{
			switch (_achievementKey)
			{
				case AchievementKeyType.ACH_WIN_ONE_GAME:
				case AchievementKeyType.ACH_WIN_100_GAMES:
					_statsKey = SteamStatsKey.NUM_WINS;
					break;
				case AchievementKeyType.ACH_TRAVEL_FAR_ACCUM:
					_statsKey = SteamStatsKey.FEET_TRAVELED;
					break;
				case AchievementKeyType.ACH_TRAVEL_FAR_SINGLE:
					_statsKey = SteamStatsKey.AVERAGE_SPEED;
					break;
			}
		}
	}
}
