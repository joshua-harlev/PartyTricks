using System;
using System.Collections;
using System.Reflection;
using CoreData;
using FMOD.Studio;
using FMODUnity;
using Game;
using Input;
using Minigames.DireDodging;
using Services;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class DireDodgingMinigameManager : MonoBehaviour, IMinigameManager {
    private EventInstance musicInstance;
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    public bool IsDoubleRound { get; private set; }
    private IDireDodgingState currentState;
    public static DireDodgingMinigameManager Instance { get; private set; }
    private IPlayerService playerService;
    private IPowerUpService powerUpService;

    [Header("Minigame Settings")] 
    private int countdownDurationInSeconds;
    private int[] BaseFundsPerRank => MinigamePayouts.GetBaseFundsPerRank();
    [Tooltip("How much shorter should this minigame be from other minigames?")]
    [SerializeField] private int TimerDecreaseAmount = 5;
    [SerializeField] private int ResultsDisplayDurationInSeconds = 5;
    [SerializeField] private EventReference MusicEvent;

    [Header("References")] 
    [SerializeField] private DireDodgingPlayer[] Players = new DireDodgingPlayer[4];
    [SerializeField] private MinigameStartCountdown StartCountdown;
    [SerializeField] private MinigameTimer MinigameTimer;
    [SerializeField] private PlayerCornerDisplay[] PlayerCornerDisplays = new PlayerCornerDisplay[4];
    [SerializeField] private PlacesDisplay PlacesDisplay;
    [SerializeField] private Camera MainCamera;
    
    private float baseGameDurationInSeconds => Mathf.Max(TimerLengths.GetMinigameTimerLengthInSeconds() - TimerDecreaseAmount, 5f);
    private float effectiveGameDurationInSeconds;
    private bool hasBeenInitialized = false;
    private bool gameHasEnded = false;
    public bool GameHasEnded => gameHasEnded;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        
        effectiveGameDurationInSeconds = baseGameDurationInSeconds;
        
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        powerUpService = ServiceLocatorAccessor.GetService<IPowerUpService>();
        this.musicInstance = RuntimeManager.CreateInstance(MusicEvent);
    }

    private void Start() {
        countdownDurationInSeconds = TimerLengths.GetCountdownTimerLengthInSeconds();
        SetUpVariables();
        StartCoroutine(WaitForInitialization());
    }

    private void SetUpVariables() {
        InitializePlayerDisplays();
        PlacesDisplay.Hide();
        StartCountdown.Initialize(countdownDurationInSeconds);
        if (IsDoubleRound) {
            effectiveGameDurationInSeconds *= 2;
        }
        MinigameTimer.Initialize(effectiveGameDurationInSeconds, null);
        for (int i = 0; i < Players.Length; i++) {
            if (Players[i] == null) continue;
            PlayerSlot slot = playerService.PlayerSlots[i];
            if (slot.IsAI) {
                slot.SwapAIHandler<AIDireDodgingInputHandler>();
                var aiHandler = slot.GetAIHandler<AIDireDodgingInputHandler>();
                aiHandler?.SetPlayerContext(Players[i].transform, Camera.main);
            }
            CombatModifiers modifiers = powerUpService.GetCombatModifiers(slot.Profile);
            Players[i].Initialize(i, slot.InputHandler, slot.IsAI, modifiers, IsDoubleRound);
        }
        DireDodgingCameraZoomService.Initialize(Camera.main);
    }

    private void InitializePlayerDisplays() {
        for (int i = 0; i < 4; i++) {
            var slot = playerService.PlayerSlots[i];
            if (slot?.Profile != null) {
                PlayerCornerDisplays[i].Initialize(slot.Profile, PlayerCornerDisplay.DisplayMode.Eliminations);
            }
        }
    }

    private void StartGameFlow() {
        currentState = new DireDodgingCountdownState(StartCountdown);
        currentState.Enter();
    }

    private void ChangeState(IDireDodgingState newState) {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void EnableAllPlayerInput() {
        if(gameHasEnded) return;
        foreach (var player in Players) {
            if (player != null) {
                player.EnableInput();
            }
        }
    }

    public void StartPlayerShooting() {
        foreach (var player in Players) {
            if (player != null) {
                player.StartShooting();
            }
        }
    }
    
    public void TransitionToGameplay() {
        ChangeState(new DireDodgingGameplayState(MinigameTimer, PlayerCornerDisplays, MainCamera));
    }

    public void TransitionToResults(int[] playerPlaces, int[] playerKills) {
        Debug.Log("Game ended. Places: " + string.Join(", ", playerPlaces) + ", kills: " + string.Join(", ", playerKills));
        DireDodgingResultsState resultsState = new DireDodgingResultsState(playerPlaces, playerKills, BaseFundsPerRank, PlacesDisplay, MinigameTimer);
        ChangeState(resultsState);
    }

    private IEnumerator WaitForInitialization() {
        while (!hasBeenInitialized) {
            yield return null;
        }

        StartGameFlow();
    }
    
    public void Initialize(bool isDoubleRound) {
        this.IsDoubleRound = isDoubleRound;
        hasBeenInitialized = true;
        DebugLogger.Log(LogChannel.Systems, $"DireDodgingMinigame initialized. Double round: {isDoubleRound}");
    }

    public void StartMusic() {
        musicInstance.start();
    }

    public void RegisterDeath(int killerID, int killedID) {
        DebugLogger.Log(LogChannel.Systems, $"P{killerID+1} eliminated P{killedID+1}!");
        if (currentState is DireDodgingGameplayState gameplayState) {
            gameplayState.HandlePlayerKill(killerID);
            gameplayState.OnPlayerDeath(killedID);
        }
        else {
            Debug.Log("Wrong state! See DireDodgingMinigameManager, RegisterDeath");
        }
    }

    public void FreezeAllPlayers() {
        gameHasEnded = true;
        foreach (var player in Players) {
            player.Freeze();
        }
    }

    private void Update() {
        if (!hasBeenInitialized) return;
        currentState?.OnUpdate();
    }

    private void OnDestroy() {
        musicInstance.stop(STOP_MODE.IMMEDIATE);
        musicInstance.release();
        DireDodgingCameraZoomService.Cleanup();
        Instance = null;
    }

    private void OnDisable() {
        musicInstance.stop(STOP_MODE.IMMEDIATE);
    }

    public void ReturnAllProjectiles() {
        foreach (var player in Players) {
            player.DestroyVisibleProjectiles();
        }
    }

    public void StartIncreasingIntensity(float remainingTimeInSeconds) {
        foreach (var player in Players) {
            player.StartIncreasingIntensity(remainingTimeInSeconds);
        }
    }

    public void OnGameEnd(PlayerMinigameResult[] playerResults) {
        StartCoroutine(WaitAndEndMinigame(playerResults));
    }

    private IEnumerator WaitAndEndMinigame(PlayerMinigameResult[] playerResults) {
        yield return new WaitForSeconds(ResultsDisplayDurationInSeconds);
        OnMinigameFinished?.Invoke(playerResults);
    }
    
    public void DebugKillPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Players.Length) return;
    
        DireDodgingPlayer player = Players[playerIndex];
        if (player == null) {
            Debug.LogWarning($"Player {playerIndex} is null");
            return;
        }
    
        var dummyProjectile = new GameObject("DummyProjectile").AddComponent<DireDodgingProjectile>();
        dummyProjectile.Initialize(playerIndex, 9999f, 0f, Vector2.zero, false);
        
        player.TakeProjectileDamage(dummyProjectile);
    
        Destroy(dummyProjectile.gameObject);
    }

    public void DebugStunPlayer(int playerIndex) {
        Players[playerIndex].Stun();
    }
}
