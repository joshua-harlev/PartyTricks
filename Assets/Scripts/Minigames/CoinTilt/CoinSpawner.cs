using System.Collections.Generic;
using CoreData;
using Debug;
using Player;
using Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Minigames.CoinTilt {
    public class CoinSpawner : MonoBehaviour {
        [Header("Coin Types")] [SerializeField]
        private CoinTypeSO[] AvailableCoinTypes;

        [Header("Spawn Settings")] 
        [Tooltip("Coins per second")]
        [SerializeField] private float InitialSpawnRate = 0.45f;
        [SerializeField] private float FinalSpawnRate = 1.3f;
        [SerializeField] private float LateGamePhaseStartTimeFromEndInSeconds = 7f;
        [SerializeField] private int MaxCoinsOnPlatform = 10;
        [SerializeField] private float CoinLifetimeInSeconds = 15f;
        [SerializeField] private float SpecialCoinRateBoostPerStack = 3f;
        [SerializeField] private float SpawnRateMultiplierPerStack = 1.50f;
        [SerializeField] private int MaxSpawnAttempts = 10;
        [SerializeField] private float MinCoinDistance = 1f;

        [Header("Spawn Area")] [SerializeField]
        private float SpawnRadiusMin = 2f;

        [SerializeField] private float SpawnRadiusMax = 7.5f;
        [SerializeField] private float PlayerAvoidanceRadius = 3.5f;
        [SerializeField] private float SpawnHeight = 0.0025f;

        [Header("References")] 
        [SerializeField] private Transform PlatformTransform;

        [SerializeField] private CoinTiltPlayer AssignedPlayer;

        private bool isSpawning;
        private float spawnTimer;
        private float currentSpawnInterval;
        private float gameDuration;
        private float elapsedTime;
        private bool powerupsHaveBeenApplied = false;
        private readonly List<GameObject> activeCoins = new();
        private IPowerUpService powerUpService;
        private CoinTypeSelector coinTypeSelector;
        private System.Random random;

        private void Awake() {
            powerUpService = ServiceLocatorAccessor.GetService<IPowerUpService>();
        }

        private void Update() {
            if (!isSpawning) return;

            elapsedTime += Time.deltaTime;
            spawnTimer += Time.deltaTime;
            UpdateSpawnRate();

            if (spawnTimer >= currentSpawnInterval) {
                spawnTimer = 0;
                TryToSpawnCoin();
            }

            activeCoins.RemoveAll(coin => coin == null);
        }

        public void StartSpawning(float durationInSeconds,
            PlayerProfile playerProfile) {
            MovementModifiers modifiers = powerUpService.GetMovementModifiers(playerProfile);
            
            float specialCoinRateBoostModifier = 1 + (modifiers.SpecialCoinRateBoostCount * SpecialCoinRateBoostPerStack);
            coinTypeSelector = new CoinTypeSelector(AvailableCoinTypes, specialCoinRateBoostModifier);
            random = new System.Random();
            
            if (!powerupsHaveBeenApplied) {
                float spawnRateMultiplier = 1;
                for (int i = 0; i < modifiers.CoinSpawnRateBoostCount; i++) {
                    spawnRateMultiplier *= SpawnRateMultiplierPerStack;
                }

                InitialSpawnRate *= spawnRateMultiplier;
                FinalSpawnRate *= spawnRateMultiplier;
                powerupsHaveBeenApplied = true;
            }

            isSpawning = true;
            gameDuration = durationInSeconds;
            elapsedTime = 0;
            spawnTimer = 0;
            TryToSpawnCoin();
            UpdateSpawnRate();
            DebugLogger.Log(LogChannel.Systems, "Coin spawning started.");
        }

        public void StopSpawning() {
            isSpawning = false;
            DebugLogger.Log(LogChannel.Systems, "Coin spawning stopped.");
        }

        public void DestroyAll() {
            foreach (GameObject coin in activeCoins) {
                if (coin) {
                    Destroy(coin);
                }
            }
        }

        private void UpdateSpawnRate() {
            float timeRemaining = gameDuration - elapsedTime;
            float spawnRate;
            bool endGameTimeThresholdReached = timeRemaining <= LateGamePhaseStartTimeFromEndInSeconds;

            if (endGameTimeThresholdReached) {
                spawnRate = FinalSpawnRate;
            }
            else {
                spawnRate = GetSpawnRateBasedOnTime();
            }

            currentSpawnInterval = 1f / spawnRate;
        }

        private float GetSpawnRateBasedOnTime() {
            float progress = elapsedTime / (gameDuration - LateGamePhaseStartTimeFromEndInSeconds);
            float midGameRate = (InitialSpawnRate + FinalSpawnRate) / 2f;
            var spawnRate = Mathf.Lerp(InitialSpawnRate, midGameRate, progress);
            return spawnRate;
        }

        private void TryToSpawnCoin() {
            bool tooManyCoinsAlreadyExist = MaxCoinsOnPlatform > 0 && activeCoins.Count >= MaxCoinsOnPlatform;
            if (tooManyCoinsAlreadyExist) {
                return;
            }

            CoinTypeSO coinType = coinTypeSelector.SelectCoinType(random);

            if (!coinType || !coinType.CoinPrefab) {
                UnityEngine.Debug.LogWarning("No valid coin type/prefab available.");
                return;
            }

            var spawnPosition = TryToFindValidSpawnLocation(out var attempts);

            if (attempts >= MaxSpawnAttempts) {
                UnityEngine.Debug.LogWarning("Could not find valid coin spawn location.");
                return;
            }

            var coinObject = CreateCoinInstance(coinType, spawnPosition);
            InitializeAndTrackCoin(coinObject, coinType);
        }

        private void InitializeAndTrackCoin(GameObject coinObject, CoinTypeSO coinType) {
            Coin coin = coinObject.GetComponent<Coin>();
            if (coin) {
                coin.InitializeWithType(coinType);
                coin.SetSpawnHeight(SpawnHeight);
            }
            activeCoins.Add(coinObject);

            if (CoinLifetimeInSeconds > 0) {
                coin.PrepareForDestruction(CoinLifetimeInSeconds);
            }
        }

        private Vector3 TryToFindValidSpawnLocation(out int attempts) {
            Vector3 spawnPosition;
            attempts = 0;
            do {
                spawnPosition = GetRandomSpawnPosition();
                attempts++;
            } while (attempts < MaxSpawnAttempts && !IsValidSpawnPosition(spawnPosition));

            return spawnPosition;
        }

        private GameObject CreateCoinInstance(CoinTypeSO coinType, Vector3 spawnPosition) {
            // Can consider adding object pooling IF noticing performance issues.
            GameObject coinObject = Instantiate(coinType.CoinPrefab, spawnPosition, Quaternion.identity);
            coinObject.transform.SetParent(PlatformTransform, true);
            coinObject.transform.rotation = Quaternion.identity;

            Vector3 localPosition = PlatformTransform.InverseTransformPoint(spawnPosition);
            localPosition.y = SpawnHeight;
            coinObject.transform.localPosition = localPosition;
            coinObject.transform.localRotation = Quaternion.identity;

            Vector3 desiredWorldScale = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 parentScale = PlatformTransform.lossyScale;
            coinObject.transform.localScale = new Vector3(
                desiredWorldScale.x / parentScale.x,
                desiredWorldScale.y / parentScale.y,
                desiredWorldScale.z / parentScale.z);
            return coinObject;
        }

        private Vector3 GetRandomSpawnPosition() {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            float radius = Random.Range(SpawnRadiusMin, SpawnRadiusMax);

            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, SpawnHeight, Mathf.Sin(angle) * radius);
            return PlatformTransform.position + offset;
        }

        private bool IsValidSpawnPosition(Vector3 worldPosition) {
            Vector3 localPosition = PlatformTransform.InverseTransformPoint(worldPosition);
            localPosition.y = SpawnHeight;
            Vector3 finalWorldPosition = PlatformTransform.TransformPoint(localPosition);

            if (AssignedPlayer) {
                float distanceToPlayer = Vector3.Distance(finalWorldPosition, AssignedPlayer.Position);
                if (distanceToPlayer < PlayerAvoidanceRadius) {
                    return false;
                }
            }

            foreach (var coin in activeCoins) {
                if (coin) {
                    float distanceToCoin = Vector3.Distance(finalWorldPosition, coin.transform.position);
                    if (distanceToCoin < MinCoinDistance) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}