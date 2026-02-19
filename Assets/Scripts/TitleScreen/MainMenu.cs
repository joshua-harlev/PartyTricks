using System.Collections;
using System.Collections.Generic;
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
    private Label connectedPlayersLabel;
    private IPlayerService playerService;

    private void Awake() {
        gameFlowService = ServiceLocatorAccessor.GetService<IGameFlowService>();
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
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
        connectedPlayersLabel = root.Q<Label>("ConnectedPlayersLabel");

        buttons = new []
        {
            startGameButton,
            optionsButton,
            quitButton
        };
        
        playerService.OnPlayerJoined += HandlePlayerJoined;

        foreach (var playerSlot in playerService.PlayerSlots) {
            if(playerSlot.IsOccupied && !playerSlot.IsAI) HandlePlayerJoined(playerSlot.SlotIndex);
        }
        
        StartCoroutine(FocusFirstButtonAfterOneFrame());
    }

    private void HandlePlayerJoined(int playerIndex, PlayerProfile profile = null) {
        UpdateConnectedPlayersText();
    }

    private void UpdateConnectedPlayersText() {
        var connectedPlayers = new List<string>();
        for (int i = 0; i < playerService.PlayerSlots.Count; i++) {
            if (playerService.PlayerSlots[i].IsOccupied && !playerService.PlayerSlots[i].IsAI) {
                connectedPlayers.Add($"P{i+1}");
            }
        }
        connectedPlayersLabel.text = connectedPlayers.Count > 0 ?
            "Connected: " + string.Join(", ", connectedPlayers) 
            : string.Empty;
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
    }

    private void OnDestroy() {
        playerService.OnPlayerJoined -= HandlePlayerJoined;   
        quitButton.clicked -= QuitGame;
        startGameButton.clicked -= StartGame;
        optionsButton.clicked -= ShowOptions;
    }
}
