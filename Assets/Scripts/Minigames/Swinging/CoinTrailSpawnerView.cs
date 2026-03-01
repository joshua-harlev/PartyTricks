using UnityEngine;
using VineSwinging.Core;
using Random = System.Random;

namespace Minigames.Swinging {
    public class CoinTrailSpawnerView : MonoBehaviour {
        public void SpawnCoinsForTrack(CoinPosition[][] allTrails, float[] vinePositions, float vineAnchorY,
            CoinTypeSO[] coinTypes, Random randomNumberGenerator, float specialCoinRateModifier = 1f) {
            float totalWeight = 0f;
            foreach (var coinType in coinTypes) {
                float spawnWeight = coinType.SpawnWeight;
                if(coinType.IsSpecialCoin) spawnWeight *= specialCoinRateModifier;
                totalWeight += spawnWeight;
            }

            for (int gapIndex = 1; gapIndex < allTrails.Length; gapIndex++) {
                float startXPosition = vinePositions[gapIndex];
                float gapWidth = vinePositions[gapIndex + 1] - startXPosition;

                foreach (var coinPosition in allTrails[gapIndex]) {
                    float worldXPosition = startXPosition + coinPosition.RelativeXPosition * gapWidth;
                    float worldYPosition = vineAnchorY + coinPosition.RelativeYPosition;

                    CoinTypeSO coinType = SelectCoinType(coinTypes, totalWeight, randomNumberGenerator, specialCoinRateModifier);
                    var gameObject = Instantiate(coinType.CoinPrefab, transform);
                    gameObject.transform.localPosition = new Vector3(worldXPosition, worldYPosition);
                    gameObject.GetComponent<SwingingCoinView>().Initialize(coinType.PointValue);
                }
            }
        }

        private CoinTypeSO SelectCoinType(CoinTypeSO[] coinTypes, float totalWeight, Random randomNumberGenerator, float specialCoinRateModifier) {
            float roll = (float)(randomNumberGenerator.NextDouble() * totalWeight);
            float cumulativeWeight = 0f;
            foreach (var coinType in coinTypes) {
                float weight = coinType.SpawnWeight;
                if (coinType.IsSpecialCoin) {
                    weight *= specialCoinRateModifier;
                }
                cumulativeWeight += weight;
                if (roll <= cumulativeWeight) return coinType;
            }

            return coinTypes[^1];
        }
    }
}