using System;
using System.Collections;
using Debug;
using Game;
using Minigames.Swinging.Core.PlayerStateMachine;
using Minigames.Swinging.States;
using Minigames.Utilities;
using Player;
using Services;
using UnityEngine;

namespace Minigames.Swinging {
    public class VineSwingingMinigameManager : MonoBehaviour, IMinigameManager {
        public event Action<PlayerMinigameResult[]> OnMinigameFinished;
        public bool IsDoubleRound { get; set; }
        
        public VineSwingingCountdownState VineSwingingCountdownState { get; private set; }
        public VineSwingingGameplayState VineSwingingGameplayState { get; private set; }
        public VineSwingingResultsState VineSwingingResultsState { get; private set; }

        [Header("Minigame Settings")] 
        private int countdownDurationInSeconds;
        private int[] fundsPerRank => MinigamePayouts.GetBaseFundsPerRank();
        [SerializeField] private float resultsDisplayDurationInSeconds = 5f;
        [SerializeField] private int vineCount = 20;
        [SerializeField] private float vineAnchorY = 4f;
        [SerializeField] private VineSwingingAIConfigSO aiConfig;

        [Header("Sweet Spot Hint")] 
        [SerializeField] private float hintSuccessDecrement = (1f/4f); // 4 advances to fully hide
        [SerializeField] private float hintFailureIncrement = 0.5f; // 2 falls to fully restore
        [SerializeField] private float hintFadeSpeed = 2f;
        
        public float HintSuccessDecrement => hintSuccessDecrement;
        public float HintFailureIncrement => hintFailureIncrement;
        public float HintFadeSpeed => hintFadeSpeed;

        [Header("References")] 
        [SerializeField] private MinigameStartCountdown startCountdown;
        [SerializeField] private VineSwingingMusic music;
        [SerializeField] private VineSwingingPlayers players;
        [SerializeField] private VineSwingingMagnets magnets;
        [SerializeField] private MinigameTimer gameTimer;
        [SerializeField] private PlacesDisplay placesDisplay;
        
        public IPlayerService PlayerService { get; private set; }
        public MinigameTimer GameTimer => gameTimer;
        public PlacesDisplay PlacesDisplay => placesDisplay;
        public int[] FundsPerRank => fundsPerRank;
        public VineSwingingAIConfigSO AIConfig => aiConfig;
        public PlayerStateMachine[] PlayerStateMachines => players.PlayerStateMachines;
        public VineSwingingPlayerView[] PlayerViews => players.PlayerViews;
        public PlayerCornerDisplay[] PlayerCornerDisplays => players.PlayerCornerDisplays;

        private IVineSwingingGameState currentState;
        private IPowerUpService powerUpService;
        private bool hasBeenInitialized;
        
        private bool isInGameplay;
        
        private float baseGameDurationInSeconds => TimerLengths.GetMinigameTimerLengthInSeconds();
        private float effectiveGameDurationInSeconds;

        public int PlayerCount => PlayerService.GetPlayerCount();

        public bool IsInGameplay
        {
            get => isInGameplay;
            set => isInGameplay = value;
        }

        private void Awake() {
            PlayerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            powerUpService = ServiceLocatorAccessor.GetService<IPowerUpService>();
            effectiveGameDurationInSeconds = baseGameDurationInSeconds;
            VineSwingingCountdownState = new(this, startCountdown);
            VineSwingingGameplayState = new(this);
            VineSwingingResultsState = new(this);
        }

        private IEnumerator Start() {
            while (!hasBeenInitialized) {
                yield return null;
            }

            SetUpVariables();
            StartGameFlow();
        }

        public void Initialize(bool isDoubleRound) {
            IsDoubleRound = isDoubleRound;
            countdownDurationInSeconds = TimerLengths.GetCountdownTimerLengthInSeconds();
            hasBeenInitialized = true;
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging initiated. Double round: {isDoubleRound}");
        }
        
        private void SetUpVariables() {
            if (IsDoubleRound) {
                effectiveGameDurationInSeconds *= 2;
                vineCount *= 2;
            }

            players.SetUp(PlayerService, powerUpService, PlayerCount, vineCount, vineAnchorY, countdownDurationInSeconds); 
            placesDisplay.Hide();
            startCountdown.Initialize(countdownDurationInSeconds);
            gameTimer.Initialize(effectiveGameDurationInSeconds);
        }

        private void StartGameFlow() {
            ChangeState(VineSwingingCountdownState);
        }

        public void ChangeState(IVineSwingingGameState newState) {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        private void Update() {
            if (!hasBeenInitialized) return;
            currentState?.OnUpdate();

            if (!isInGameplay && players.PlayerStateMachines != null) {
                for (int i = 0; i < players.PlayerStateMachines.Length; i++) {
                    players.PlayerStateMachines[i].Update(Time.deltaTime, false);
                    players.PlayerViews[i].Pull(players.PlayerStateMachines[i].PlayerContext);
                }
            }

            if (players.PlayerStateMachines != null) {
                for (int i = 0; i < players.PlayerStateMachines.Length; i++) {
                    var context = players.PlayerStateMachines[i].PlayerContext;
                    players.TrackViews[i].UpdateElapsedTime(players.PlayerStateMachines[i].ElapsedTime);

                    int activeVineIndex = -1;
                    if (isInGameplay && context.CurrentStateType == PlayerStateType.Swinging) {
                        activeVineIndex = context.CurrentVineIndex;
                    }
                    players.TrackViews[i].SetSweetSpotHintLevel(activeVineIndex, context.DisplayedHintLevel);
                }
            }

        }

        public void OnGameEnd(PlayerMinigameResult[] results) {
            StartCoroutine(WaitAndEndMinigame(results));
        }

        private IEnumerator WaitAndEndMinigame(PlayerMinigameResult[] results) {
            yield return new WaitForSeconds(resultsDisplayDurationInSeconds);
            OnMinigameFinished?.Invoke(results);
        }

        public void StartMusic() => music.Play();
        public void UpdateMagnets() => magnets.DoTick();
    }
}