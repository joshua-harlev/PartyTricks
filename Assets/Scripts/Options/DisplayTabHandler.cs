using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Debug;
using UnityEngine;
using UnityEngine.UIElements;

namespace Options {
    public class DisplayTabHandler : IOptionsTab {
        public bool DisplayOptionChanged => displayOptionChanged;
        
        private Toggle vSyncToggle;
        private Toggle bloomToggle;
        private DropdownField displayModeDropdown;
        private DropdownField resolutionDropdown;
        private DropdownField antiAliasingDropdown;
        
        private bool displayOptionChanged;
        private List<(int width, int height)> resolutionList = new();
        private List<string> displayModeOptions = new();
        
        public void Initialize(VisualElement tabRoot) {
            bloomToggle = tabRoot.Q<Toggle>("Bloom_Toggle");
            vSyncToggle = tabRoot.Q<Toggle>("Vsync_Toggle");
            displayModeDropdown = tabRoot.Q<DropdownField>("DisplayMode_Dropdown");
            resolutionDropdown = tabRoot.Q<DropdownField>("Resolution_Dropdown");
            antiAliasingDropdown = tabRoot.Q<DropdownField>("Anti-Aliasing_Dropdown");
            SetUpResolutionList();
            SetUpDisplayModeList();
        }

        public void SyncToSettings() {
            vSyncToggle.value = GameSettings.Display.VSync;
            bloomToggle.value = GameSettings.Display.Bloom;
            
            int currentWidth = Screen.width;
            int currentHeight = Screen.height;
          
            resolutionList.Sort();
            int resIndex = resolutionList.FindIndex(r =>
                r.width == currentWidth && r.height == currentHeight);
            if (resIndex < 0) {
                resolutionList.Add((currentWidth, currentHeight));
                resolutionList.Sort();
                resIndex = resolutionList.FindIndex(r=> r.width == currentWidth && r.height == currentHeight);
            }
          
            resolutionDropdown.choices = resolutionList.Select(r => $"{r.width} x {r.height}").ToList();

            resolutionDropdown.index = resIndex;
          
            GameSettings.Display.ResolutionWidth = currentWidth;
            GameSettings.Display.ResolutionHeight = currentHeight;

            displayModeDropdown.index = GetDisplayIndexFromEnum(GameSettings.Display.DisplayMode);
          
            antiAliasingDropdown.choices = new List<string> { "None", "FXAA", "SMAA" };
            antiAliasingDropdown.index = GameSettings.Display.AntiAliasingMode;
            
            displayOptionChanged = false;
        }

        public void RegisterCallbacks() {
            vSyncToggle.RegisterValueChangedCallback(evt => GameSettings.Display.VSync = evt.newValue);
            bloomToggle.RegisterValueChangedCallback(evt => GameSettings.Display.Bloom = evt.newValue);
            resolutionDropdown.RegisterValueChangedCallback(evt =>
            {
                var sel = resolutionList[resolutionDropdown.index];
                GameSettings.Display.ResolutionWidth = sel.width;
                GameSettings.Display.ResolutionHeight = sel.height;
                displayOptionChanged = true;
            });
            antiAliasingDropdown.RegisterValueChangedCallback(evt =>
                GameSettings.Display.AntiAliasingMode = antiAliasingDropdown.index);
            displayModeDropdown.RegisterValueChangedCallback(SetDisplayMode);
        }

        public void Cleanup() { }

        private void SetUpDisplayModeList() {
            displayModeOptions = new List<string>
            {
                "Fullscreen",
                "Windowed"
            };
          
            displayModeDropdown.choices = displayModeOptions;
        }
      
        private void SetDisplayMode(ChangeEvent<string> evt) {
            displayOptionChanged = true;
            switch (evt.newValue) {
                case "Fullscreen":
                    GameSettings.Display.DisplayMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case "Windowed":
                    GameSettings.Display.DisplayMode = FullScreenMode.Windowed;
                    break;
                default:
                    DebugLogger.LogException(LogChannel.Systems, new InvalidEnumArgumentException("Invalid display mode in GameSettings!"));
                    break;
            }
        }
        
        private void SetUpResolutionList()
        {
            var seen = new HashSet<string>();
            foreach (var res in Screen.resolutions)
            {
                if (seen.Add($"{res.width}x{res.height}"))
                    resolutionList.Add((res.width, res.height));
            }
          
            var fallbackResolutionList = new List<(int width, int height)>
            {
                (800, 600), (1280, 720), (1280, 800),
                (1366, 768), (1600, 900), (1680, 1050),
                (1920, 1080), (1920, 1200), (2560, 1440),
                (2560, 1600), (3840, 2160)
            };

            foreach (var resolution in fallbackResolutionList) {
                if (seen.Add($"{resolution.width}x{resolution.height}")) resolutionList.Add((resolution.width, resolution.height));
            }

            resolutionList.Sort();
            resolutionDropdown.choices = resolutionList.Select(r => $"{r.width} x {r.height}").ToList();
        }
        
        private int GetDisplayIndexFromEnum(FullScreenMode displayMode) {
            return displayMode switch
            {
                FullScreenMode.ExclusiveFullScreen => 0,
                FullScreenMode.Windowed => 1,
                _ => 0
            };
        }
    }
}