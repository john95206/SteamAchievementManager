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
        private AchievementKeyType _key;
        public AchievementKeyType Key => _key;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }
    }
}
