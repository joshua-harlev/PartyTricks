using System;
using VineSwinging.Core;
using Xunit;

namespace Swinging.Core.Tests {
    public class VineAttractionTests {
        [Fact]
        public void DistanceAtPhaseZeroReturnsZero() {
          Assert.Equal(0f, VineAttraction.DistanceToNearestMultipleOfPi(0f), 3);  
        }

        [Fact]
        public void DistanceAtPhasePiReturnsZero() {
            Assert.Equal(0f, VineAttraction.DistanceToNearestMultipleOfPi((float)Math.PI), 3);
        }

        [Fact]
        public void DistanceAtPhaseHalfPiReturnsHalfPi() {
            float expected = (float)Math.PI / 2f;
            Assert.Equal(expected, VineAttraction.DistanceToNearestMultipleOfPi(expected), 3);
        }
        
        [Fact]
        public void DistanceAtNegativeHalfPiReturnsHalfPi() {
            float expected = (float)Math.PI / 2f;
            Assert.Equal(expected, VineAttraction.DistanceToNearestMultipleOfPi(-expected), 3);
        }
        
        [Fact]
        public void DistanceAtPhaseTwoPiReturnsZero() {
            Assert.Equal(0f, VineAttraction.DistanceToNearestMultipleOfPi((float)(2*Math.PI)), 2);
        }

        [Fact]
        public void DistanceAtQuarterPiReturnsQuarterPi() {
            float expected = (float)Math.PI / 4f;
            Assert.Equal(expected, VineAttraction.DistanceToNearestMultipleOfPi(expected), 3);
        }

        [Fact]
        public void AdjustmentIncreaseWhenFarFromIdeal() {
            var config = TestHelpers.DefaultConfig();
            float result = VineAttraction.UpdatePhaseAdjustment(0f, (float)Math.PI/2f, 100f, 0f, config, 0.1f);
            Assert.True(result > 0f);
        }
        
        [Fact]
        public void AdjustmentDecreaseWhenFarFromIdeal() {
            var config = TestHelpers.DefaultConfig();
            float result = VineAttraction.UpdatePhaseAdjustment(0f, 0.1f, 100f, 0f, config, 0.1f);
            Assert.True(result < 0f);
        }

        [Fact]
        public void AdjustmentAccumulatesAcrossFrames() {
            var config = TestHelpers.DefaultConfig();
            float vinePhaseOffset = (float)Math.PI / 2f;
            float oneFrameAdjustment = VineAttraction.UpdatePhaseAdjustment(0f, vinePhaseOffset, 100f, 0f, config, 0.1f);
            float twoFrameAdjustment = VineAttraction.UpdatePhaseAdjustment(oneFrameAdjustment, vinePhaseOffset, 100f, 0f, config, 0.1f);
            Assert.True(twoFrameAdjustment > oneFrameAdjustment);
            Assert.True(oneFrameAdjustment > 0f);
        }

        [Fact]
        public void AdjustmentCappedAtMax() {
            var config = TestHelpers.DefaultConfig();
            float result = VineAttraction.UpdatePhaseAdjustment(config.MaxPhaseAdjustment - 0.001f, (float)Math.PI / 2f, 100f, 0f, config, 0.1f);
            Assert.True(result <= config.MaxPhaseAdjustment);
        }

        [Fact]
        public void NoChangeWhenDeltaTimeIsZero() {
            var config = TestHelpers.DefaultConfig();
            float result = VineAttraction.UpdatePhaseAdjustment(0.5f, (float)Math.PI/2f, 100f, 0f, config, 0f);
            Assert.Equal(0.5f, result);
        }

        [Fact]
        public void LargerAdjustmentPerFrameWhenAttractionRateHigher() {
            var lowConfig = TestHelpers.DefaultConfig(baseAttractionRate: 1f);
            var highConfig = TestHelpers.DefaultConfig(baseAttractionRate: 5f);
            float vinePhaseOffset = (float)Math.PI / 2f;
            float resultLow = VineAttraction.UpdatePhaseAdjustment(0f, vinePhaseOffset, 100f, 0f, lowConfig, 0.1f);
            float resultHigh = VineAttraction.UpdatePhaseAdjustment(0f, vinePhaseOffset, 100f, 0f, highConfig, 0.1f);
            Assert.True(resultHigh > resultLow);
        }

        [Fact]
        public void MoreDecelerationNearIdealWhenSlowdownRateHigher() {
            var lowConfig = TestHelpers.DefaultConfig(slowdownRate: 0.3f);
            var highConfig = TestHelpers.DefaultConfig(slowdownRate: 2f);
            float resultLow = VineAttraction.UpdatePhaseAdjustment(0f, 0.1f, 100f, 0f, lowConfig, 0.1f);
            float resultHigh = VineAttraction.UpdatePhaseAdjustment(0f, 0.1f, 100f, 0f, highConfig, 0.1f);
            Assert.True(resultHigh < resultLow);
        }
    }
}