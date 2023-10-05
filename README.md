# Installation(導入)

### PackageManager を使う

`Packages` の `manifest.json` の `dependencies` に下記を追記してください。

`"com.yuu.steamachievementmanagerpackage": "https://github.com/john95206/SteamAchievementManager.git?path=/SteamAchievementManager/Packages/SteamAchievementManager"`

または、 `PackageManager` の `Add package from git...` にて下記 URL を入れてください

`https://github.com/john95206/SteamAchievementManager.git?path=/SteamAchievementManager/Packages/SteamAchievementManager`


### リポジトリをクローンする

サンプルプロジェクトを用意しているので、具体的な使い方を見たい場合はこのリポジトリをクローンしてください。
`Assets/SteamAchievementManager/Example/Scenes/Example` にシーンを用意しています。
Steam 公式で公開している、 Space War の実績を用いて実績の更新、解除が試せるようになっています。

# Summary(概要)

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

`AchievementManager` はシングルトン化しているので、 GameObject として用意したり GetComponent しなくても
`AchievementManager.Instance` で呼び出せば実行できるようになっています。

# How To Use (使い方)

## AchievementManager.UpdateAchievement(ISteamAchievement, out string progress)

`AchievementManager.UpdateAchievement` を呼び出せば Steam 実績の更新をやってくれますが、インターフェース `ISteamAchievement` として引数を用意しています。
お使いの際はご自身で `ISteamAchievement` を継承したクラスを作り、それを `UpdateAchievement` の引数に与えるようにしてお使いください。
サンプルプロジェクトでは以下のようにしています。

```
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
```

```
public enum AchievementKeyType
{
    ACH_WIN_ONE_GAME,
    ACH_WIN_100_GAMES,
    ACH_TRAVEL_FAR_ACCUM,
    ACH_TRAVEL_FAR_SINGLE,
}
```

```
public class SteamStatsKey
{
    public const string NUM_GAMES = "NumGames";
    public const string NUM_WINS = "NumWins";
    public const string FEET_TRAVELED = "FeetTraveled";
    public const string AVERAGE_SPEED = "AverageSpeed";
}
```

この例ですと、`AchievementKeyType` に Steamworks で設定した実績の Key を格納するようにしてください。
また、Steamworks で設定したデータを使う場合は、`SteamStatsKey` に設定した Key を格納するようにしてください。

# Sample Project(サンプルプロジェクトについて)

Steam 公式で公開している Space War の実績解除を `SteamAchievementManager` を用いて試すことができます。
しかし、実際に解除できるのは `Intersteller` だけかと思います。
他の実績に関しては、目標の値に到達しても解除は確認できませんが、SteamStats には登録されており、データが更新されていることは確認できるかと思います。

# Architecture(設計思想)

サンプルプロジェクトでは `Intersteller` だけが実績解除を確認できることについて。
これは本プロジェクトと Space War の実績解除の設計思想が競合したために起きている現象です。
Steam 公式で配布されているサンプル（https://github.com/rlabrecque/Steamworks.NET-Example）では、
ローカルで `UserStats` のデータを保持しておき、一定の値を超えたら `SetAchievement` して実績を解除しています。
しかし、Steamworks でデータを用意し、それを実績に紐づけている「進行系」の実績ならば、`UserStats` を更新するだけで、
実績の条件をクリアしたかどうかは Steam の方でやってくれます。
（例：「敵を100体倒した」という実績は、敵を倒した数を Steam に送信していれば、100体目で自動的に実績が解除される）
本プロジェクトではその実装に乗っかり、「進行系」の実績であればローカルから `SetAchievement` せずに `StoreStats` だけするようにしました。

もちろん、「進行系でない」実績（例：最後のボスを倒した）は `UpdateAchievement` の実行が成功すれば直ちに実績が解除されます。

「進行系」の実績を実装する際は、忘れずに Steamworks の「データ」項目にて、API を用意するようにしてください。
その後、その API と実績を紐づけるようにしてください。
