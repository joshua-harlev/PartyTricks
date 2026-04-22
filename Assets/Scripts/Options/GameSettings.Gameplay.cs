using UnityEngine;

public partial class GameSettings {
    public static class Gameplay {
        public static bool ShowTutorials { get; set; }
        public static bool AutoDismissTutorials { get; set; }
        public static bool LongerMinigameCountdowns { get; set; }
        public static bool UsePresetBoard { get; set; }
        public static int TimerLengths { get; set; }
        public static bool ShowFirstShop { get; set; }
        
        private const string KEY_SHOW_TUTORIALS = "Settings_ShowTutorials";
        private const string KEY_AUTO_DISMISS_TUTORIALS = "Settings_AutoDismissTutorials";
        private const string KEY_LONGER_MINIGAME_COUNTDOWNS = "Settings_LongerMinigameCountdowns";
        private const string KEY_USE_PRESET_BOARD = "Settings_PresetBoard";
        private const string KEY_TIMER_LENGTHS = "Settings_TimerLengths";
        private const string KEY_SHOW_FIRST_SHOP = "Settings_ShowFirstShop";

        public static void Load() {
            ShowTutorials = PlayerPrefs.GetInt(KEY_SHOW_TUTORIALS, 1) == 1;
            TimerLengths = PlayerPrefs.GetInt(KEY_TIMER_LENGTHS, 1);
            UsePresetBoard = PlayerPrefs.GetInt(KEY_USE_PRESET_BOARD, 1) == 1;
            AutoDismissTutorials = PlayerPrefs.GetInt(KEY_AUTO_DISMISS_TUTORIALS, 1) == 1;
            LongerMinigameCountdowns = PlayerPrefs.GetInt(KEY_LONGER_MINIGAME_COUNTDOWNS, 0) == 1;
            ShowFirstShop = PlayerPrefs.GetInt(KEY_SHOW_FIRST_SHOP, 1) == 1;
        }

        public static void Save() {
            PlayerPrefs.SetInt(KEY_SHOW_TUTORIALS, ShowTutorials ? 1 : 0);
            PlayerPrefs.SetInt(KEY_AUTO_DISMISS_TUTORIALS, AutoDismissTutorials ? 1 : 0);
            PlayerPrefs.SetInt(KEY_LONGER_MINIGAME_COUNTDOWNS, LongerMinigameCountdowns ? 1 : 0);
            PlayerPrefs.SetInt(KEY_USE_PRESET_BOARD, UsePresetBoard ? 1 : 0);
            PlayerPrefs.SetInt(KEY_SHOW_FIRST_SHOP, ShowFirstShop ? 1 : 0);
            PlayerPrefs.SetInt(KEY_TIMER_LENGTHS, TimerLengths);
        }

        public static void Apply() { }
    }
}