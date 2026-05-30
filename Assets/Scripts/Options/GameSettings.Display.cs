using System;
using System.ComponentModel;
using Debug;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Options {
    public static partial class GameSettings {
        public static class Display {
            private const string KEY_VSYNC = "Settings_VSync";
            private const string KEY_RES_WIDTH = "Settings_ResWidth";
            private const string KEY_RES_HEIGHT = "Settings_ResHeight";
            private const string KEY_AA_MODE = "Settings_AntiAliasing";
            private const string KEY_DISPLAY_MODE = "Settings_DisplayMode";
            private const string KEY_BLOOM = "Settings_Bloom";

            public static bool VSync { get; set; }
            public static bool Bloom { get; set; }
            public static int ResolutionWidth { get; set; }
            public static int ResolutionHeight { get; set; }
            public static int AntiAliasingMode { get; set; }
            public static FullScreenMode DisplayMode { get; set; }
        
            public static void Load() {
                VSync = PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1;
                ResolutionWidth = PlayerPrefs.GetInt(KEY_RES_WIDTH, UnityEngine.Display.main.systemWidth);
                ResolutionHeight = PlayerPrefs.GetInt(KEY_RES_HEIGHT, UnityEngine.Display.main.systemHeight);
                AntiAliasingMode = PlayerPrefs.GetInt(KEY_AA_MODE, 1);
                Bloom = PlayerPrefs.GetInt(KEY_BLOOM, 1) == 1;
                LoadDisplayMode();
            }

            public static void Save() {
                PlayerPrefs.SetInt(KEY_VSYNC, VSync ? 1 : 0);
                PlayerPrefs.SetInt(KEY_BLOOM, Bloom ? 1 : 0);
                PlayerPrefs.SetInt(KEY_RES_WIDTH, ResolutionWidth);
                PlayerPrefs.SetInt(KEY_RES_HEIGHT, ResolutionHeight);
                PlayerPrefs.SetInt(KEY_AA_MODE, AntiAliasingMode);
                PlayerPrefs.SetString(KEY_DISPLAY_MODE, DisplayMode.ToString());
            }

            public static void Apply(bool applyResolution = true) {
                QualitySettings.vSyncCount = VSync ? 1 : 0;
                if (applyResolution) Screen.SetResolution(ResolutionWidth, ResolutionHeight, DisplayMode);
                ApplyAntiAliasing();
            }
        
            private static void LoadDisplayMode() {
                string mode = PlayerPrefs.GetString(KEY_DISPLAY_MODE, nameof(FullScreenMode.ExclusiveFullScreen));
                if (Enum.TryParse<FullScreenMode>(mode, out var parsedMode)) {
                    DisplayMode = parsedMode;
                } else {
                    DebugLogger.LogException(LogChannel.Systems,
                        new InvalidEnumArgumentException("Invalid display mode in GameSettings!"));
                }
            }
        
            public static void ApplyAntiAliasing() {
                var mode = AntiAliasingMode switch {
                    1 => AntialiasingMode.FastApproximateAntialiasing,
                    2 => AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                    _ => AntialiasingMode.None
                };

                foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                {
                    var cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
                    if (cameraData != null)
                    {
                        cameraData.antialiasing = mode;
                    }
                }
            }
        } 
    }
}