using FMODUnity;
using UnityEngine;

public partial class GameSettings {
    public static class Sound {
        private const string KEY_VOLUME = "Settings_Volume";
        private const string KEY_MUSIC_VOLUME = "Settings_MusicVolume";
        private const string KEY_SFX_VOLUME = "Settings_SFXVolume";
        
        public static float Volume { get; set; }
        public static float MusicVolume { get; set; }
        public static float SFXVolume { get; set; }
        
        public static void Load() {
            Volume = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
            MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f);
            SFXVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
        }

        public static void Save() {
            PlayerPrefs.SetFloat(KEY_VOLUME, Volume);
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, MusicVolume);
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME, SFXVolume);
        }
        public static void Apply() {
            ApplyVolume();
            ApplyMusicVolume();
            ApplySFXVolume();
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
    }

}