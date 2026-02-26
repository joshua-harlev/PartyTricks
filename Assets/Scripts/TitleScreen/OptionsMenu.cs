using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    public static float ScreenShakeIntensity = 1f;

    private void Awake() {
        root = optionsDocument.rootVisualElement;
        root.style.display = DisplayStyle.None;
    }

    private void Start() {
        vSyncToggle = root.Q<Toggle>("Vsync_Toggle");
        resolutionDropdown = root.Q<DropdownField>("Resolution_Dropdown");
        antiAliasingDropdown = root.Q<DropdownField>("Anti-Aliasing_Dropdown");
        volumeSlider = root.Q<Slider>("Volume_Slider");
        screenShakeSlider = root.Q<Slider>("Screen_Shake_Slider");
        okayButton = root.Q<Button>("Okay_Button");

        SetUpVSync();
        SetUpResolution();
        SetUpAntiAliasing();
        SetUpVolume();
        SetUpScreenShake();

        okayButton.clicked += Hide;
    }

    private void SetUpVSync() {
        vSyncToggle.value = QualitySettings.vSyncCount > 0;
        vSyncToggle.RegisterValueChangedCallback(evt => 
            QualitySettings.vSyncCount = evt.newValue ? 1 : 0);
    }

    private void SetUpResolution() {
        Resolution[] resolutions = Screen.resolutions;
        var choices = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++) {
            choices.Add($"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRateRatio.numerator}hz");
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height) {
                currentIndex = i;
            }
        }

        resolutionDropdown.choices = choices;
        resolutionDropdown.index = currentIndex;
        resolutionDropdown.RegisterValueChangedCallback(evt => {
            Resolution selected = resolutions[resolutionDropdown.index];
            Screen.SetResolution(selected.width, selected.height, Screen.fullScreen);
        });
    }

    private void SetUpAntiAliasing() {
        var choices = new List<string> { "None", "2x", "4x", "8x" };
        antiAliasingDropdown.choices = choices;

        int current = QualitySettings.antiAliasing;
        antiAliasingDropdown.index = current switch {
            2 => 1,
            4 => 2,
            8 => 3,
            _ => 0
        };

        antiAliasingDropdown.RegisterValueChangedCallback(evt => {
            QualitySettings.antiAliasing = antiAliasingDropdown.index switch {
                1 => 2,
                2 => 4,
                3 => 8,
                _ => 0
            };
        });
    }

    private void SetUpVolume() {
        volumeSlider.lowValue = 0f;
        volumeSlider.highValue = 1f;
        volumeSlider.value = AudioListener.volume;
        volumeSlider.RegisterValueChangedCallback(evt => 
            AudioListener.volume = evt.newValue);
    }

    private void SetUpScreenShake() {
        screenShakeSlider.lowValue = 0f;
        screenShakeSlider.highValue = 1f;
        screenShakeSlider.value = ScreenShakeIntensity;
        screenShakeSlider.RegisterValueChangedCallback(evt => 
            ScreenShakeIntensity = evt.newValue);
    }

    public void Show() {
        root.style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        root.style.display = DisplayStyle.None;
    }

    private void OnDestroy() {
        okayButton.clicked -= Hide;
    }
}