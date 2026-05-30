using System;
using System.Collections;
using System.Collections.Generic;
using CoreData;
using Debug;
using FMOD.Studio;
using FMODUnity;
using Game;
using Services;
using UnityEngine;
using UnityEngine.Serialization;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Minigames.CoinTilt {
    public class CoinTiltMinigameManager : MonoBehaviour, IMinigameManager {
        public event Action<PlayerMinigameResult[]> OnMinigameFinished;
        public bool IsDoubleRound { get; private set; }

        [Header("Minigame Settings")] 
        private int countdownDurationInSeconds;
        [SerializeField] private float resultsDisplayDurationInSeconds = 5f;
        [SerializeField] private EventReference MusicEvent;
        [SerializeField] private EventReference CoinCollectSoundEvent;

        [Header("References")] [SerializeField]
        private CoinTiltPlayer[] players = new CoinTiltPlayer[4];

        [SerializeField] private TiltingPlatform[] tiltingPlatforms = new TiltingPlatform[4];
        [SerializeField] private CoinSpawner[] coinSpawners = new CoinSpawner[4];
        [SerializeField] private PlayerCornerDisplay[] playerCornerDisplays = new PlayerCornerDisplay[4];

        [FormerlySerializedAs("countdown")] [SerializeField]
        private MinigameStartCountdown StartCountdown;

        [SerializeField] private MinigameTimer gameTimer;
        [SerializeField] private PlacesDisplay placesDisplay;

        private float baseGameDurationInSeconds => TimerLengths.GetMinigameTimerLengthInSeconds();
        private float effectiveGameDurationInSeconds;
        private float targetIntensityCompletionTime;
        private bool hasBeenInitialized;
        private EventInstance musicInstance;
        private readonly int[] playerScores = new int[4];
        private IPlayerService playerService;
        private IPowerUpService powerUpService;

        private void Start() {
            StartCoroutine(WaitForInitialization());
        }

        private void Awake() {
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            powerUpService = ServiceLocatorAccessor.GetService<IPowerUpService>();
            musicInstance = RuntimeManager.CreateInstance(MusicEvent);
            effectiveGameDurationInSeconds = baseGameDurationInSeconds;
        }

        public void Initialize(bool isDoubleRound) {
            this.IsDoubleRound = isDoubleRound;
            countdownDurationInSeconds = TimerLengths.GetCountdownTimerLengthInSeconds();
            AdjustGameDurationForDoubleRound();
            CheckForGameObjectAssignments();
            InitializeVariables();

            hasBeenInitialized = true;
            DebugLogger.Log(LogChannel.Systems, $"CoinTiltMinigame initialized. Double round: {isDoubleRound}");
        }

        private void InitializeVariables() {
            InitializePlayerScores();
            InitializeCountdown();
            InitializeGameTimer();
            InitializePlacesDisplay();
        }

        private void InitializeGameTimer() {
            if (gameTimer == null) {
                UnityEngine.Debug.LogError("CoinTiltMinigameManager does not have a Coin Tilt Minigame Timer assigned!");
            }
            else {
                gameTimer.Initialize(effectiveGameDurationInSeconds);
                gameTimer.OnTimerEnd += EndGame;
                targetIntensityCompletionTime = effectiveGameDurationInSeconds * 0.6f;
            }
        }

        private void InitializeCountdown() {
            if (StartCountdown == null) {
                UnityEngine.Debug.LogError("CoinTiltMinigameManager does not have a Coin Tilt Minigame Countdown assigned!");
            }
            else {
                StartCountdown.Initialize(countdownDurationInSeconds);
            }
        }

        private void InitializePlacesDisplay() {
            if (placesDisplay == null) {
                UnityEngine.Debug.LogError("CoinTiltMinigameManager does not have a Coin Tilt Places Display assigned!");
            }
            else {
                placesDisplay.Hide();
            }
        }

        private void CheckForGameObjectAssignments() {
            CheckForPlayerAssignments();
            CheckForTiltingPlatformAssignments();
            CheckForCoinSpawnerAssignments();
        }

        private void AdjustGameDurationForDoubleRound() {
            if (IsDoubleRound) {
                effectiveGameDurationInSeconds *= 2;
            }
        }

        private void InitializePlayerScores() {
            for (int i = 0; i < 4; i++) {
                playerScores[i] = 0;
            }
        }

        private void CheckForCoinSpawnerAssignments() {
            if (coinSpawners == null || coinSpawners.Length != 4) {
                UnityEngine.Debug.LogError("CoinTiltMinigameManager does not have 4 Coin Spawners assigned!");
            }
        }

        private void CheckForTiltingPlatformAssignments() {
            if (tiltingPlatforms == null || tiltingPlatforms.Length != 4) {
                UnityEngine.Debug.LogError("CoinTiltMinigameManager does not have 4 Tilting Platforms assigned!");
            }
        }

        private void CheckForPlayerAssignments() {
            if (players == null || players.Length != 4) {
                UnityEngine.Debug.LogError("CoinTiltMinigameManager does not have 4 players assigned!");
            }
        }

        private IEnumerator WaitForInitialization() {
            while (!hasBeenInitialized) {
                yield return null;
            }

            StartCountdownPhase();
        }

        private void StartCountdownPhase() {
            InitializePlayers();
            InitializePlayerDisplays();

            DebugLogger.Log(LogChannel.Systems, "Starting countdown phase...");
            StartCountdown.StartTimer();
            StartCountdown.OnTimerEnd += StartPlayingPhase;
        }

        private void InitializePlayers() {
            for (int i = 0; i < players.Length; i++) {
                if (!players[i]) {
                    UnityEngine.Debug.LogError($"CoinTiltMinigameManager does not have a player in slot {i}!");
                    continue;
                }

                var slot = playerService.PlayerSlots[i];
                if (!slot) {
                    UnityEngine.Debug.LogError($"PlayerSlot {i} not found!");
                    continue;
                }

                InitializePlayerWithEvents(i, slot);
                InitializeTiltingPlatformForPlayer(i);
            }
        }

        private void InitializeTiltingPlatformForPlayer(int playerIndex) {
            if (tiltingPlatforms[playerIndex] != null) {
                tiltingPlatforms[playerIndex].Initialize(players[playerIndex]);
                players[playerIndex].SetPlatform(tiltingPlatforms[playerIndex]);
            }
        }

        private void InitializePlayerWithEvents(int playerIndex, PlayerSlot slot) {
            PlayerProfile profile = playerService.PlayerSlots[playerIndex].Profile;
            MovementModifiers modifiers = powerUpService.GetMovementModifiers(profile);
            players[playerIndex].Initialize(playerIndex, slot.InputHandler, slot.IsAI, modifiers);
            players[playerIndex].OnCoinCollected += HandleCoinCollected;
            players[playerIndex].OnFallOff += HandlePlayerFall;
        }

        private void InitializePlayerDisplays() {
            for (int i = 0; i < playerCornerDisplays.Length; i++) {
                if (!playerCornerDisplays[i]) {
                    UnityEngine.Debug.LogWarning($"PlayerCornerDisplay {i} not found!");
                    continue;
                }

                var slot = playerService.PlayerSlots[i];
                if (slot?.Profile != null) {
                    playerCornerDisplays[i].Initialize(slot.Profile, PlayerCornerDisplay.DisplayMode.Score);
                }
            }
        }

        private void StartPlayingPhase() {
            StartCountdown.OnTimerEnd -= StartPlayingPhase;
            gameTimer.StartTimer();
            musicInstance.start();

            EnablePlayerInput();
            DebugLogger.Log(LogChannel.Systems, "Movement enabled.");
            StartCoinSpawning();
            DebugLogger.Log(LogChannel.Systems, "Game started!");
        }

        private void StartCoinSpawning() {
            for (int i = 0; i < coinSpawners.Length; i++) {
                if (coinSpawners[i]) {
                    coinSpawners[i].StartSpawning(effectiveGameDurationInSeconds, playerService.GetPlayerProfile(i));
                }
            }
        }

        private void EnablePlayerInput() {
            foreach (var player in players) {
                if (player) {
                    player.EnableInput();
                }
            }
        }


        private void HandleCoinCollected(int playerIndex, int coinValue) {
            RuntimeManager.PlayOneShot(CoinCollectSoundEvent);
            playerScores[playerIndex] += coinValue;
            playerCornerDisplays[playerIndex].UpdateScore(playerScores[playerIndex]);
            DebugLogger.Log(LogChannel.Systems,
                $"P{playerIndex + 1} collected a coin. New score: {playerScores[playerIndex]}");
        }

        private void HandlePlayerFall(int playerIndex) {
            DebugLogger.Log(LogChannel.Systems, $"Player {playerIndex} fell!");
        }

        private void EndGame() {
            gameTimer.OnTimerEnd -= EndGame;
            DisablePlayerControlsAndMovement();
            FinalizeCoinSpawnerOperations();

            DebugLogger.Log(LogChannel.Systems, "Game ended. Calculating results.");
            StartCoroutine(DisplayResultsAndFinish());
        }

        private void DisablePlayerControlsAndMovement() {
            foreach (var player in players) {
                if (player) {
                    player.DisableInput();
                    player.Freeze();
                }
            }
        }

        private void FinalizeCoinSpawnerOperations() {
            for (int i = 0; i < coinSpawners.Length; i++) {
                if (coinSpawners[i]) {
                    coinSpawners[i].StopSpawning();
                    coinSpawners[i].DestroyAll();
                }
            }
        }

        private IEnumerator DisplayResultsAndFinish() {
            var results = CalculateResults();

            // short delay so that the players have a moment to breathe
            yield return new WaitForSeconds(0.75f);

            string[] resultsText = new string[4];
            for (int i = 0; i < results.Length; i++) {
                int fundsEarned = results[i].BaseFundsEarned;
                int currentFunds = playerService.PlayerSlots[i].Profile.Wallet.GetCurrentFunds();
                int newFunds = currentFunds + fundsEarned;
                int place = results[i].PlayerPlace;
                DebugLogger.Log(LogChannel.Systems, $"Player {i}: Score {playerScores[i]}, Rank {place}");
                resultsText[i] += GetPlaceText(place);
                resultsText[i] += "\n";
                resultsText[i] += "<size=50>+" + fundsEarned + " funds</size>";
                resultsText[i] += "\n";
                resultsText[i] += "<size=30>New funds: " + newFunds + "</size>";
            }

            placesDisplay.UpdateTextObjects(resultsText);
            placesDisplay.Show();
            yield return new WaitForSeconds(resultsDisplayDurationInSeconds);
            OnMinigameFinished?.Invoke(results);
        }

        private string GetPlaceText(int place) {
            return place switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                4 => "4th",
                _ => "ERR"
            };
        }

        private PlayerMinigameResult[] CalculateResults() {
            var results = new PlayerMinigameResult[4];

            var playerRankings = new List<(int index, int score)>();
            for (int i = 0; i < 4; i++) {
                playerRankings.Add((i, playerScores[i]));
            }

            playerRankings.Sort((a, b) => b.score.CompareTo(a.score));

            int[] ranks = new int[4];
            int currentRank = 0;
            for (int i = 0; i < playerRankings.Count; i++) {
                if (i > 0 && playerRankings[i].score == playerRankings[i - 1].score) {
                    ranks[playerRankings[i].index] = ranks[playerRankings[i - 1].index];
                }
                else {
                    ranks[playerRankings[i].index] = currentRank;
                    currentRank++;
                }

            }

            int multiplier = IsDoubleRound ? 2 : 1;
            int[] sourceFunds = MinigamePayouts.GetBaseFundsPerRank();
            for (int i = 0; i < 4; i++) {
                results[i] = new PlayerMinigameResult(i, ranks[i], sourceFunds[ranks[i]]*multiplier);
            }

            return results;
        }

        private void OnDestroy() {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
            foreach (var player in players) {
                player.OnCoinCollected -= HandleCoinCollected;
                player.OnFallOff -= HandlePlayerFall;
            }
        }
    }
}