using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Options {
    public class GameplayTabHandler : IOptionsTab {
        private Toggle showTutorialsToggle;
        private Toggle autoDismissTutorialsToggle;
        private Toggle longerMinigameCountdownsToggle;
        private DropdownField timerLengthDropdown;
        private Toggle presetBoardToggle;
        private Toggle showFirstShopToggle;

        private List<string> timerLengthOptions = new()
        {
            "Less Time",
            "Default",
            "More Time"
        };
        
        public void Initialize(VisualElement tabRoot) {
            showTutorialsToggle = tabRoot.Q<Toggle>("Show_Tutorials_Toggle");
            autoDismissTutorialsToggle = tabRoot.Q<Toggle>("Auto_Dismiss_Tutorials_Toggle");
            longerMinigameCountdownsToggle = tabRoot.Q<Toggle>("Longer_Minigame_Countdowns_Toggle");
            timerLengthDropdown = tabRoot.Q<DropdownField>("Timer_Length_Dropdown");
            presetBoardToggle = tabRoot.Q<Toggle>("Preset_Board_Toggle");
            showFirstShopToggle = tabRoot.Q<Toggle>("First_Shop_Toggle");
            SetUpTimerLengthList();
        }

        public void SyncToSettings() {
            showTutorialsToggle.value = GameSettings.Gameplay.ShowTutorials;
            autoDismissTutorialsToggle.value = GameSettings.Gameplay.AutoDismissTutorials;
            longerMinigameCountdownsToggle.value = GameSettings.Gameplay.LongerMinigameCountdowns;
            presetBoardToggle.value = GameSettings.Gameplay.UsePresetBoard;
            timerLengthDropdown.index = GameSettings.Gameplay.TimerLengths;
            showFirstShopToggle.value = GameSettings.Gameplay.ShowFirstShop;
        }

        public void RegisterCallbacks() {
            showTutorialsToggle.RegisterValueChangedCallback(OnShowTutorialsUpdated);
            autoDismissTutorialsToggle.RegisterValueChangedCallback(evt => GameSettings.Gameplay.AutoDismissTutorials = evt.newValue);
            longerMinigameCountdownsToggle.RegisterValueChangedCallback(evt => GameSettings.Gameplay.LongerMinigameCountdowns = evt.newValue);
            timerLengthDropdown.RegisterValueChangedCallback(OnTimerLengthChanged);
            presetBoardToggle.RegisterValueChangedCallback(evt => GameSettings.Gameplay.UsePresetBoard = evt.newValue);
            showFirstShopToggle.RegisterValueChangedCallback(evt => GameSettings.Gameplay.ShowFirstShop = evt.newValue);
        }

        private void OnShowTutorialsUpdated(ChangeEvent<bool> evt) {
            if (evt.newValue) {
                autoDismissTutorialsToggle.SetEnabled(true);
            } else autoDismissTutorialsToggle.SetEnabled(false);
            GameSettings.Gameplay.ShowTutorials = evt.newValue;
        }

        public void Cleanup() { }

        private void OnTimerLengthChanged(ChangeEvent<string> evt) {
            GameSettings.Gameplay.TimerLengths = timerLengthOptions.IndexOf(evt.newValue);
        }

        private void SetUpTimerLengthList() {
            timerLengthDropdown.choices = timerLengthOptions;
        }
    }
}