using UnityEngine.UIElements;

namespace Options {
    public class MiscTabHandler : IOptionsTab {
        private Toggle randomizeCoinSpinDirectionToggle;
        private Toggle showPlayerLabelToggle;
        
        public void Initialize(VisualElement tabRoot) {
            randomizeCoinSpinDirectionToggle = tabRoot.Q<Toggle>("Coin_Spin_Randomization_Toggle");
            showPlayerLabelToggle = tabRoot.Q<Toggle>("Player_Label_Visibility_Toggle");
        }

        public void SyncToSettings() {
            randomizeCoinSpinDirectionToggle.value = GameSettings.Misc.RandomizeCoinSpinDirection;
            showPlayerLabelToggle.value = GameSettings.Misc.ShowPlayerLabels;
        }

        public void RegisterCallbacks() {
            randomizeCoinSpinDirectionToggle.RegisterValueChangedCallback(evt => GameSettings.Misc.RandomizeCoinSpinDirection = evt.newValue);
            showPlayerLabelToggle.RegisterValueChangedCallback(evt => GameSettings.Misc.ShowPlayerLabels = evt.newValue);
        }
    }
}