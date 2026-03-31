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
            volumeSlider.value = GameSettings.Volume;
          
            musicVolumeSlider.lowValue = 0f;
            musicVolumeSlider.highValue = 1f;
            musicVolumeSlider.value = GameSettings.MusicVolume;

            sfxVolumeSlider.lowValue = 0f;
            sfxVolumeSlider.highValue = 1f;
            sfxVolumeSlider.value = GameSettings.SFXVolume;
        }

        public void RegisterCallbacks() {
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Volume = evt.newValue;
                GameSettings.ApplyVolume();
            });
            musicVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.MusicVolume = evt.newValue;
                GameSettings.ApplyMusicVolume();
            });
            sfxVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.SFXVolume = evt.newValue;
                GameSettings.ApplySFXVolume();
            });
        }
    }
}