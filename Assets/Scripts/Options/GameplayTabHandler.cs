using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Options {
    public class GameplayTabHandler : IOptionsTab {
        private Toggle autoDismissTutorialsToggle;
        private Toggle longerMinigameCountdownsToggle;
        private DropdownField timerLengthDropdown;
        private Toggle presetBoardToggle;

        private List<string> timerLengthOptions = new()
        {
            "Less Time",
            "Default",
            "More Time"
        };
        
        public void Initialize(VisualElement tabRoot) {
            autoDismissTutorialsToggle = tabRoot.Q<Toggle>("Auto_Dismiss_Tutorials_Toggle");
            longerMinigameCountdownsToggle = tabRoot.Q<Toggle>("Longer_Minigame_Countdowns_Toggle");
            timerLengthDropdown = tabRoot.Q<DropdownField>("Timer_Length_Dropdown");
            presetBoardToggle = tabRoot.Q<Toggle>("Preset_Board_Toggle");
            
            SetUpTimerLengthList();
        }

        public void SyncToSettings() {
            autoDismissTutorialsToggle.value = GameSettings.Gameplay.AutoDismissTutorials;
            longerMinigameCountdownsToggle.value = GameSettings.Gameplay.LongerMinigameCountdowns;
            presetBoardToggle.value = GameSettings.Gameplay.UsePresetBoard;
            timerLengthDropdown.index = GameSettings.Gameplay.TimerLengths;
        }

        public void RegisterCallbacks() {
            autoDismissTutorialsToggle.RegisterValueChangedCallback(evt => GameSettings.Gameplay.AutoDismissTutorials = evt.newValue);
            longerMinigameCountdownsToggle.RegisterValueChangedCallback(evt => GameSettings.Gameplay.LongerMinigameCountdowns = evt.newValue);
            timerLengthDropdown.RegisterValueChangedCallback(OnTimerLengthChanged);
            presetBoardToggle.RegisterValueChangedCallback(evt => GameSettings.Gameplay.UsePresetBoard = evt.newValue);
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