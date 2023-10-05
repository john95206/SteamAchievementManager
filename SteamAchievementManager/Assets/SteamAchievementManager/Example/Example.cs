using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SteamAchievement.Example
{
    /// <summary>
    /// AchievementManager の使い方サンプル。
    /// Steam のことを考えずに AchievementManager.UpdateAchievement を呼べば実績が更新されるようにした。
    /// </summary>
    public class Example : MonoBehaviour
    {
        // Current Stat details
        private float _gameFeetTraveled;
        private double _gameDurationSeconds;
        private float _tickCountGameStart;

        // Persisted Stat details
        private int _totalNumOfWins;
        private float _totalFeetTraveled;
        private float _averageSpeed;
        private float _maxFeetTraveled;

        [SerializeField]
        private List<AchievementButton> _achievementButtonList = new List<AchievementButton>();
        [SerializeField]
        private Button _resetButton = default;
        [SerializeField]
        private TextMeshProUGUI _totalWinText = default;
        [SerializeField]
        private TextMeshProUGUI _totalFeetTraveledText = default;
        [SerializeField]
        private TextMeshProUGUI _gameFeetTraveledText = default;
        [SerializeField]
        private TextMeshProUGUI _gameDurationSecondsText = default;
        [SerializeField]
        private TextMeshProUGUI _averageSpeedText = default;
        [SerializeField]
        private Button _activateButton = default;

        private void Start()
        {
            _resetButton.onClick.AddListener(() =>
            {
                ResetGame();
            });

            foreach (var button in _achievementButtonList)
            {
                button.Button.onClick.AddListener(() =>
                {
                    OnAchieveButton(button.Key);
                });
            }

            _activateButton.onClick.AddListener(() =>
            {
                ActivateGame();
            });

            ActivateGame();
        }

        private void OnAchieveButton(AchievementKeyType key)
        {
            string progress = string.Empty;

            switch (key)
            {
                case AchievementKeyType.ACH_WIN_ONE_GAME:
                    WinGame();
                    AchievementManager.Instance.UpdateAchievement(new SteamAchievement
                    (
                        key,
                        ApiType.INT,
                        _totalNumOfWins.ToString()
                    ), out progress);
                    _totalWinText.text = progress;
                    break;
                case AchievementKeyType.ACH_WIN_100_GAMES:
                    for (var i = 0; i < 100; i++)
                    {
                        WinGame();
                    }
                    AchievementManager.Instance.UpdateAchievement(new SteamAchievement
                    (
                        key,
                        ApiType.INT,
                        _totalNumOfWins.ToString()
                    ), out progress);
                    _totalWinText.text = progress;
                    break;
                case AchievementKeyType.ACH_TRAVEL_FAR_ACCUM:
                    OnStoreStats();
                    AchievementManager.Instance.UpdateAchievement(new SteamAchievement
                    (
                        key,
                        ApiType.FLOAT,
                        _totalFeetTraveled.ToString()
                    ), out progress);
                    _totalFeetTraveledText.text = progress;
                    break;
                case AchievementKeyType.ACH_TRAVEL_FAR_SINGLE:
                    AddDistanceTraveled(100.0f);
                    AchievementManager.Instance.UpdateAvgrateAchievement(new SteamAchievement
                    (
                        key,
                        ApiType.AVGRATE,
                        _gameFeetTraveled.ToString(),
                        _gameDurationSeconds
                    ), out _averageSpeed);
                    _gameFeetTraveledText.text = _gameFeetTraveled.ToString();
                    _gameDurationSecondsText.text = _gameDurationSeconds.ToString();
                    _averageSpeedText.text = _averageSpeed.ToString();
                    break;
            }
        }

        private void AddDistanceTraveled(float flDistance)
        {
            _gameFeetTraveled += flDistance;
            OnStoreStats();
        }

        private void ActivateGame()
        {
            _gameDurationSeconds = 0;
            _gameFeetTraveled = 0;
            _tickCountGameStart = 0;
            _tickCountGameStart = Time.time;
            _gameFeetTraveledText.text = _gameFeetTraveled.ToString();
            _gameDurationSecondsText.text = _gameDurationSeconds.ToString();

            AchievementManager.Instance.GetIntStatsByKey(SteamStatsKey.NUM_WINS, out _totalNumOfWins);
            // 値がなかった場合 int.MinValue で返しているので 0 に丸める
            if (_totalNumOfWins == int.MinValue)
            {
                _totalNumOfWins = 0;
            }

            AchievementManager.Instance.GetAvgrateStatsByKey(SteamStatsKey.AVERAGE_SPEED, out _averageSpeed);
            // 値がなかった場合 float.MinValue で返しているので 0 に丸める
            if (_averageSpeed == float.MinValue)
            {
                _averageSpeed = 0;
            }

            AchievementManager.Instance.GetFloatStatsByKey(SteamStatsKey.FEET_TRAVELED, out _totalFeetTraveled);
            // 値がなかった場合 float.MinValue で返しているので 0 に丸める
            if (_totalFeetTraveled == float.MinValue)
            {
                _totalFeetTraveled = 0;
            }

            _totalWinText.text = _totalNumOfWins.ToString();
            _averageSpeedText.text = _averageSpeed.ToString();
            _totalFeetTraveledText.text = _totalFeetTraveled.ToString();
        }

        private void WinGame()
        {
            _totalNumOfWins++;
            OnStoreStats();
        }

        private void OnStoreStats()
        {
            _gameDurationSeconds = Time.time - _tickCountGameStart;
            _totalFeetTraveled += _gameFeetTraveled;

            if (_gameFeetTraveled > _maxFeetTraveled)
            {
                _maxFeetTraveled = _gameFeetTraveled;
            }
        }

        private void ResetGame()
        {
            AchievementManager.Instance.ResetAchievement();
            _totalNumOfWins = 0;
            _totalWinText.text = _totalNumOfWins.ToString();
            _totalFeetTraveled = 0;
            _totalFeetTraveledText.text = _totalFeetTraveledText.ToString();
            _gameFeetTraveledText.text = _gameFeetTraveled.ToString();
            _gameDurationSecondsText.text = _gameDurationSeconds.ToString();
        }
    }
}
