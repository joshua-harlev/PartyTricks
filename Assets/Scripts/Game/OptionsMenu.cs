using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionsMenu : MonoBehaviour
  {
      [SerializeField] private UIDocument optionsDocument;

      private VisualElement root;
      private Toggle vSyncToggle;
      private Toggle presetBoardToggle;
      private DropdownField displayModeDropdown;
      private DropdownField resolutionDropdown;
      private DropdownField antiAliasingDropdown;
      private Slider volumeSlider;
      private Slider screenShakeSlider;
      private Button okayButton;
      private Toggle musicToggle;

      private List<(int width, int height)> resolutionList = new();
      private List<string> displayModeOptions = new();
      private bool resolutionChanged;

      private void Awake()
      {
          root = optionsDocument.rootVisualElement;
          root.style.display = DisplayStyle.None;
          GameSettings.Load();
          GameSettings.Apply();
      }

      private void Start()
      {
          vSyncToggle = root.Q<Toggle>("Vsync_Toggle");
          presetBoardToggle = root.Q<Toggle>("Preset_Board_Toggle");
          displayModeDropdown = root.Q<DropdownField>("DisplayMode_Dropdown");
          resolutionDropdown = root.Q<DropdownField>("Resolution_Dropdown");
          antiAliasingDropdown = root.Q<DropdownField>("Anti-Aliasing_Dropdown");
          volumeSlider = root.Q<Slider>("Volume_Slider");
          screenShakeSlider = root.Q<Slider>("Screen_Shake_Slider");
          okayButton = root.Q<Button>("Okay_Button");
          musicToggle = root.Q<Toggle>("Music_Toggle");

          SetUpResolutionList();
          SetUpDisplayModeList();
          SyncUIToSettings();

          vSyncToggle.RegisterValueChangedCallback(evt => GameSettings.VSync = evt.newValue);
          presetBoardToggle.RegisterValueChangedCallback(evt => GameSettings.UsePresetBoard = evt.newValue);
          resolutionDropdown.RegisterValueChangedCallback(evt =>
          {
              var sel = resolutionList[resolutionDropdown.index];
              GameSettings.ResolutionWidth = sel.width;
              GameSettings.ResolutionHeight = sel.height;
              resolutionChanged = true;
          });
          antiAliasingDropdown.RegisterValueChangedCallback(evt =>
              GameSettings.AntiAliasingMode = antiAliasingDropdown.index);
          volumeSlider.RegisterValueChangedCallback(evt =>
          {
              GameSettings.Volume = evt.newValue;
              GameSettings.ApplyVolume();
          });
          screenShakeSlider.RegisterValueChangedCallback(evt => GameSettings.ScreenShakeIntensity = evt.newValue);
          musicToggle.RegisterValueChangedCallback(evt => { GameSettings.MusicEnabled = evt.newValue; GameSettings.ApplyMusic(); });
          displayModeDropdown.RegisterValueChangedCallback(SetDisplayMode);
          okayButton.clicked += OnOkay;
      }

      private void SetUpDisplayModeList() {
          displayModeOptions = new List<string>
          {
              "Fullscreen",
              "Windowed"
          };
          
          displayModeDropdown.choices = displayModeOptions;
      }
      
      private void SetDisplayMode(ChangeEvent<string> evt) {
          switch (evt.newValue) {
              case "Fullscreen":
                  GameSettings.DisplayMode = FullScreenMode.ExclusiveFullScreen;
                  break;
              case "Windowed":
                  GameSettings.DisplayMode = FullScreenMode.Windowed;
                  break;
              default:
                  DebugLogger.LogException(LogChannel.Systems, new InvalidEnumArgumentException("Invalid display mode in GameSettings!"));
                  break;
          }
      }

      private int GetDisplayIndexFromEnum(FullScreenMode displayMode) {
          return displayMode switch
          {
              FullScreenMode.ExclusiveFullScreen => 0,
              FullScreenMode.Windowed => 1,
              _ => 0
          };
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

          resolutionDropdown.choices = resolutionList.Select(r => $"{r.width} x {r.height}").ToList();
      }

      private void SyncUIToSettings()
      {
          vSyncToggle.value = GameSettings.VSync;
          presetBoardToggle.value = GameSettings.UsePresetBoard;
          int currentWidth = Screen.width;
          int currentHeight = Screen.height;
          resolutionList.Sort();
          int resIndex = resolutionList.FindIndex(r =>
              r.width == currentWidth && r.height == currentHeight);
          if (resIndex < 0) {
              resolutionList.Add((currentWidth, currentHeight));
              resolutionDropdown.choices = resolutionList.Select(r => $"{r.width} x {r.height}").ToList();
              resIndex = resolutionList.Count - 1;
          }

          resolutionDropdown.index = resIndex;
          
          GameSettings.ResolutionWidth = currentWidth;
          GameSettings.ResolutionHeight = currentHeight;

          displayModeDropdown.index = GetDisplayIndexFromEnum(GameSettings.DisplayMode);
          
          antiAliasingDropdown.choices = new List<string> { "None", "FXAA", "SMAA" };
          antiAliasingDropdown.index = GameSettings.AntiAliasingMode;

          volumeSlider.lowValue = 0f;
          volumeSlider.highValue = 1f;
          volumeSlider.value = GameSettings.Volume;

          screenShakeSlider.lowValue = 0f;
          screenShakeSlider.highValue = 1f;
          screenShakeSlider.value = GameSettings.ScreenShakeIntensity;
          musicToggle.value = GameSettings.MusicEnabled;
          resolutionChanged = false;
      }

      private void OnOkay()
      {
          GameSettings.Save();
          GameSettings.Apply(resolutionChanged);
          root.style.display = DisplayStyle.None;
      }

      public void Show()
      {
          SyncUIToSettings();
          root.style.display = DisplayStyle.Flex;
      }

      private void OnDestroy()
      {
          if (okayButton != null) okayButton.clicked -= OnOkay;
      }
  }
