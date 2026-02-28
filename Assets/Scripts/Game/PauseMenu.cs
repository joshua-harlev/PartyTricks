using System.Collections;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour {
    private Button resumeButton;
    private Button optionsButton;
    private Button returnToMenuButton;
    private PauseService pauseService;
    private InputAction cancelAction;
    private InputAction navigateAction;
    private bool hasFocused;
    private IPlayerService playerService;
    private OptionsMenu optionsMenu;
    [SerializeField] private UIDocument pauseMenu;
    
    public void Initialize(PauseService service) {
        optionsMenu = FindFirstObjectByType<OptionsMenu>();
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        VisualElement root = pauseMenu.rootVisualElement;
        pauseService = service;
        resumeButton = root.Q<Button>("ResumeButton");
        optionsButton = root.Q<Button>("OptionsButton");
        returnToMenuButton = root.Q<Button>("ReturnToMenuButton");
        if (resumeButton != null) {
            resumeButton.clicked += OnResumeClicked;
        }

        if (optionsButton != null) {
            optionsButton.clicked += OnOptionsClicked;
        }

        if (returnToMenuButton != null) {
            returnToMenuButton.clicked += OnReturnToMenuClicked;
        }
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
        StartCoroutine(FocusFirstButtonAfterOneFrame());
    }

    private void Update() {
        if (cancelAction != null && cancelAction.WasPressedThisFrame()) {
            OnResumeClicked();
        }
        if (!hasFocused && navigateAction != null && navigateAction.ReadValue<Vector2>() != Vector2.zero) {
            FocusFirstButton();
        }
    }
    
    private IEnumerator FocusFirstButtonAfterOneFrame() {
        yield return null;
        FocusFirstButton();
    }

    private void FocusFirstButton() {
        if (resumeButton != null) {
            resumeButton.Focus();
        }

        hasFocused = true;
    }
    

    private void OnReturnToMenuClicked() {
        foreach (var slot in playerService.PlayerSlots) {
            slot.Profile.Reset();
        }
        pauseService.Resume();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnOptionsClicked() {
        if (optionsMenu != null) optionsMenu.Show();
    }

    private void OnResumeClicked() {
        pauseService.Resume();
    }

    private void OnDestroy() {
        if (resumeButton != null) {
            resumeButton.clicked -= OnResumeClicked;
        }

        if (optionsButton != null) {
            optionsButton.clicked -= OnOptionsClicked;
        }

        if (returnToMenuButton != null) {
            returnToMenuButton.clicked -= OnReturnToMenuClicked;
        }
        cancelAction = null;
    }
}
