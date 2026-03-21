using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class GameSettings
  {
      private const string KEY_VSYNC = "Settings_VSync";
      private const string KEY_RES_WIDTH = "Settings_ResWidth";
      private const string KEY_RES_HEIGHT = "Settings_ResHeight";
      private const string KEY_AA_MODE = "Settings_AntiAliasing";
      private const string KEY_VOLUME = "Settings_Volume";
      private const string KEY_SCREEN_SHAKE = "Settings_ScreenShake";
      private const string KEY_USE_PRESET_BOARD = "Settings_PresetBoard";
      private const string KEY_MUSIC = "Settings_MusicEnabled";

      public static bool VSync { get; set; }
      public static int ResolutionWidth { get; set; }
      public static int ResolutionHeight { get; set; }
      public static int AntiAliasingMode { get; set; }
      public static float Volume { get; set; }
      public static float ScreenShakeIntensity { get; set; }
      public static bool UsePresetBoard { get; set; }
      public static bool MusicEnabled { get; set; }

      public static void Load()
      {
          VSync = PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1;
          ResolutionWidth = PlayerPrefs.GetInt(KEY_RES_WIDTH, Display.main.systemWidth);
          ResolutionHeight = PlayerPrefs.GetInt(KEY_RES_HEIGHT, Display.main.systemHeight);
          AntiAliasingMode = PlayerPrefs.GetInt(KEY_AA_MODE, 0);
          Volume = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
          ScreenShakeIntensity = PlayerPrefs.GetFloat(KEY_SCREEN_SHAKE, 1f);
          UsePresetBoard = PlayerPrefs.GetInt(KEY_USE_PRESET_BOARD, 1) == 1;
          MusicEnabled = PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
      }

      public static void Save()
      {
          PlayerPrefs.SetInt(KEY_VSYNC, VSync ? 1 : 0);
          PlayerPrefs.SetInt(KEY_RES_WIDTH, ResolutionWidth);
          PlayerPrefs.SetInt(KEY_RES_HEIGHT, ResolutionHeight);
          PlayerPrefs.SetInt(KEY_AA_MODE, AntiAliasingMode);
          PlayerPrefs.SetInt(KEY_USE_PRESET_BOARD, UsePresetBoard ? 1 : 0);
          PlayerPrefs.SetFloat(KEY_VOLUME, Volume);
          PlayerPrefs.SetFloat(KEY_SCREEN_SHAKE, ScreenShakeIntensity);
          PlayerPrefs.SetInt(KEY_MUSIC, MusicEnabled ? 1 : 0);
          PlayerPrefs.Save();
      }

      public static void Apply()
      {
          QualitySettings.vSyncCount = VSync ? 1 : 0;
          Screen.SetResolution(ResolutionWidth, ResolutionHeight, Screen.fullScreen);
          RuntimeManager.GetBus("bus:/").setVolume(Volume);
          ApplyAntiAliasing();
          ApplyMusic();
      }

      public static void ApplyVolume() {
          RuntimeManager.GetBus("bus:/").setVolume(Volume);
      }

      public static void ApplyAntiAliasing() {
          var mode = AntiAliasingMode switch
          {
              1 => AntialiasingMode.FastApproximateAntialiasing,
              2 => AntialiasingMode.SubpixelMorphologicalAntiAliasing,
              _ => AntialiasingMode.None // Default case of no antialiasing
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

      public static void ApplyMusic() {
          FMODUnity.RuntimeManager.GetBus("bus:/Music").setMute(!MusicEnabled);
      }
  }

