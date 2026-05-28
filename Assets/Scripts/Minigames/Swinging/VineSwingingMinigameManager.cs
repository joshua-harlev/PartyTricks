using System;
using System.Collections;
using CoreData;
using FMOD.Studio;
using FMODUnity;
using Game;
using Minigames.Swinging.States;
using Services;
using UnityEngine;
using VineSwinging.Core;
using Random = UnityEngine.Random;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Minigames.Swinging {
    public class VineSwingingMinigameManager : MonoBehaviour, IMinigameManager {
        public event Action<PlayerMinigameResult[]> OnMinigameFinished;
        public bool IsDoubleRound { get; set; }

        [Header("Minigame Settings")] 
        private int countdownDurationInSeconds;
        private int[] fundsPerRank => MinigamePayouts.GetBaseFundsPerRank();
        [SerializeField] private float resultsDisplayDurationInSeconds = 5f;
        [SerializeField] private int vineCount = 20;
        [SerializeField] private float vineAnchorY = 4f;
        [SerializeField] private VineSwingingPlayerStatsSO playerStats;
        [SerializeField] private VineSwingingAIConfigSO aiConfig;

        [Header("References")] [SerializeField]
        private MinigameStartCountdown startCountdown;

        [SerializeField] private MinigameTimer gameTimer;
        [SerializeField] private PlacesDisplay placesDisplay;
        [SerializeField] private PlayerCornerDisplay[] playerCornerDisplays = new PlayerCornerDisplay[4];
        [SerializeField] private VineSwingingPlayerView[] playerViews = new VineSwingingPlayerView[4];
        [SerializeField] private VineTrackView[] trackViews = new VineTrackView[4];
        [SerializeField] private VineSwingingCameraFollow[] cameraFollows = new VineSwingingCameraFollow[4];
        [SerializeField] private CoinTrailSpawnerView[] coinSpawners = new CoinTrailSpawnerView[4];

        [Header("Audio")] 
        [SerializeField] private EventReference musicEvent;
        private EventInstance musicInstance;

        public PlayerStateMachine[] PlayerStateMachines { get; private set; }
        public IPlayerService PlayerService { get; private set; }
        public MinigameTimer GameTimer => gameTimer;
        public PlacesDisplay PlacesDisplay => placesDisplay;
        public PlayerCornerDisplay[] PlayerCornerDisplays => playerCornerDisplays;
        public VineSwingingPlayerView[] PlayerViews => playerViews;
        public int[] FundsPerRank => fundsPerRank;
        public VineSwingingAIConfigSO AIConfig => aiConfig;

        private IVineSwingingGameState currentState;
        private IPowerUpService powerUpService;
        private bool hasBeenInitialized;
        
        private bool isInGameplay;
        
        private float baseGameDurationInSeconds => TimerLengths.GetMinigameTimerLengthInSeconds();
        private float effectiveGameDurationInSeconds;

        public bool[] PlayerHasMagnet { get; private set; }
        public float[] PlayerMagnetRadii { get; private set; }
        public float[] PlayerMagnetPullSpeed { get; private set; }

        public bool IsInGameplay
        {
            get => isInGameplay;
            set => isInGameplay = value;
        }

        private void Awake() {
            PlayerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            powerUpService = ServiceLocatorAccessor.GetService<IPowerUpService>();
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
            effectiveGameDurationInSeconds = baseGameDurationInSeconds;
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

            InitializePlayerDisplays(); 
            placesDisplay.Hide();
            startCountdown.Initialize(countdownDurationInSeconds);
            gameTimer.Initialize(effectiveGameDurationInSeconds);
            
            PlayerStateMachines = new PlayerStateMachine[4];
            PlayerHasMagnet = new bool[4];
            PlayerMagnetRadii = new float[4];
            PlayerMagnetPullSpeed = new float[4];
            float[][] allVinePositions = new float[4][];
            MovementModifiers[] playerModifiers = new MovementModifiers[4];
            int seed = Random.Range(0, 10000);
            
            for (int i = 0; i < 4; i++) {
                PlayerSlot slot = PlayerService.PlayerSlots[i];
                MovementModifiers movementModifiers = powerUpService.GetMovementModifiers(slot.Profile);
                bool hasMoveBoost = movementModifiers.MoveBoostCount > 0;
                playerModifiers[i] = movementModifiers;
                PlayerHasMagnet[i] = (movementModifiers.MagnetCount > 0);
                PlayerMagnetRadii[i] = playerStats.MagnetRadius * movementModifiers.MagnetCount;
                PlayerMagnetPullSpeed[i] = playerStats.MagnetPullSpeed * movementModifiers.MagnetCount;
                
                
                SwingConfig config = playerStats.CreateConfig(movementModifiers, movementModifiers.CoinSpawnRateBoostCount);
                
                playerViews[i].Initialize(hasMoveBoost, PlayerHasMagnet[i], config.CoinsPerGap);
                
                var (vinePositions, phaseOffsets, periods) = trackViews[i].SpawnVines(vineCount, config.VineSpacing, vineAnchorY, config, new System.Random(seed), countdownDurationInSeconds);
                allVinePositions[i] = vinePositions;
                PlayerStateMachines[i] = new PlayerStateMachine(config, vinePositions, vineAnchorY, phaseOffsets, periods);
                PlayerStateMachines[i].Start(0);
                playerViews[i].transform.localPosition = new Vector3(
                    PlayerStateMachines[i].PlayerContext.PositionX,
                    PlayerStateMachines[i].PlayerContext.PositionY);
                cameraFollows[i].Initialize(PlayerStateMachines[i].PlayerContext);
            }

            
            for (int i = 0; i < 4; i++) {
                var randomNumberGenerator = new System.Random(seed);
                SwingConfig config = PlayerStateMachines[i].SwingConfig;
                float specialCoinRateMultiplier = 1f + playerModifiers[i].SpecialCoinRateBoostCount * 3f;
                var trails = CoinTrailGenerator.GenerateAllTrails(vineCount, config, seed);
                coinSpawners[i].SpawnCoinsForTrack(trails, allVinePositions[i], vineAnchorY, playerStats.CoinTypes, randomNumberGenerator, specialCoinRateMultiplier);
            }
        }

        private void InitializePlayerDisplays() {
            for (int i = 0; i < 4; i++) {
                var slot = PlayerService.PlayerSlots[i];
                if (slot?.Profile != null) {
                    playerCornerDisplays[i].Initialize(slot.Profile, PlayerCornerDisplay.DisplayMode.Score);
                }
            }
        }

        private void StartGameFlow() {
            ChangeState(new VineSwingingCountdownState(this, startCountdown));
        }

        public void ChangeState(IVineSwingingGameState newState) {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        private void Update() {
            if (!hasBeenInitialized) return;
            currentState?.OnUpdate();

            if (!isInGameplay && PlayerStateMachines != null) {
                for (int i = 0; i < PlayerStateMachines.Length; i++) {
                    PlayerStateMachines[i].Update(Time.deltaTime, false);
                    playerViews[i].Pull(PlayerStateMachines[i].PlayerContext);
                }
            }

            if (PlayerStateMachines != null) {
                float elapsedTime = PlayerStateMachines[0].ElapsedTime;
                foreach (var trackView in trackViews) {
                    trackView.UpdateElapsedTime(elapsedTime);
                }
            }

        }

        public void StartMusic() => musicInstance.start();

        private void OnDisable() {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
        }

        public void OnGameEnd(PlayerMinigameResult[] results) {
            StartCoroutine(WaitAndEndMinigame(results));
        }

        private IEnumerator WaitAndEndMinigame(PlayerMinigameResult[] results) {
            yield return new WaitForSeconds(resultsDisplayDurationInSeconds);
            OnMinigameFinished?.Invoke(results);
        }

        private void OnDestroy() {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}