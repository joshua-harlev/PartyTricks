using System;
using System.Collections;
using CoreData;
using FMOD.Studio;
using FMODUnity;
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

        [Header("Minigame Settings")] [SerializeField]
        private int gameDurationInSeconds = 30;

        [SerializeField] private int countdownDurationInSeconds = 5;
        [SerializeField] private float resultsDisplayDurationInSeconds = 5f;
        [SerializeField] private int[] fundsPerRank = new[] { 100, 80, 60, 50 };
        [SerializeField] private int vineCount = 20;
        [SerializeField] private float vineAnchorY = 4f;
        [SerializeField] private VineSwingingPlayerStatsSO playerStats;

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

        private IVineSwingingGameState currentState;
        private IPowerUpService powerUpService;
        private bool hasBeenInitialized;

        private bool isInGameplay;

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
            hasBeenInitialized = true;
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging initiated. Double round: {isDoubleRound}");
        }

        private void SetUpVariables() {
            if (IsDoubleRound) {
                gameDurationInSeconds *= 2;
            }

            InitializePlayerDisplays();
            placesDisplay.Hide();
            startCountdown.Initialize(countdownDurationInSeconds);
            gameTimer.Initialize(gameDurationInSeconds);
            
            PlayerStateMachines = new PlayerStateMachine[4];
            PlayerHasMagnet = new bool[4];
            PlayerMagnetRadii = new float[4];
            PlayerMagnetPullSpeed = new float[4];
            float[][] allVinePositions = new float[4][];
            MovementModifiers[] playerModifiers = new MovementModifiers[4];
            for (int i = 0; i < 4; i++) {
                PlayerSlot slot = PlayerService.PlayerSlots[i];
                MovementModifiers movementModifiers = powerUpService.GetMovementModifiers(slot.Profile);
                playerModifiers[i] = movementModifiers;
                PlayerHasMagnet[i] = (movementModifiers.MagnetCount > 0);
                PlayerMagnetRadii[i] = playerStats.MagnetRadius * movementModifiers.MagnetCount;
                PlayerMagnetPullSpeed[i] = playerStats.MagnetPullSpeed * movementModifiers.MagnetCount;

                SwingConfig config = playerStats.CreateConfig(movementModifiers);
                var (vinePositions, phaseOffsets, periods) = trackViews[i].SpawnVines(vineCount, config.VineSpacing, vineAnchorY, config, playerStats.PeriodVariation);
                allVinePositions[i] = vinePositions;
                PlayerStateMachines[i] = new PlayerStateMachine(config, vinePositions, vineAnchorY, phaseOffsets, periods);
                PlayerStateMachines[i].Start(0);
                cameraFollows[i].Initialize(PlayerStateMachines[i].PlayerContext);
            }

            int seed = Random.Range(0, 10000);
            var randomNumberGenerator = new System.Random(seed);
            for (int i = 0; i < 4; i++) {
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
        }

        public void StartMusic() => musicInstance.start();
        
        // only useful if music is configurable (like the original music for Dire Dodging)
        public void SetMusicIntensity(float intensity) => musicInstance.setParameterByName("Intensity", intensity);

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
    }
}