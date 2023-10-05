using UnityEngine;
using UnityEngine.UI;

namespace SteamAchievement.Example
{
    [RequireComponent(typeof(Button))]
    public class AchievementButton : MonoBehaviour
    {
        private Button _button = default;
        public Button Button => _button;
        [SerializeField]
        private AchievementKey _key;
        public AchievementKey Key => _key;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }
    }
}
