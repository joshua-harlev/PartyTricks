using UnityEngine;

public partial class GameSettings {
    public static class Gameplay {
        public static bool UsePresetBoard { get; set; }
        public static int TimerLengths { get; set; }

        private const string KEY_USE_PRESET_BOARD = "Settings_PresetBoard";
        private const string KEY_TIMER_LENGTHS = "Settings_TimerLengths";

        public static void Load() {
            TimerLengths = PlayerPrefs.GetInt(KEY_TIMER_LENGTHS, 1);
            UsePresetBoard = PlayerPrefs.GetInt(KEY_USE_PRESET_BOARD, 1) == 1;
        }

        public static void Save() {
            PlayerPrefs.SetInt(KEY_USE_PRESET_BOARD, UsePresetBoard ? 1 : 0);
            PlayerPrefs.SetInt(KEY_TIMER_LENGTHS, TimerLengths);
        }

        public static void Apply() { }
    }
}