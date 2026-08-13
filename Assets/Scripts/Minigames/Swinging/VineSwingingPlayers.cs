using CoreData;
using Input;
using Minigames.Swinging.Core;
using Minigames.Swinging.Core.PlayerStateMachine;
using Player;
using Services;
using UnityEngine;

namespace Minigames.Swinging {
    public class VineSwingingPlayers : MonoBehaviour {
        [SerializeField] private VineSwingingPlayerStatsSO playerStats;
        [SerializeField] private PlayerCornerDisplay[] playerCornerDisplays = new PlayerCornerDisplay[4];
        [SerializeField] private VineSwingingPlayerView[] playerViews = new VineSwingingPlayerView[4];
        [SerializeField] private VineTrackView[] trackViews = new VineTrackView[4];
        [SerializeField] private VineSwingingCameraFollow[] cameraFollows = new VineSwingingCameraFollow[4];
        [SerializeField] private CoinTrailSpawnerView[] coinSpawners = new CoinTrailSpawnerView[4];
        
        public PlayerCornerDisplay[] PlayerCornerDisplays => playerCornerDisplays;
        public VineSwingingPlayerView[] PlayerViews => playerViews;
        public VineTrackView[] TrackViews => trackViews;
        
        public PlayerStateMachine[] PlayerStateMachines { get; private set; }
        public bool[] PlayerHasMagnet { get; private set; }
        public float[] PlayerMagnetRadii { get; private set; }
        public float[] PlayerMagnetPullSpeed { get; private set; }

        private MovementModifiers[] playerModifiers;
        private float[][] allVinePositions;

        public void SetUp(IPlayerService playerService, IPowerUpService powerUpService, int playerCount, int vineCount, float vineAnchorY, int countdownDurationInSeconds) {
            InitializeArrays(playerCount);
            int seed = Random.Range(0, 10000);
            
            for (int i = 0; i < playerCount; i++) {
                PlayerSlot slot = playerService.PlayerSlots[i];
                
                MovementModifiers movementModifiers = powerUpService.GetMovementModifiers(slot.Profile);
                playerModifiers[i] = movementModifiers;
                
                SwingConfig config = playerStats.CreateConfig(movementModifiers, movementModifiers.CoinSpawnRateBoostCount);
                
                bool hasMoveBoost = movementModifiers.MoveBoostCount > 0;

                var (vinePositions, phaseOffsets, periods) = trackViews[i].SpawnVines(vineCount, config.VineSpacing, vineAnchorY, config, new System.Random(seed), countdownDurationInSeconds);
                allVinePositions[i] = vinePositions;
                InitializePlayerStateMachine(vineAnchorY, i, config, vinePositions, phaseOffsets, periods);
                
                InitializePlayerView(i, movementModifiers, hasMoveBoost, config);
                cameraFollows[i].Initialize(PlayerStateMachines[i].PlayerContext);
            }
            
            SetUpCoins(playerCount, vineCount, vineAnchorY, seed);
            InitializePlayerDisplays(playerCount, playerService);
        }

        private void InitializeArrays(int playerCount) {
            PlayerStateMachines = new PlayerStateMachine[playerCount];
            PlayerHasMagnet = new bool[playerCount];
            PlayerMagnetRadii = new float[playerCount];
            PlayerMagnetPullSpeed = new float[playerCount];
            playerModifiers = new MovementModifiers[playerCount];
            allVinePositions = new float[playerCount][];
        }
        
        private void InitializePlayerStateMachine(float vineAnchorY, int playerIndex, SwingConfig config, float[] vinePositions,
            float[] phaseOffsets, float[] periods) {
            PlayerStateMachines[playerIndex] = new PlayerStateMachine(config, vinePositions, vineAnchorY, phaseOffsets, periods);
            PlayerStateMachines[playerIndex].Start(0);
        }
        
        private void InitializePlayerView(int playerIndex, MovementModifiers movementModifiers, bool hasMoveBoost,
            SwingConfig config) {
            PlayerHasMagnet[playerIndex] = (movementModifiers.MagnetCount > 0);
            PlayerMagnetRadii[playerIndex] = playerStats.MagnetRadius * movementModifiers.MagnetCount;
            PlayerMagnetPullSpeed[playerIndex] = playerStats.MagnetPullSpeed * movementModifiers.MagnetCount;
            playerViews[playerIndex].Initialize(hasMoveBoost, PlayerHasMagnet[playerIndex], config.CoinsPerGap);
            playerViews[playerIndex].transform.localPosition = new Vector3(
                PlayerStateMachines[playerIndex].PlayerContext.PositionX,
                PlayerStateMachines[playerIndex].PlayerContext.PositionY);
        }
        
        private void SetUpCoins(int playerCount, int vineCount, float vineAnchorY, int seed) {
            for (int i = 0; i < playerCount; i++) {
                var randomNumberGenerator = new System.Random(seed);
                SwingConfig config = PlayerStateMachines[i].SwingConfig;
                float specialCoinRateMultiplier = 1f + playerModifiers[i].SpecialCoinRateBoostCount * 3f;
                var trails = CoinTrailGenerator.GenerateAllTrails(vineCount, config, seed);
                coinSpawners[i].SpawnCoinsForTrack(trails, allVinePositions[i], vineAnchorY, playerStats.CoinTypes, randomNumberGenerator, specialCoinRateMultiplier);
            }
        }
        
        private void InitializePlayerDisplays(int playerCount, IPlayerService playerService) {
            for (int i = 0; i < playerCount; i++) {
                var slot = playerService.PlayerSlots[i];
                if (slot?.Profile != null) {
                    playerCornerDisplays[i].Initialize(slot.Profile, PlayerCornerDisplay.DisplayMode.Score);
                }
            }
        }
    }
}