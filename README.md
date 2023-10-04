# SteamAchievementManager

Unity の Steam 実績の実装が面倒だったので便利なものを作りました。

`AchievementManager.UpdateAchievement` を呼び出せば実績の更新ができるようになっています。

```
AchievementManager.Instance.UpdateAchievement(new SteamAchievement
(
    key,
    ApiType.INT,
    _totalNumOfWins.ToString()
), out progress);
```

`AVGRATE` 型の実績は共通化できなかったので、それ用の `AchievementManager.UpdateAvgrateAchievement` を用意しました。

```
AchievementManager.Instance.UpdateAvgrateAchievement(new SteamAchievement
(
    key,
    ApiType.FLOAT,
    _gameFeetTraveled.ToString(),
    _gameDurationSeconds
), out _averageSpeed);
```

`AchievementManager` はシングルトン化しているので、 GameObject として用意しなくても `AchievementManager.Instance` で呼び出せば実行できるようになっています。

# 導入方法



# 設計思想

実績の解除状況や進捗はすべて Steam 側に任せた。
