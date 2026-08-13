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

        public void SetUp(IPlayerService playerService, IPowerUpService powerUpService, int playerCount, int vineCount, float vineAnchorY, int countdownDurationInSeconds) {
            PlayerStateMachines = new PlayerStateMachine[playerCount];
            PlayerHasMagnet = new bool[playerCount];
            PlayerMagnetRadii = new float[playerCount];
            PlayerMagnetPullSpeed = new float[playerCount];
            
            float[][] allVinePositions = new float[playerCount][];
            MovementModifiers[] playerModifiers = new MovementModifiers[playerCount];
            int seed = Random.Range(0, 10000);
            
            for (int i = 0; i < playerCount; i++) {
                PlayerSlot slot = playerService.PlayerSlots[i];
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
            
            for (int i = 0; i < playerCount; i++) {
                var randomNumberGenerator = new System.Random(seed);
                SwingConfig config = PlayerStateMachines[i].SwingConfig;
                float specialCoinRateMultiplier = 1f + playerModifiers[i].SpecialCoinRateBoostCount * 3f;
                var trails = CoinTrailGenerator.GenerateAllTrails(vineCount, config, seed);
                coinSpawners[i].SpawnCoinsForTrack(trails, allVinePositions[i], vineAnchorY, playerStats.CoinTypes, randomNumberGenerator, specialCoinRateMultiplier);
            }
            
            InitializePlayerDisplays(playerCount, playerService);
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