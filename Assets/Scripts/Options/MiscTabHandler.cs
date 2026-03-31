using UnityEngine.UIElements;

namespace Options {
    public class MiscTabHandler : IOptionsTab {
        private Toggle randomizeCoinSpinDirectionToggle;
        
        public void Initialize(VisualElement tabRoot) {
            randomizeCoinSpinDirectionToggle = tabRoot.Q<Toggle>("Coin_Spin_Randomization_Toggle");
        }

        public void SyncToSettings() {
            randomizeCoinSpinDirectionToggle.value = GameSettings.RandomizeCoinSpinDirection;
        }

        public void RegisterCallbacks() {
            randomizeCoinSpinDirectionToggle.RegisterValueChangedCallback(evt => GameSettings.RandomizeCoinSpinDirection = evt.newValue);
        }
    }
}