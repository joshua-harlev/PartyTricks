using UnityEngine;

namespace Options {
    public partial class GameSettings {
        public static class Accessibility {
            public static float ScreenShakeIntensity { get; set; }
            public static bool AnimateClouds { get; set; }
            public static bool TinnitusFilterEnabled { get; set; }
            public static float TinnitusFilterFrequency { get; set; }
            public static float TinnitusFilterGain { get; set; }
            public static bool IncreaseBackgroundVisibility { get; set; }
            public static bool DisableParallax { get; set; }
            public static bool OneHandedMode { get; set; }
            public static bool ToggleCharge { get; set; }
        
            private const string KEY_SCREEN_SHAKE = "Settings_ScreenShake";
            private const string KEY_ANIMATE_CLOUDS = "Settings_AnimateClouds";
            private const string KEY_TINNITUS_FILTER_ENABLED = "Settings_TinnitusFilterEnabled";
            private const string KEY_TINNITUS_FILTER_FREQUENCY = "Settings_TinnitusFilterFrequency";
            private const string KEY_TINNITUS_FILTER_GAIN = "Settings_TinnitusFilterGain";
            private const string KEY_INCREASE_BACKGROUND_VISIBILITY = "Settings_IncreaseBackgroundVisibility";
            private const string KEY_DISABLE_PARALLAX = "Settings_DisableParallax";
            private const string KEY_ONE_HANDED_MODE = "Settings_OneHandedMode";
            private const string KEY_TOGGLE_CHARGE = "Settings_ToggleCharge";

            public static void Load() {
                ScreenShakeIntensity = PlayerPrefs.GetFloat(KEY_SCREEN_SHAKE, 1f);
                AnimateClouds = PlayerPrefs.GetInt(KEY_ANIMATE_CLOUDS, 1) == 1;
                TinnitusFilterEnabled = PlayerPrefs.GetInt(KEY_TINNITUS_FILTER_ENABLED, 0) == 1;
                TinnitusFilterFrequency = PlayerPrefs.GetFloat(KEY_TINNITUS_FILTER_FREQUENCY, 8000f);
                TinnitusFilterGain = PlayerPrefs.GetFloat(KEY_TINNITUS_FILTER_GAIN, -30f);
                IncreaseBackgroundVisibility = PlayerPrefs.GetInt(KEY_INCREASE_BACKGROUND_VISIBILITY, 0) == 1;
                DisableParallax = PlayerPrefs.GetInt(KEY_DISABLE_PARALLAX, 0) == 1;
                OneHandedMode = PlayerPrefs.GetInt(KEY_ONE_HANDED_MODE, 0) == 1;
                ToggleCharge = PlayerPrefs.GetInt(KEY_TOGGLE_CHARGE, 0) == 1;
            }

            public static void Save() {
                PlayerPrefs.SetInt(KEY_ANIMATE_CLOUDS, AnimateClouds ? 1 : 0);
                PlayerPrefs.SetFloat(KEY_SCREEN_SHAKE, ScreenShakeIntensity);
                PlayerPrefs.SetInt(KEY_TINNITUS_FILTER_ENABLED, TinnitusFilterEnabled ? 1 : 0);
                PlayerPrefs.SetFloat(KEY_TINNITUS_FILTER_FREQUENCY, TinnitusFilterFrequency);
                PlayerPrefs.SetFloat(KEY_TINNITUS_FILTER_GAIN, TinnitusFilterGain);
                PlayerPrefs.SetInt(KEY_INCREASE_BACKGROUND_VISIBILITY, IncreaseBackgroundVisibility ? 1 : 0);
                PlayerPrefs.SetInt(KEY_DISABLE_PARALLAX, DisableParallax ? 1 : 0);
                PlayerPrefs.SetInt(KEY_ONE_HANDED_MODE, OneHandedMode ? 1 : 0);
                PlayerPrefs.SetInt(KEY_TOGGLE_CHARGE, ToggleCharge ? 1 : 0);
            }

            public static void Apply() { }
        }
    }
}