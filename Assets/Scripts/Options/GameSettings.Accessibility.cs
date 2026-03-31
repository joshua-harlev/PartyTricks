using UnityEngine;

public partial class GameSettings {
    public static class Accessibility {
        public static float ScreenShakeIntensity { get; set; }
        public static bool AnimateClouds { get; set; }
        
        private const string KEY_SCREEN_SHAKE = "Settings_ScreenShake";
        private const string KEY_ANIMATE_CLOUDS = "Settings_AnimateClouds";
        public static void Load() {
            ScreenShakeIntensity = PlayerPrefs.GetFloat(KEY_SCREEN_SHAKE, 1f);
            AnimateClouds = PlayerPrefs.GetInt(KEY_ANIMATE_CLOUDS, 1) == 1;
        }

        public static void Save() {
            PlayerPrefs.SetInt(KEY_ANIMATE_CLOUDS, AnimateClouds ? 1 : 0);
            PlayerPrefs.SetFloat(KEY_SCREEN_SHAKE, ScreenShakeIntensity);
        }

        public static void Apply() { }
    }
}