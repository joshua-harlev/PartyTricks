using System;
using CoreData;

namespace Minigames {
    public class CoinTypeSelector {
        private readonly CoinTypeSO[] coinTypes;
        private readonly float specialCoinRateModifier;
        private int missCounter;

        public CoinTypeSelector(CoinTypeSO[] coinTypes, float specialCoinRateModifier) {
            this.coinTypes = coinTypes;
            this.specialCoinRateModifier = specialCoinRateModifier;
            this.missCounter = 0;
        }

        public CoinTypeSO SelectCoinType(Random random) {
            float targetSpecialCoinChance = CalculateSpecialCoinChance();

            float adjustedChance = targetSpecialCoinChance * (missCounter + 1);
            adjustedChance = Math.Min(adjustedChance, 1f);
            
            float roll = (float) random.NextDouble();
            if (roll < adjustedChance) {
                missCounter = 0;
                return GetRandomSpecialCoin(random);
            }

            missCounter++;
            return GetRandomRegularCoin(random);
        }

        private float CalculateSpecialCoinChance() {
            float specialWeight = 0f;
            float totalWeight = 0f;
            foreach (var coinType in coinTypes) {
                if (coinType == null) continue;
                float weight = coinType.SpawnWeight;
                if (coinType.IsSpecialCoin) weight *= specialCoinRateModifier;
                totalWeight += weight;
                if (coinType.IsSpecialCoin) specialWeight += weight;
            }
            if (totalWeight > 0) return specialWeight/totalWeight;
            return 0;
        }
        
        private CoinTypeSO GetRandomRegularCoin(Random random) {
            return GetWeightedRandomCoin(random, false);
        }

        private CoinTypeSO GetRandomSpecialCoin(Random random) {
            return GetWeightedRandomCoin(random,true);
        }

        private CoinTypeSO GetWeightedRandomCoin(Random random, bool isSpecial) {
            float totalWeight = 0f;
            foreach (var coinType in coinTypes) {
                if (coinType != null && coinType.IsSpecialCoin == isSpecial) {
                    totalWeight += coinType.SpawnWeight;
                }
            }
            
            float roll = (float) random.NextDouble() * totalWeight;
            float cumulativeWeight = 0f;
            foreach (var coinType in coinTypes) {
                if (coinType != null && coinType.IsSpecialCoin == isSpecial) {
                    cumulativeWeight += coinType.SpawnWeight;
                    if (roll <= cumulativeWeight) return coinType;
                }
            }
            
            return coinTypes[0];
        }
    }
}