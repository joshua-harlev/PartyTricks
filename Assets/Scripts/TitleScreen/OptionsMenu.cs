using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering.Universal;
using System.Linq;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private UIDocument optionsDocument;
    private static OptionsMenu instance;

    private VisualElement root;
    private Toggle vSyncToggle;
    private DropdownField resolutionDropdown;
    private DropdownField antiAliasingDropdown;
    private Slider volumeSlider;
    private Slider screenShakeSlider;
    private Button okayButton;

    public static float ScreenShakeIntensity = 1f;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
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
        vSyncToggle.RegisterValueChangedCallback(evt => {
            QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
            Debug.Log("VSync set to: " + QualitySettings.vSyncCount);
        });
    }

    private void SetUpResolution() {
        Resolution[] systemResolutions = Screen.resolutions;

        var seen = new HashSet<string>();
        var uniqueResolutions = new List<Resolution>();
        foreach (var res in systemResolutions) {
            if (seen.Add($"{res.width}x{res.height}")) {
                uniqueResolutions.Add(res);
            }
        }

        if (uniqueResolutions.Count <= 1) { // This should never be called, but just in case a fallback exists
            var fallback = new List<(int width, int height)> {
                (800, 600),
                (1280, 720), (1280, 800), 
                (1366, 768), (1600, 900), 
                (1680, 1050), (1920, 1080),
                (1920, 1200), (2560, 1440), 
                (2560, 1600), (3840, 2160)
            };
            resolutionDropdown.choices = fallback.Select(r => $"{r.width} x {r.height}").ToList();
            resolutionDropdown.index = fallback.FindIndex(r =>
                r.width == Display.main.systemWidth &&
                r.height == Display.main.systemHeight);
            if (resolutionDropdown.index == -1) resolutionDropdown.index = fallback.Count - 1;
            resolutionDropdown.RegisterValueChangedCallback(evt => {
                var selected = fallback[resolutionDropdown.index];
                Screen.SetResolution(selected.width, selected.height, Screen.fullScreen);
            });
            return;
        }

        var choices = uniqueResolutions.Select(r => $"{r.width} x {r.height}").ToList();

        int currentIndex = uniqueResolutions.FindIndex(r =>
            r.width == Display.main.systemWidth &&
            r.height == Display.main.systemHeight);
        if (currentIndex == -1) currentIndex = uniqueResolutions.Count - 1;

        resolutionDropdown.choices = choices;
        resolutionDropdown.index = currentIndex;

        resolutionDropdown.RegisterValueChangedCallback(evt => {
            var selected = uniqueResolutions[resolutionDropdown.index];
            Screen.SetResolution(selected.width, selected.height, Screen.fullScreen);
        });
    }

    private void SetUpAntiAliasing() {
        var choices = new List<string> { "None", "FXAA", "SMAA" };
        antiAliasingDropdown.choices = choices;

        RefreshAntiAliasingFromCurrentCamera();

        antiAliasingDropdown.RegisterValueChangedCallback(evt => {
            ApplyAntiAliasing(antiAliasingDropdown.index);
        });
    }
    
    private void ApplyAntiAliasing(int index) {
        var mode = index switch {
            1 => AntialiasingMode.FastApproximateAntialiasing,
            2 => AntialiasingMode.SubpixelMorphologicalAntiAliasing,
            _ => AntialiasingMode.None
        };

        foreach (var cam in FindObjectsByType<Camera>(FindObjectsSortMode.None)) {
            var cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData != null) {
                cameraData.antialiasing = mode;
            }
        }
    }
    
    private void RefreshAntiAliasingFromCurrentCamera() {
        var cam = FindFirstObjectByType<Camera>();
        if (cam == null) return;
    
        var cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null) return;

        antiAliasingDropdown.index = cameraData.antialiasing switch {
            AntialiasingMode.FastApproximateAntialiasing => 1,
            AntialiasingMode.SubpixelMorphologicalAntiAliasing => 2,
            _ => 0
        };
    }

    private void SetUpVolume() {
        volumeSlider.lowValue = 0f;
        volumeSlider.highValue = 1f;
        FMODUnity.RuntimeManager.GetBus("bus:/").getVolume(out float currentVolume);
        volumeSlider.value = currentVolume;
        volumeSlider.RegisterValueChangedCallback(evt => {
            FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(evt.newValue);
        });
    }

    private void SetUpScreenShake() {
        screenShakeSlider.lowValue = 0f;
        screenShakeSlider.highValue = 1f;
        screenShakeSlider.value = ScreenShakeIntensity;
        screenShakeSlider.RegisterValueChangedCallback(evt => 
            ScreenShakeIntensity = evt.newValue);
    }
    
    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    
    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (antiAliasingDropdown == null) return;
        ApplyAntiAliasing(antiAliasingDropdown.index);
    }
    
    public void Show() {
        RefreshAntiAliasingFromCurrentCamera();
        root.style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        root.style.display = DisplayStyle.None;
    }

    private void OnDestroy() {
        okayButton.clicked -= Hide;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}