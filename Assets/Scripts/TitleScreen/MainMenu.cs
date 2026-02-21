using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour {
    [SerializeField]
    private UIDocument mainMenu;

    private Button startGameButton;
    private Button optionsButton;
    private Button quitButton;
    private IGameFlowService gameFlowService;
    private Button[] buttons;
    private int focusedIndex;
    private float lastNavigateTime;
    private const float NavigationCooldownSeconds = 0.2f;
    private Label connectedPlayersLabel;
    private IPlayerService playerService;
    private readonly List<InputAction> subscribedSubmitActions = new();
    private readonly List<InputAction> gamepadNavigateActions = new();

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
        connectedPlayersLabel = root.Q<Label>("ConnectedPlayersLabel");

        buttons = new [] {
            startGameButton,
            optionsButton,
            quitButton
        };

        playerService.OnPlayerJoined += HandlePlayerJoined;

        foreach (var playerSlot in playerService.PlayerSlots) {
            if(playerSlot.IsOccupied && !playerSlot.IsAI) HandlePlayerJoined(playerSlot.SlotIndex);
        }
        
        StartCoroutine(FocusFirstButtonAfterOneFrame());
        SubscribeToGamepadActions();
    }

    private void SubscribeToGamepadActions() {
        for (int i = 0; i < playerService.PlayerSlots.Count; i++) {
            SubscribeGamepadActionsForPlayer(i);
        }
    }

    private void SubscribeGamepadActionsForPlayer(int playerIndex) {
        var slot = playerService.PlayerSlots[playerIndex];
        if (!slot.IsOccupied || slot.PlayerInput == null) return;
        if (slot.PlayerInput.devices.Any(device => device is Keyboard || device is Mouse)) return;

        var submitAction = slot.PlayerInput.actions.FindAction("UI/Submit");
        if (submitAction != null) {
            submitAction.performed += OnGamepadSubmitPerformed;
            subscribedSubmitActions.Add(submitAction);
        }

        var navigateAction = slot.PlayerInput.actions.FindAction("UI/Navigate");
        if (navigateAction != null) {
            gamepadNavigateActions.Add(navigateAction);
        }
    }

    private void OnGamepadSubmitPerformed(InputAction.CallbackContext context) {
        if (focusedIndex < 0 || focusedIndex >= buttons.Length) return;
        using (var evt = NavigationSubmitEvent.GetPooled()) {
            evt.target = buttons[focusedIndex];
            buttons[focusedIndex].SendEvent(evt);
        }
    }

    private void Update() {
        if (Time.time - lastNavigateTime < NavigationCooldownSeconds) return;

        foreach (var action in gamepadNavigateActions) {
            var movementVector = action.ReadValue<Vector2>();
            if (Mathf.Abs(movementVector.y) < 0.5f || Mathf.Abs(movementVector.y) <= Mathf.Abs(movementVector.x)) continue;

            if (movementVector.y > 0) {
                focusedIndex = Mathf.Max(0, focusedIndex - 1);
            } else {
                focusedIndex = Mathf.Min(buttons.Length - 1, focusedIndex + 1);
            }
            buttons[focusedIndex].Focus();
            lastNavigateTime = Time.time;
            break;
        }
    }

    private void HandlePlayerJoined(int playerIndex, PlayerProfile profile = null) {
        UpdateConnectedPlayersText();
        SubscribeGamepadActionsForPlayer(playerIndex);
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

    private IEnumerator FocusFirstButtonAfterOneFrame() {
        yield return null;
        FocusFirstButton();
    }

    private void FocusFirstButton() {
        if (startGameButton != null) {
            startGameButton.Focus();
        }
        focusedIndex = 0;
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
        foreach (var action in subscribedSubmitActions) {
            action.performed -= OnGamepadSubmitPerformed;
        }
        subscribedSubmitActions.Clear();
        gamepadNavigateActions.Clear();
    }
}
