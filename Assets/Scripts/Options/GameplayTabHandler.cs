using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Options {
    public class GameplayTabHandler : IOptionsTab {
        private DropdownField timerLengthDropdown;
        private Toggle presetBoardToggle;

        private List<string> timerLengthOptions = new()
        {
            "Less Time",
            "Default",
            "More Time"
        };
        
        public void Initialize(VisualElement tabRoot) {
            timerLengthDropdown = tabRoot.Q<DropdownField>("Timer_Length_Dropdown");
            presetBoardToggle = tabRoot.Q<Toggle>("Preset_Board_Toggle");
            
            SetUpTimerLengthList();
        }

        public void SyncToSettings() {
            presetBoardToggle.value = GameSettings.Gameplay.UsePresetBoard;
            timerLengthDropdown.index = GameSettings.Gameplay.TimerLengths;
        }

        public void RegisterCallbacks() {
            timerLengthDropdown.RegisterValueChangedCallback(OnTimerLengthChanged);
            presetBoardToggle.RegisterValueChangedCallback(evt => GameSettings.Gameplay.UsePresetBoard = evt.newValue);
        }
        
        private void OnTimerLengthChanged(ChangeEvent<string> evt) {
            GameSettings.Gameplay.TimerLengths = timerLengthOptions.IndexOf(evt.newValue);
        }

        private void SetUpTimerLengthList() {
            timerLengthDropdown.choices = timerLengthOptions;
        }
    }
}