using CoreData;
using Minigames.Swinging.Core;
using UnityEngine;
using Random = System.Random;

namespace Minigames.Swinging {
    public class CoinTrailSpawnerView : MonoBehaviour {
        public void SpawnCoinsForTrack(CoinPosition[][] allTrails, float[] vinePositions, float vineAnchorY,
            CoinTypeSO[] coinTypes, Random randomNumberGenerator, float specialCoinRateModifier = 1f) {
            
            var selector = new CoinTypeSelector(coinTypes, specialCoinRateModifier);

            for (int gapIndex = 1; gapIndex < allTrails.Length; gapIndex++) {
                float startXPosition = vinePositions[gapIndex];
                float gapWidth = vinePositions[gapIndex + 1] - startXPosition;

                foreach (var coinPosition in allTrails[gapIndex]) {
                    float worldXPosition = startXPosition + coinPosition.RelativeXPosition * gapWidth;
                    float worldYPosition = vineAnchorY + coinPosition.RelativeYPosition;

                    CoinTypeSO coinType = selector.SelectCoinType(randomNumberGenerator);
                    var gameObject = Instantiate(coinType.CoinPrefab, transform);
                    gameObject.transform.localPosition = new Vector3(worldXPosition, worldYPosition);
                    gameObject.GetComponent<SwingingCoinView>().Initialize(coinType.PointValue);
                }
            }
        }
    }
}