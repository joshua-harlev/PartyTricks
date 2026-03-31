using UnityEngine.UIElements;

namespace Options {
    public class SoundTabHandler : IOptionsTab {
        private Slider volumeSlider;
        private Slider musicVolumeSlider;
        private Slider sfxVolumeSlider;
        
        public void Initialize(VisualElement tabRoot) {
            volumeSlider = tabRoot.Q<Slider>("Volume_Slider");
            musicVolumeSlider = tabRoot.Q<Slider>("Music_Volume_Slider");
            sfxVolumeSlider = tabRoot.Q<Slider>("SFX_Volume_Slider");
        }

        public void SyncToSettings() {
            volumeSlider.lowValue = 0f;
            volumeSlider.highValue = 1f;
            volumeSlider.value = GameSettings.Sound.Volume;
          
            musicVolumeSlider.lowValue = 0f;
            musicVolumeSlider.highValue = 1f;
            musicVolumeSlider.value = GameSettings.Sound.MusicVolume;

            sfxVolumeSlider.lowValue = 0f;
            sfxVolumeSlider.highValue = 1f;
            sfxVolumeSlider.value = GameSettings.Sound.SFXVolume;
        }

        public void RegisterCallbacks() {
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Sound.Volume = evt.newValue;
                GameSettings.Sound.ApplyVolume();
            });
            musicVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Sound.MusicVolume = evt.newValue;
                GameSettings.Sound.ApplyMusicVolume();
            });
            sfxVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Sound.SFXVolume = evt.newValue;
                GameSettings.Sound.ApplySFXVolume();
            });
        }

        public void Cleanup() { }
    }
}