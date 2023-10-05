using UnityEngine;
using Steamworks;
using System;
using System.Collections.Generic;

namespace SteamAchievement
{
	/// <summary>
	/// 実績の Manager
	/// </summary>
	public class AchievementManager : MonoBehaviour
	{
		private static AchievementManager instance;
		public static AchievementManager Instance
		{
			get
			{
				if (!instance)
				{
					if (instance == null)
					{
						instance = FindObjectOfType<AchievementManager>();
						if (!instance)
						{
							GameObject singleton = new GameObject();
							instance = singleton.AddComponent<AchievementManager>();
							singleton.name = typeof(AchievementManager).ToString();

							DontDestroyOnLoad(singleton);
						}
					}
				}
				return instance;
			}
		}

		private List<string> _achievedList = new List<string>();

		private void Start()
		{
			if (!SteamManager.Initialized)
			{
				Log($"Failed Initialize", true);
				return;
			}

			Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			Callback<UserAchievementStored_t>.Create(OnAchievementStored);

			SteamUserStats.RequestCurrentStats();
		}

		/// <summary>
		/// 実績を更新する
		/// </summary>
		/// <param name="achievement"></param>
		public void UpdateAchievement(ISteamAchievement achievement, out string progress)
		{
			progress = string.Empty;
			if (!SteamManager.Initialized)
			{
				Log($"Steam Error: Initialize Failed", true);
				return;
			}

			if (_achievedList.Contains(achievement.AchievementKey))
			{
				Log($"Already achieved: {achievement.AchievementKey}");
				progress = achievement.Progress;
				return;
			}

			string achievementKey = achievement.AchievementKey.ToString();

			// 進捗系でなければ発火即実績解除
			if (string.IsNullOrEmpty(achievement.Progress))
			{
				UnlockAchievement(achievementKey);
			}
			else
			{
				UpdateProgressiveAchievement(achievement, out progress);
			}
		}

		/// <summary>
		/// 進捗系実績を更新する。
		/// 実績の達成・解除済みかどうかの考慮は Steam の方でやってくれる。
		/// </summary>
		/// <param name="achievement"></param>
		/// <param name="progress">更新された進捗</param>
		private void UpdateProgressiveAchievement(ISteamAchievement achievement, out string progress)
		{
			progress = achievement.Progress;
			if (string.IsNullOrEmpty(achievement.Progress))
			{
				return;
			}

			if (!GetStat(achievement, out string registedProgress))
			{
				return;
			}

			// 値が更新されていなければ早期 return
			if (achievement.Progress == registedProgress)
			{
				return;
			}

			if (!float.TryParse(achievement.Progress, out var parsedProgress))
			{
				return;
			}

			if (parsedProgress < float.Parse(registedProgress))
			{
				progress = registedProgress;
			}

			if (!SetStat(achievement, progress))
			{
				return;
			}

			if (!SteamUserStats.StoreStats())
			{
				Log($"Steam Error: Couldn't Update Status", true);
				return;
			}
		}

		/// <summary>
		/// Avgrate 実績を更新する
		/// </summary>
		/// <param name="achievement"></param>
		/// <param name="averagedResult"></param>
		/// <returns></returns>
		public bool UpdateAvgrateAchievement(ISteamAchievement achievement, out float averagedResult)
		{
			averagedResult = float.MinValue;

			if (!SteamManager.Initialized)
			{
				Log($"Steam Error: Initialize Failed", true);
				return false;
			}

			if (achievement.ApiType != ApiType.AVGRATE)
			{
				return false;
			}

			if (string.IsNullOrEmpty(achievement.Progress))
			{
				return false;
			}

			if (_achievedList.Contains(achievement.AchievementKey))
			{
				Log($"Already achieved: {achievement.AchievementKey}");
				averagedResult = float.Parse(achievement.Progress);
				return false;
			}

			if (!GetStat(achievement, out string registedProgress))
			{
				return false;
			}

			// 値が更新されていなければ早期 return
			if (achievement.Progress == registedProgress)
			{
				return false;
			}

			if (!float.TryParse(achievement.Progress, out averagedResult))
			{
				return false;
			}

			// Update average feet / second stat
			if (!SteamUserStats.UpdateAvgRateStat(achievement.StatsKey, averagedResult, achievement.Duration))
			{
				return false;
			}

			if (!GetStat(achievement, out registedProgress))
			{
				return false;
			}

			averagedResult = float.Parse(registedProgress);

			return true;
		}

		/// <summary>
		/// 実績を解除する
		/// 解除済みかどうかの考慮は Steam がやってくれる
		/// </summary>
		/// <param name="achievementKey">実績 API キー</param>
		private void UnlockAchievement(string achievementKey)
		{
			if (!SteamUserStats.SetAchievement(achievementKey))
			{
				Log($"Steam Error: Couldn't Update Achievement", true);
				return;
			}

			// 実績の状態を Steam に送信する
			if (!SteamUserStats.RequestCurrentStats())
			{
				Log($"Steam Error: Couldn't Request Stats", true);
				return;
			}
		}

		/// <summary>
		/// SteamUserStats.RequestCurrentStats() のコールバック。
		/// 公式に用意しろと言われたので用意した
		/// https://partner.steamgames.com/doc/features/achievements/ach_guide
		/// </summary>
		/// <param name="_"></param>
		private void OnUserStatsReceived(UserStatsReceived_t value)
		{
		}

		/// <summary>
		/// SteamUserStats.StoreStats() のコールバック。
		/// 公式に用意しろと言われたので用意した
		/// https://partner.steamgames.com/doc/features/achievements/ach_guide
		/// </summary>
		/// <param name="_"></param>
		private void OnUserStatsStored(UserStatsStored_t _)
		{
		}

		/// <summary>
		/// SteamUserStats.SetAchievement のコールバック。
		/// 公式に用意しろと言われたので用意した
		/// https://partner.steamgames.com/doc/features/achievements/ach_guide
		/// </summary>
		/// <param name="_"></param>
		private void OnAchievementStored(UserAchievementStored_t _)
		{
		}

		/// <summary>
		/// SteamUserStats.GetStat を呼び出す。
		/// </summary>
		/// <param name="steamAchievement"></param>
		/// <param name="progress">受信する進捗</param>
		/// <returns>API が正常に終了したかどうか</returns>
		private bool GetStat(ISteamAchievement steamAchievement, out string progress)
		{
			return GetStatByKey(steamAchievement.StatsKey, steamAchievement.ApiType, out progress);
		}

		/// <summary>
		/// Stats 名と API Type から実績の進行状態を返す
		/// </summary>
		/// <param name="key">Stats 名</param>
		/// <param name="apiType">API Type</param>
		/// <param name="progress">受信する進捗</param>
		/// <returns>API が正常に終了したかどうか</returns>
		private bool GetStatByKey(string key, ApiType apiType, out string progress)
		{
			switch (apiType)
			{
				case ApiType.INT:
					int intProgress;
					if (!SteamUserStats.GetStat(key, out intProgress))
					{
						progress = intProgress == int.MinValue ? string.Empty : intProgress.ToString();
						return false;
					}
					progress = intProgress == int.MinValue ? string.Empty : intProgress.ToString();
					return true;
				case ApiType.FLOAT:
					float floatProgress;
					if (!SteamUserStats.GetStat(key, out floatProgress))
					{
						progress = floatProgress == float.MinValue ? string.Empty : floatProgress.ToString();
						return false;
					}
					progress = floatProgress == float.MinValue ? string.Empty : floatProgress.ToString();
					return true;
				case ApiType.AVGRATE:
					float avgrateProgress;
					if (!SteamUserStats.GetStat(key, out avgrateProgress))
					{
						progress = avgrateProgress == float.MinValue ? string.Empty : avgrateProgress.ToString();
						return false;
					}
					progress = avgrateProgress == float.MinValue ? string.Empty : avgrateProgress.ToString();
					return true;
				default:
					progress = string.Empty;
					return false;
			}
		}

		public bool GetIntStatsByKey(string key, out int progresss)
		{
			if (!GetStatByKey(key, ApiType.INT, out string progressString))
			{
				progresss = int.MinValue;
				return false;
			}
			progresss = int.Parse(progressString);
			return true;
		}

		public bool GetFloatStatsByKey(string key, out float progresss)
		{
			if (!GetStatByKey(key, ApiType.INT, out string progressString))
			{
				progresss = float.MinValue;
				return false;
			}
			progresss = float.Parse(progressString);
			return true;
		}

		public bool GetAvgrateStatsByKey(string key, out float progresss)
		{
			if (!GetStatByKey(key, ApiType.AVGRATE, out string progressString))
			{
				progresss = float.MinValue;
				return false;
			}
			progresss = float.Parse(progressString);
			return true;
		}

		/// <summary>
		/// SteamUserStats.SetStat を呼び出す。
		/// </summary>
		/// <param name="steamAchievement"></param>
		/// <param name="progress">送信する進捗</param>
		/// <returns>API が正常に終了したかどうか</returns>
		private bool SetStat(ISteamAchievement steamAchievement, string progress)
		{
			switch (steamAchievement.ApiType)
			{
				case ApiType.INT:
					var intProgress = int.TryParse(progress, out var parsedIntProgress) ? parsedIntProgress : int.MinValue;
					if (!SteamUserStats.SetStat(steamAchievement.StatsKey, intProgress))
					{
						Log($"Steam Error: Couldn't set {steamAchievement.StatsKey} stats", true);
						return false;
					}
					return true;
				case ApiType.FLOAT:
					var floatProgress = float.TryParse(progress, out var parsedfloatProgress) ? parsedfloatProgress : int.MinValue;
					if (!SteamUserStats.SetStat(steamAchievement.StatsKey, floatProgress))
					{
						Log($"Steam Error: Couldn't set {steamAchievement.StatsKey} stats", true);
						return false;
					}
					return true;
				case ApiType.AVGRATE:
					var avgrateProgress = float.TryParse(progress, out var parsedavgrateProgress) ? parsedavgrateProgress : int.MinValue;
					if (!SteamUserStats.SetStat(steamAchievement.StatsKey, avgrateProgress))
					{
						Log($"Steam Error: Couldn't set {steamAchievement.StatsKey} stats", true);
						return false;
					}
					return true;
				default:
					Log($"Steam Error: Couldn't set {steamAchievement.StatsKey} stats", true);
					return false;
			}
		}

		/// <summary>
		/// 実績のリセットを行う
		/// </summary>
		public void ResetAchievement()
		{
			SteamUserStats.ResetAllStats(true);
			SteamUserStats.RequestCurrentStats();
			_achievedList.Clear();
		}

		private void Log(string text, bool isError = false)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (isError)
			{
				Debug.LogError(text);
			}
			else
			{
				Debug.Log(text);
			}
#endif
		}
	}
}
