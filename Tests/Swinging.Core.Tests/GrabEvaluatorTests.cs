using VineSwinging.Core;
using Xunit;

namespace Swinging.Core.Tests {
    public class GrabEvaluatorTests {
        
        // Helpers with a single vine at x=0, phase=0
        private static readonly float[] SingleVineXPosition = { 0f };
        private static readonly float[] SingleVinePhase = { 0f };
        private static readonly float[] SingleVinePeriod = { 2f };

        [Fact]
        public void VineIndexIsReturnedWhenPlayerIsOnVine() {
            var config = TestHelpers.DefaultConfig(grabRadius: 1.5f, ropeLength: 5f);
            float vineAnchorYPosition = 10f;
            float tipYPosition = vineAnchorYPosition - config.RopeLength;

            int result = GrabEvaluator.CheckGrab(
                playerX: 0f, playerY: tipYPosition, SingleVineXPosition, vineAnchorYPosition, config, minVineIndex: 0,
                SingleVinePhase, SingleVinePeriod, elapsedTime: 0f, playerContextVelocityX: 0f,
                playerContextVelocityY: 0f);
            
            Assert.Equal(0, result);
        }

        [Fact]
        public void NegativeOneIsReturnedWhenPlayerIsFarAway() {
            var config = TestHelpers.DefaultConfig(grabRadius: 1.5f, ropeLength: 5f);
            
            int result = GrabEvaluator.CheckGrab(
                playerX: 99999f, playerY: 99999f, SingleVineXPosition, vineAnchorY: 10f, config, minVineIndex: 0,
                SingleVinePhase, SingleVinePeriod, elapsedTime: 0f, playerContextVelocityX: 0f,
                playerContextVelocityY: 0f);
            
            Assert.Equal(-1, result);
        }

        [Fact]
        public void EarlierVinesAreSkippedWhenMinVineIndexSet() {
            var config = TestHelpers.DefaultConfig(grabRadius: 1.5f, ropeLength: 5f);
            float[] twoVineXPositions = { 0f, 20f };
            float[] twoVinePhases = { 0f, 0f };
            float[] twoVinePeriods = { 2f, 2f };
            float vineAnchorYPosition = 10f;
            float tipYPosition = vineAnchorYPosition - config.RopeLength;

            int result = GrabEvaluator.CheckGrab(
                playerX: 0f, playerY: tipYPosition,
                twoVineXPositions, vineAnchorYPosition, config, minVineIndex: 1,
                twoVinePhases, twoVinePeriods, elapsedTime: 0f,
                playerContextVelocityX: 0f, playerContextVelocityY: 0f);
            
            Assert.Equal(-1, result);
        }

        [Fact]
        public void LookaheadWhenPlayerIsMovingTowardsVineButOutsideRadius() {
            var config = TestHelpers.DefaultConfig(grabRadius: 0.5f, ropeLength: 5f, grabLookaheadFrames: 3, gravity: 0f);
            float vineAnchorYPosition = 10f;
            float tipYPosition = vineAnchorYPosition - config.RopeLength;
            
            int result = GrabEvaluator.CheckGrab(
                playerX: -1f, playerY: tipYPosition,
                SingleVineXPosition, vineAnchorYPosition,
                config, minVineIndex: 0,
                SingleVinePhase, SingleVinePeriod,
                elapsedTime: 0f,
                playerContextVelocityX: 30f, playerContextVelocityY: 0f);
            
            Assert.Equal(0, result);
        }

        [Fact]
        public void NegativeOneIsReturnedWhenPlayerIsOutsideOfRadiusAndMovingAway() {
            var config = TestHelpers.DefaultConfig(grabRadius: 0.5f, ropeLength: 5f, grabLookaheadFrames: 3, gravity: 0f);
            float vineAnchorYPosition = 10f;
            float tipYPosition = vineAnchorYPosition - config.RopeLength;
            
            int result = GrabEvaluator.CheckGrab(
                playerX: -1f, playerY: tipYPosition,
                SingleVineXPosition, vineAnchorYPosition,
                config, minVineIndex: 0,
                SingleVinePhase, SingleVinePeriod,
                elapsedTime: 0f,
                playerContextVelocityX: -30f, playerContextVelocityY: 0f);
            
            Assert.Equal(-1, result);
        }

        [Fact]
        public void NegativeOneIsReturnedWhenPlayerIsJustOutsideOfRadius() {
            var config = TestHelpers.DefaultConfig(grabRadius: 1.0f, ropeLength: 5f, grabLookaheadFrames: 0);
            float vineAnchorY = 10f;
            float tipYPosition = vineAnchorY - config.RopeLength;
            
            int result = GrabEvaluator.CheckGrab(
                playerX: 1.1f, playerY: tipYPosition,
                SingleVineXPosition, vineAnchorY,
                config, minVineIndex: 0,
                SingleVinePhase, SingleVinePeriod,
                elapsedTime: 0f,
                playerContextVelocityX: 0f, playerContextVelocityY: 0f);
            
            Assert.Equal(-1, result);
        }

        [Fact]
        public void FirstVineInRangeIsReturnedWhenMultipleAreInRange() {
            var config = TestHelpers.DefaultConfig(grabRadius: 1.5f, ropeLength: 5f);
            float[] threeVineXPositions = { 0f, 10f, 20f };
            float[] threeVinePhases = { 0f, 0f, 0f };
            float[] threeVinePeriods = { 2f, 2f, 2f };
            float vineAnchorYPosition = 10f;
            float tipYPosition = vineAnchorYPosition - config.RopeLength;
            
            int result = GrabEvaluator.CheckGrab(
                playerX: 10f, playerY: tipYPosition,
                threeVineXPositions, vineAnchorYPosition,
                config, minVineIndex: 0,
                threeVinePhases, threeVinePeriods,
                elapsedTime: 0f,
                playerContextVelocityX: 0f, playerContextVelocityY: 0f);

            Assert.Equal(1, result);
        }
    }
}