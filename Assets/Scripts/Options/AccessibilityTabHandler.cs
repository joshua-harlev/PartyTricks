using UnityEngine.UIElements;

namespace Options {
    public class AccessibilityTabHandler : IOptionsTab {
        private Slider screenShakeSlider;
        private Toggle shopBackgroundMovementToggle;
        
        public void Initialize(VisualElement tabRoot) {
            screenShakeSlider = tabRoot.Q<Slider>("Screen_Shake_Slider");
            shopBackgroundMovementToggle = tabRoot.Q<Toggle>("Shop_Background_Movement_Toggle");
        }

        public void SyncToSettings() {
            screenShakeSlider.lowValue = 0f;
            screenShakeSlider.highValue = 1f;
            screenShakeSlider.value = GameSettings.ScreenShakeIntensity;

            shopBackgroundMovementToggle.value = GameSettings.AnimateClouds;
        }

        public void RegisterCallbacks() {
            screenShakeSlider.RegisterValueChangedCallback(evt => GameSettings.ScreenShakeIntensity = evt.newValue);
            shopBackgroundMovementToggle.RegisterValueChangedCallback(evt => GameSettings.AnimateClouds = evt.newValue);
        }
    }
}