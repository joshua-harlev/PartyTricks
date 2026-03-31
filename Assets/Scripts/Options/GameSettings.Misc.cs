using UnityEngine;

public partial class GameSettings {
    public static class Misc {
        private const string KEY_RANDOMIZE_COIN_SPIN_DIRECTION = "Settings_RandomizeCoinSpinDirection";
        public static bool RandomizeCoinSpinDirection { get; set; }
        public static void Load() {
            RandomizeCoinSpinDirection = PlayerPrefs.GetInt(KEY_RANDOMIZE_COIN_SPIN_DIRECTION, 0) == 1;
        }

        public static void Save() {
            PlayerPrefs.SetInt(KEY_RANDOMIZE_COIN_SPIN_DIRECTION, RandomizeCoinSpinDirection ? 1 : 0);
        }

        public static void Apply() { }
    }
}