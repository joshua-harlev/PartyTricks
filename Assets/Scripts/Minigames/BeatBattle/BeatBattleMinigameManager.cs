using System;
using FMOD.Studio;
using FMODUnity;
using Minigames.BeatBattle.Core;
using Minigames.BeatBattle.States;
using Services;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class BeatBattleMinigameManager : MonoBehaviour, IMinigameManager {
    [SerializeField] private BeatBattleConfigSO configSO;
    [SerializeField] private MinigameStartCountdown countdown;
    [SerializeField] private EventReference musicEvent;

    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    public bool IsDoubleRound { get; private set; }

    public event Action<int> OnCreationPhaseStarted;
    public event Action<int, BeatBattleChart> OnPlaybackPhaseStarted;
    public event Action<int, int, NoteType> OnNoteCreated;
    public event Action<int, int, float> OnPlayerHit;
    public event Action<int, int> OnPlayerMiss;
    public event Action<int, int[]> OnRoundEnd;
    public event Action<int> OnTransitionStart;
    
    public BeatBattleConfigSO ConfigSO => configSO;
    public RoundState RoundState { get; private set; }
    public RoundScorer Scorer { get; private set; }

    private EventInstance musicInstance;
    private IBeatBattleGameState currentState;
    private IPlayerService playerService;

    private void Awake() {
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
    }

    public void Initialize(bool isDoubleRound) {
        IsDoubleRound = isDoubleRound;

        var config = configSO.GetConfig();
        RoundState = new RoundState(config, Environment.TickCount);
        Scorer = new RoundScorer();

        ChangeState(new BeatBattleCountdownState(this, countdown));
    }

    private void Update() {
        currentState?.OnUpdate();
    }

    public void ChangeState(IBeatBattleGameState newState) {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void StartMusic() {
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    public int GetTimelinePositionInMs() {
        musicInstance.getTimelinePosition(out int positionInMs);
        return positionInMs;
    }

    public IDirectionalTwoButtonInputHandler GetInputHandler(int playerIndex) {
        return playerService.PlayerSlots[playerIndex].InputHandler;
    }
    
    public void InvokeCreationPhaseStarted(int creatorIndex) => OnCreationPhaseStarted?.Invoke(creatorIndex);
    public void InvokePlaybackPhaseStarted(int creatorIndex, BeatBattleChart chart) => OnPlaybackPhaseStarted?.Invoke(creatorIndex, chart);
    public void InvokeNoteCreated(int creatorIndex, int gridSlot, NoteType type) => OnNoteCreated?.Invoke(creatorIndex, gridSlot, type);
    public void InvokePlayerHit(int playerIndex, int noteIndex, float offsetInMs) => OnPlayerHit?.Invoke(playerIndex, noteIndex, offsetInMs);
    public void InvokePlayerMissed(int playerIndex, int noteIndex) => OnPlayerMiss?.Invoke(playerIndex, noteIndex);
    public void InvokeRoundEnded(int roundIndex, int[] scores) => OnRoundEnd?.Invoke(roundIndex, scores);
    public void InvokeTransitionStart(int playerIndex) => OnTransitionStart?.Invoke(playerIndex);
    public void InvokeMinigameFinished(PlayerMinigameResult[] results) => OnMinigameFinished?.Invoke(results);

    private void OnDisable() {
        if (musicInstance.isValid()) {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}
