using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using Services;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class MainMenu : MonoBehaviour {
    [SerializeField] private EventReference musicEvent;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text connectedPlayersLabel;
    [SerializeField] private MenuSoundConfigSO menuSoundConfig;

    private EventInstance musicInstance;
    private Button[] buttons;
    private int focusedIndex;
    private float lastNavigateTime;
    private const float NavigationCooldownSeconds = 0.2f;
    private IGameFlowService gameFlowService;
    private IPlayerService playerService;
    private readonly List<InputAction> subscribedSubmitActions = new();
    private readonly List<InputAction> gamepadNavigateActions = new();
    private Coroutine intensityCoroutine;
    private InputSystemUIInputModule inputModule;
    private InputActionReference cachedMoveAction;
    private InputActionReference cachedSubmitAction;

    private void Awake() {
        gameFlowService = ServiceLocatorAccessor.GetService<IGameFlowService>();
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
    }

    private void Start()
    {
        if (!musicEvent.IsNull) {
            musicInstance.start();
            intensityCoroutine = StartCoroutine(IncreaseIntensityOverTime());
        }

        startGameButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(ShowOptions);
        quitButton.onClick.AddListener(QuitGame);

        buttons = new [] {
            startGameButton,
            optionsButton,
            quitButton
        };

        playerService.OnPlayerJoined += HandlePlayerJoined;

        foreach (var playerSlot in playerService.PlayerSlots) {
            if(playerSlot.IsOccupied && !playerSlot.IsAI) HandlePlayerJoined(playerSlot.SlotIndex);
        }

        inputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        if (inputModule != null) {
            cachedMoveAction = inputModule.move;
            cachedSubmitAction = inputModule.submit;
            inputModule.move = null;
            inputModule.submit = null;
        }

        StartCoroutine(FocusFirstButtonAfterOneFrame());
        SubscribeToGamepadActions();
    }

    private IEnumerator IncreaseIntensityOverTime() {
        int amountOfTimeToTakeInSeconds = 10;
        int elapsedTimeInSeconds = 0;
        float targetIntensity = 1f;
        while (elapsedTimeInSeconds < amountOfTimeToTakeInSeconds) {
            elapsedTimeInSeconds++;
            float intensity = Mathf.Lerp(0f, targetIntensity, (float)elapsedTimeInSeconds / amountOfTimeToTakeInSeconds);
            musicInstance.setParameterByName("Intensity", intensity);
            yield return new WaitForSeconds(1);
        }
        intensityCoroutine = null;
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
        buttons[focusedIndex].onClick.Invoke();
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
            EventSystem.current.SetSelectedGameObject(buttons[focusedIndex].gameObject);
            RuntimeManager.PlayOneShot(buttons[focusedIndex].name);
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
        EditorApplication.isPlaying = false;
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
            EventSystem.current.SetSelectedGameObject(startGameButton.gameObject);
        }
        focusedIndex = 0;
    }

    private void StartGame() {
        if (gameFlowService != null) {
            gameFlowService.StartGame();
            if(intensityCoroutine != null) StopCoroutine(intensityCoroutine);
            musicInstance.setParameterByName("Intensity", 2f);
        }
        else {
            Debug.LogError("MainMenu: GameFlowManager not found.");
        }
    }

    private void ShowOptions() {
        FindFirstObjectByType<OptionsMenu>()?.Show();
    }

    private void OnDestroy() {
        playerService.OnPlayerJoined -= HandlePlayerJoined;
        startGameButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        foreach (var action in subscribedSubmitActions) {
            action.performed -= OnGamepadSubmitPerformed;
        }
        subscribedSubmitActions.Clear();
        gamepadNavigateActions.Clear();
        if (inputModule != null) {
            inputModule.move = cachedMoveAction;
            inputModule.submit = cachedSubmitAction;
        }
        if (musicInstance.isValid()) {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
        }
    }
}
