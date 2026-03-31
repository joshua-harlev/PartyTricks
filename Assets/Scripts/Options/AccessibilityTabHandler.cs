using Services;
using UnityEngine.UIElements;

namespace Options {
    public class AccessibilityTabHandler : IOptionsTab {
        private Slider screenShakeSlider;
        private Toggle shopBackgroundMovementToggle;
        private Toggle tinnitusFilterToggle;
        private Slider tinnitusFilterFrequencySlider;
        private Slider tinnitusFilterGainSlider;
        private Button testToneButton;
        private VisualElement tinnitusFilterControls;
        private ITinnitusFilterService tinnitusFilterService;
        
        public void Initialize(VisualElement tabRoot) {
            screenShakeSlider = tabRoot.Q<Slider>("Screen_Shake_Slider");
            shopBackgroundMovementToggle = tabRoot.Q<Toggle>("Shop_Background_Movement_Toggle");
            tinnitusFilterToggle = tabRoot.Q<Toggle>("Tinnitus_Filter_Toggle");
            tinnitusFilterFrequencySlider = tabRoot.Q<Slider>("Tinnitus_Filter_Frequency_Slider");
            tinnitusFilterGainSlider = tabRoot.Q<Slider>("Tinnitus_Filter_Gain_Slider");
            testToneButton = tabRoot.Q<Button>("Tinnitus_Filter_Test_Tone_button");
            tinnitusFilterControls = tabRoot.Q<VisualElement>("Tinnitus_Filter_Controls");
            tinnitusFilterService = ServiceLocatorAccessor.GetService<ITinnitusFilterService>();
        }

        public void SyncToSettings() {
            screenShakeSlider.lowValue = 0f;
            screenShakeSlider.highValue = 1f;
            screenShakeSlider.value = GameSettings.Accessibility.ScreenShakeIntensity;

            shopBackgroundMovementToggle.value = GameSettings.Accessibility.AnimateClouds;

            tinnitusFilterToggle.value = GameSettings.Accessibility.TinnitusFilterEnabled;

            tinnitusFilterFrequencySlider.lowValue = 5000f;
            tinnitusFilterFrequencySlider.highValue = 16000f;
            tinnitusFilterFrequencySlider.value = GameSettings.Accessibility.TinnitusFilterFrequency;
            
            tinnitusFilterGainSlider.lowValue = -30f;
            tinnitusFilterGainSlider.highValue = 0f;
            tinnitusFilterGainSlider.value = GameSettings.Accessibility.TinnitusFilterGain;
            
            SetTinnitusControlsVisible(GameSettings.Accessibility.TinnitusFilterEnabled);
            
            if(tinnitusFilterService != null && tinnitusFilterService.IsPlayingTestTone) tinnitusFilterService.StopTestTone();
            testToneButton.text = "Play Test Tone";
        }

        public void RegisterCallbacks() {
            screenShakeSlider.RegisterValueChangedCallback(evt => GameSettings.Accessibility.ScreenShakeIntensity = evt.newValue);
            shopBackgroundMovementToggle.RegisterValueChangedCallback(evt => GameSettings.Accessibility.AnimateClouds = evt.newValue);

            tinnitusFilterToggle.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Accessibility.TinnitusFilterEnabled = evt.newValue;
                tinnitusFilterService?.SetEnabled(evt.newValue);
                SetTinnitusControlsVisible(evt.newValue);
            });

            tinnitusFilterFrequencySlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Accessibility.TinnitusFilterFrequency = evt.newValue;
                tinnitusFilterService?.SetFrequency(evt.newValue);
            });

            tinnitusFilterGainSlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Accessibility.TinnitusFilterGain = evt.newValue;
                tinnitusFilterService?.SetGain(evt.newValue);
            });

            testToneButton.clicked += OnTestToneClicked;
        }

        public void Cleanup() {
            testToneButton.clicked -= OnTestToneClicked;
            tinnitusFilterService?.StopTestTone();
        }

        private void OnTestToneClicked() {
            if (tinnitusFilterService == null) return;

            if (tinnitusFilterService.IsPlayingTestTone) {
                tinnitusFilterService.StopTestTone();
                testToneButton.text = "Play Test Tone";
            }
            else {
                tinnitusFilterService.PlayTestTone();
                testToneButton.text = "Stop Test Tone";
            }
        }

        private void SetTinnitusControlsVisible(bool visible) {
            if (visible) {
                tinnitusFilterControls.style.display = DisplayStyle.Flex;
            }
            else {
                tinnitusFilterControls.style.display = DisplayStyle.None;
            }
        }
    }
}