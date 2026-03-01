  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.UIElements;
  using System.Linq;

  public class OptionsMenu : MonoBehaviour
  {
      [SerializeField] private UIDocument optionsDocument;

      private VisualElement root;
      private Toggle vSyncToggle;
      private DropdownField resolutionDropdown;
      private DropdownField antiAliasingDropdown;
      private Slider volumeSlider;
      private Slider screenShakeSlider;
      private Button okayButton;

      private List<(int width, int height)> resolutionList = new();

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
          resolutionDropdown = root.Q<DropdownField>("Resolution_Dropdown");
          antiAliasingDropdown = root.Q<DropdownField>("Anti-Aliasing_Dropdown");
          volumeSlider = root.Q<Slider>("Volume_Slider");
          screenShakeSlider = root.Q<Slider>("Screen_Shake_Slider");
          okayButton = root.Q<Button>("Okay_Button");

          SetUpResolutionList();
          SyncUIToSettings();

          vSyncToggle.RegisterValueChangedCallback(evt => GameSettings.VSync = evt.newValue);
          resolutionDropdown.RegisterValueChangedCallback(evt =>
          {
              var sel = resolutionList[resolutionDropdown.index];
              GameSettings.ResolutionWidth = sel.width;
              GameSettings.ResolutionHeight = sel.height;
          });
          antiAliasingDropdown.RegisterValueChangedCallback(evt =>
              GameSettings.AntiAliasingMode = antiAliasingDropdown.index);
          volumeSlider.RegisterValueChangedCallback(evt =>
          {
              GameSettings.Volume = evt.newValue;
              GameSettings.ApplyVolume();
          });
          screenShakeSlider.RegisterValueChangedCallback(evt => GameSettings.ScreenShakeIntensity = evt.newValue);
          okayButton.clicked += OnOkay;
      }

      private void SetUpResolutionList()
      {
          var seen = new HashSet<string>();
          foreach (var res in Screen.resolutions)
          {
              if (seen.Add($"{res.width}x{res.height}"))
                  resolutionList.Add((res.width, res.height));
          }

          if (resolutionList.Count <= 1)
          {
              resolutionList = new List<(int width, int height)>
              {
                  (800, 600), (1280, 720), (1280, 800),
                  (1366, 768), (1600, 900), (1680, 1050),
                  (1920, 1080), (1920, 1200), (2560, 1440),
                  (2560, 1600), (3840, 2160)
              };
          }

          resolutionDropdown.choices = resolutionList.Select(r => $"{r.width} x {r.height}").ToList();
      }

      private void SyncUIToSettings()
      {
          vSyncToggle.value = GameSettings.VSync;

          int resIndex = resolutionList.FindIndex(r =>
              r.width == GameSettings.ResolutionWidth && r.height == GameSettings.ResolutionHeight);
          resolutionDropdown.index = resIndex >= 0 ? resIndex : resolutionList.Count - 1;

          antiAliasingDropdown.choices = new List<string> { "None", "FXAA", "SMAA" };
          antiAliasingDropdown.index = GameSettings.AntiAliasingMode;

          volumeSlider.lowValue = 0f;
          volumeSlider.highValue = 1f;
          volumeSlider.value = GameSettings.Volume;

          screenShakeSlider.lowValue = 0f;
          screenShakeSlider.highValue = 1f;
          screenShakeSlider.value = GameSettings.ScreenShakeIntensity;
      }

      private void OnOkay()
      {
          GameSettings.Save();
          GameSettings.Apply();
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
