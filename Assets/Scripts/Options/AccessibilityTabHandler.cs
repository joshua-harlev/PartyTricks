using Services;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Slider = UnityEngine.UIElements.Slider;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Options {
    public class AccessibilityTabHandler : IOptionsTab {
        private Slider screenShakeSlider;
        private Toggle shopBackgroundMovementToggle;
        private Toggle increaseBackgroundVisibilityToggle;
        private Toggle disableParallaxToggle;
        private Toggle tinnitusFilterToggle;
        private Slider tinnitusFilterFrequencySlider;
        private Slider tinnitusFilterGainSlider;
        private Button testToneButton;
        private VisualElement tinnitusFilterControls;
        private ITinnitusFilterService tinnitusFilterService;
        private Label tinnitusFilterText;
        private Toggle oneHandedModeToggle;
        
        public void Initialize(VisualElement tabRoot) {
            screenShakeSlider = tabRoot.Q<Slider>("Screen_Shake_Slider");
            shopBackgroundMovementToggle = tabRoot.Q<Toggle>("Shop_Background_Movement_Toggle");
            increaseBackgroundVisibilityToggle = tabRoot.Q<Toggle>("Increase_Background_Visibility_Toggle");
            disableParallaxToggle = tabRoot.Q<Toggle>("Disable_Parallax_Toggle");
            oneHandedModeToggle = tabRoot.Q<Toggle>("One_Handed_Mode_Toggle");
            tinnitusFilterToggle = tabRoot.Q<Toggle>("Tinnitus_Filter_Toggle");
            tinnitusFilterFrequencySlider = tabRoot.Q<Slider>("Tinnitus_Filter_Frequency_Slider");
            tinnitusFilterGainSlider = tabRoot.Q<Slider>("Tinnitus_Filter_Gain_Slider");
            testToneButton = tabRoot.Q<Button>("Tinnitus_Filter_Test_Tone_button");
            tinnitusFilterControls = tabRoot.Q<VisualElement>("Tinnitus_Filter_Controls");
            tinnitusFilterService = ServiceLocatorAccessor.GetService<ITinnitusFilterService>();
            tinnitusFilterText = tabRoot.Q<Label>("Tinnitus_Filter_Labels");
        }

        public void SyncToSettings() {
            screenShakeSlider.lowValue = 0f;
            screenShakeSlider.highValue = 1f;
            screenShakeSlider.value = GameSettings.Accessibility.ScreenShakeIntensity;

            shopBackgroundMovementToggle.value = GameSettings.Accessibility.AnimateClouds;
            increaseBackgroundVisibilityToggle.value = GameSettings.Accessibility.IncreaseBackgroundVisibility;
            disableParallaxToggle.value = GameSettings.Accessibility.DisableParallax;
            
            oneHandedModeToggle.value = GameSettings.Accessibility.OneHandedMode;

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
            
            UpdateTinnitusFilterLabel();
        }

        public void RegisterCallbacks() {
            screenShakeSlider.RegisterValueChangedCallback(evt => GameSettings.Accessibility.ScreenShakeIntensity = evt.newValue);
            shopBackgroundMovementToggle.RegisterValueChangedCallback(evt => GameSettings.Accessibility.AnimateClouds = evt.newValue);
            increaseBackgroundVisibilityToggle.RegisterValueChangedCallback(evt => GameSettings.Accessibility.IncreaseBackgroundVisibility = evt.newValue);
            disableParallaxToggle.RegisterValueChangedCallback(evt => GameSettings.Accessibility.DisableParallax = evt.newValue);
            oneHandedModeToggle.RegisterValueChangedCallback(evt => GameSettings.Accessibility.OneHandedMode = evt.newValue);

            tinnitusFilterToggle.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Accessibility.TinnitusFilterEnabled = evt.newValue;
                tinnitusFilterService?.SetEnabled(evt.newValue);
                SetTinnitusControlsVisible(evt.newValue);
                UpdateTinnitusFilterLabel();
            });

            tinnitusFilterFrequencySlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Accessibility.TinnitusFilterFrequency = evt.newValue;
                tinnitusFilterService?.SetFrequency(evt.newValue);
                UpdateTinnitusFilterLabel();
            });

            tinnitusFilterGainSlider.RegisterValueChangedCallback(evt =>
            {
                GameSettings.Accessibility.TinnitusFilterGain = evt.newValue;
                tinnitusFilterService?.SetGain(evt.newValue);
                UpdateTinnitusFilterLabel();
            });

            testToneButton.clicked += OnTestToneClicked;
        }

        private void UpdateTinnitusFilterLabel() {
            tinnitusFilterText.text =
                $"Current Frequency: {GameSettings.Accessibility.TinnitusFilterFrequency:F0} Hz, Current Gain: {GameSettings.Accessibility.TinnitusFilterGain:F0} dB";
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
                if (testToneButton.ClassListContains(".on")) {
                    testToneButton.RemoveFromClassList(".on");
                }
                testToneButton.Blur();
            }
            else {
                tinnitusFilterService.PlayTestTone();
                testToneButton.text = "Stop Test Tone";
                testToneButton.AddToClassList(".on");
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