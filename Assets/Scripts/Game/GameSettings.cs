using System.ComponentModel;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class GameSettings {

    private const string KEY_VSYNC = "Settings_VSync";
    private const string KEY_RES_WIDTH = "Settings_ResWidth";
    private const string KEY_RES_HEIGHT = "Settings_ResHeight";
    private const string KEY_AA_MODE = "Settings_AntiAliasing";
    private const string KEY_VOLUME = "Settings_Volume";
    private const string KEY_MUSIC_VOLUME = "Settings_MusicVolume";
    private const string KEY_SFX_VOLUME = "Settings_SFXVolume";
    private const string KEY_SCREEN_SHAKE = "Settings_ScreenShake";
    private const string KEY_USE_PRESET_BOARD = "Settings_PresetBoard";
    private const string KEY_DISPLAYMODE = "Settings_DisplayMode";
    public static bool VSync { get; set; }
    public static int ResolutionWidth { get; set; }
    public static int ResolutionHeight { get; set; }
    public static int AntiAliasingMode { get; set; }
    public static float Volume { get; set; }
    public static float MusicVolume { get; set; }
    public static float SFXVolume { get; set; }
    public static float ScreenShakeIntensity { get; set; }
    public static bool UsePresetBoard { get; set; }
    public static FullScreenMode DisplayMode { get; set; }

    public static void Load() {
        VSync = PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1;
        ResolutionWidth = PlayerPrefs.GetInt(KEY_RES_WIDTH, Display.main.systemWidth);
        ResolutionHeight = PlayerPrefs.GetInt(KEY_RES_HEIGHT, Display.main.systemHeight);
        AntiAliasingMode = PlayerPrefs.GetInt(KEY_AA_MODE, 0);
        Volume = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f);
        SFXVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
        ScreenShakeIntensity = PlayerPrefs.GetFloat(KEY_SCREEN_SHAKE, 1f);
        UsePresetBoard = PlayerPrefs.GetInt(KEY_USE_PRESET_BOARD, 1) == 1;
        LoadDisplayMode();
    }


    private static void LoadDisplayMode() {
        string mode = PlayerPrefs.GetString(KEY_DISPLAYMODE, nameof(FullScreenMode.ExclusiveFullScreen));
        switch (mode)
        {
            case nameof(FullScreenMode.ExclusiveFullScreen):
                DisplayMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case nameof(FullScreenMode.Windowed):
                DisplayMode = FullScreenMode.Windowed;
                break;
            default:
                DebugLogger.LogException(LogChannel.Systems, new InvalidEnumArgumentException("Invalid display mode in GameSettings!"));
                break;

        }
    }

    public static void Save() {
        PlayerPrefs.SetInt(KEY_VSYNC, VSync ? 1 : 0);
        PlayerPrefs.SetInt(KEY_RES_WIDTH, ResolutionWidth);
        PlayerPrefs.SetInt(KEY_RES_HEIGHT, ResolutionHeight);
        PlayerPrefs.SetInt(KEY_AA_MODE, AntiAliasingMode);
        PlayerPrefs.SetInt(KEY_USE_PRESET_BOARD, UsePresetBoard ? 1 : 0);
        PlayerPrefs.SetFloat(KEY_VOLUME, Volume);
        PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, MusicVolume);
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, SFXVolume);
        PlayerPrefs.SetFloat(KEY_SCREEN_SHAKE, ScreenShakeIntensity);
        PlayerPrefs.SetString(KEY_DISPLAYMODE, DisplayMode.ToString());
        PlayerPrefs.Save();
    }

    public static void Apply(bool applyDisplay = true) {
        QualitySettings.vSyncCount = VSync ? 1 : 0;
        if (applyDisplay) Screen.SetResolution(ResolutionWidth, ResolutionHeight, DisplayMode);
        ApplyVolume();
        ApplyMusicVolume();
        ApplySFXVolume();
        ApplyAntiAliasing();
    }

    public static void ApplyVolume() {
        RuntimeManager.GetBus("bus:/").setVolume(Volume);
    }

    public static void ApplyMusicVolume() {
        RuntimeManager.GetBus("bus:/Music").setVolume(MusicVolume);
    }

    public static void ApplySFXVolume() {
        RuntimeManager.GetBus("bus:/SFX").setVolume(SFXVolume);
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

