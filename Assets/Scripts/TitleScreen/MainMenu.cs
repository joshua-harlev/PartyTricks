using System.Collections;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour {
    private enum InputDecision {
        MoveUp,
        MoveDown,
        Select,
        Stay
    }
    [SerializeField]
    private UIDocument mainMenu;

    private Button startGameButton;
    private Button optionsButton;
    private Button quitButton;
    private bool hasFocused;
    private InputAction navigateAction;
    private IGameFlowService gameFlowService;
    private Button[] buttons;
    private int focusedIndex;
    private float navigationCooldown;
    private const float NavigationCooldownDurationInSeconds = 0.2f;

    private void Awake() {
        gameFlowService = ServiceLocatorAccessor.GetService<IGameFlowService>();
    }

    private void Start()
    {
        VisualElement root = mainMenu.rootVisualElement;
        startGameButton = root.Query<Button>("StartGameButton");
        optionsButton = root.Query<Button>("OptionsButton");
        quitButton = root.Query<Button>("QuitButton");
        quitButton.clicked += QuitGame;
        startGameButton.clicked += StartGame;
        optionsButton.clicked += ShowOptions;
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");

        buttons = new Button[]
        {
            startGameButton,
            optionsButton,
            quitButton
        };
        
        StartCoroutine(FocusFirstButtonAfterOneFrame());
    }

    private void QuitGame() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void Update() {
        navigationCooldown -= Time.deltaTime;
        if (!hasFocused && navigateAction.ReadValue<Vector2>() != Vector2.zero) {
            FocusFirstButton();
        }
        
        ProcessGamepadInput();
    }

    private void ProcessGamepadInput() {
        var choice = DetermineInputDecisionFromGamepads();
        ExecuteControllerAction(choice);
    }

    private InputDecision DetermineInputDecisionFromGamepads() {
        InputDecision choice = InputDecision.Stay;
        foreach (var gamepad in Gamepad.all) {
            float yValueLeftStick = gamepad.leftStick.y.ReadValue();
            choice = GetInputDecision(yValueLeftStick);
            if (choice != InputDecision.Stay) {
                break;
            }

            float yValueDPad = gamepad.dpad.y.ReadValue();
            choice = GetInputDecision(yValueDPad);
            if (choice != InputDecision.Stay) {
                break;
            }

            bool switchAButtonPressed = gamepad is SwitchProControllerHID && gamepad.buttonEast.wasPressedThisFrame;
            bool otherControllerSelectButtonPressed =
                gamepad is not SwitchProControllerHID && gamepad.buttonSouth.wasPressedThisFrame;
            if (switchAButtonPressed || otherControllerSelectButtonPressed) {
                choice = InputDecision.Select;
                break;
            }
        }

        return choice;
    }

    private void ExecuteControllerAction(InputDecision choice) {
        switch (choice) {
            case InputDecision.MoveUp:
                if (navigationCooldown > 0) break;
                focusedIndex--;
                focusedIndex = Mathf.Clamp(focusedIndex, 0, buttons.Length - 1);
                buttons[focusedIndex].Focus();
                navigationCooldown = NavigationCooldownDurationInSeconds;
                break;
            case InputDecision.MoveDown:
                if (navigationCooldown > 0) break;
                focusedIndex++;
                focusedIndex = Mathf.Clamp(focusedIndex, 0, buttons.Length - 1);
                buttons[focusedIndex].Focus();
                navigationCooldown = NavigationCooldownDurationInSeconds;
                break;
            case InputDecision.Select:
                using (var navigationSubmitEvent = NavigationSubmitEvent.GetPooled()) {
                    navigationSubmitEvent.target = buttons[focusedIndex];
                    buttons[focusedIndex].SendEvent(navigationSubmitEvent);
                }
                break;
            case InputDecision.Stay:
            default:
                break;
        }
    }

    private InputDecision GetInputDecision(float yValue) {
        switch (yValue) {
            case > 0.5f:
                return InputDecision.MoveUp;
            case < -0.5f:
                return InputDecision.MoveDown;
            default:
                return InputDecision.Stay;
        }
    }

    private IEnumerator FocusFirstButtonAfterOneFrame() {
        yield return null;
        FocusFirstButton();
    }

    private void FocusFirstButton() {
        if (startGameButton != null) {
            startGameButton.Focus();
        }

        hasFocused = true;
    }

    private void StartGame() {
        if (gameFlowService != null) {
            gameFlowService.StartGame();
        }
        else {
            Debug.LogError("MainMenu: GameFlowManager not found.");
        }
    }

    private void ShowOptions() {
        Debug.Log("NOT IMPLEMENTED YET");
        throw new System.NotImplementedException();
    }
}
