using System;
using Minigames.Swinging.Core;
using Xunit;

namespace Swinging.Core.Tests {
    public class CoinTrailGeneratorTests {
        [Theory]
        [InlineData(3)]
        [InlineData(0)]
        [InlineData(10)]
        public void GenerateAllTrailsReturnsCorrectNumberOfTrails(int coinsPerGap) {
            var config = TestHelpers.DefaultConfig(coinsPerGap: coinsPerGap);
            var trails = CoinTrailGenerator.GenerateAllTrails(vineCount: 5, config, seed: 0);
            
            Assert.Equal(4, trails.Length);
            var trailsTrimmed = trails[1..];
            Assert.All(trailsTrimmed, trail => Assert.Equal(coinsPerGap, trail.Length));
        }

        [Fact]
        public void CoinsAreOrderedLeftToRight() {
            var config = TestHelpers.DefaultConfig(coinsPerGap: 5, vineSpacing: 20f);
            var trails = CoinTrailGenerator.GenerateAllTrails(vineCount: 3, config, seed: 0);
            foreach (var trail in trails) {
                for (int i = 1; i < trail.Length; i++) {
                    Assert.True(trail[i].RelativeXPosition > trail[i - 1].RelativeXPosition);
                }
            }
        }
        
        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void CoinsHaveAnArcShape(int coinsPerGap) {
            // Assumes that the Left to Right test works
            var config = TestHelpers.DefaultConfig(coinsPerGap: coinsPerGap);
            var trails = CoinTrailGenerator.GenerateAllTrails(vineCount: 3, config, seed: 0);
            var midpoint = coinsPerGap / 2;
            foreach (var trail in trails) {
                for (int i = 2; i < midpoint; i++) {
                    Assert.True(trail[i].RelativeYPosition > trail[i - 1].RelativeYPosition);
                }
                for (int i = midpoint+1; i < trail.Length; i++) {
                    Assert.True(trail[i].RelativeYPosition < trail[i - 1].RelativeYPosition);
                }
            }
        }

        [Fact]
        public void CoinXPositionsStayBetweenZeroAndOne() {
            var config = TestHelpers.DefaultConfig(coinsPerGap: 5, vineSpacing: 20f);
            var trails = CoinTrailGenerator.GenerateAllTrails(vineCount: 4, config, seed: 0);

            foreach (var trail in trails) {
                Assert.All(trail, coin =>
                {
                    Assert.True(coin.RelativeXPosition > 0f);
                    Assert.True(coin.RelativeXPosition < 1f);
                });
            }
        }
    }
}