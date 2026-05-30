using UnityEngine;

namespace Options {
    public partial class GameSettings {
        public static class Misc {
            private const string KEY_RANDOMIZE_COIN_SPIN_DIRECTION = "Settings_RandomizeCoinSpinDirection";
            private const string KEY_SHOW_PLAYER_LABELS = "Settings_ShowPlayerLabels";
            public static bool RandomizeCoinSpinDirection { get; set; }
            public static bool ShowPlayerLabels { get; set; }
            public static void Load() {
                RandomizeCoinSpinDirection = PlayerPrefs.GetInt(KEY_RANDOMIZE_COIN_SPIN_DIRECTION, 0) == 1;
                ShowPlayerLabels = PlayerPrefs.GetInt(KEY_SHOW_PLAYER_LABELS, 1) == 1;
            }

            public static void Save() {
                PlayerPrefs.SetInt(KEY_RANDOMIZE_COIN_SPIN_DIRECTION, RandomizeCoinSpinDirection ? 1 : 0);
                PlayerPrefs.SetInt(KEY_SHOW_PLAYER_LABELS, ShowPlayerLabels ? 1 : 0);
            }

            public static void Apply() { }
        }
    }
}