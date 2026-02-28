using System;
using VineSwinging.Core;
using Xunit;

namespace Swinging.Core.Tests {
    public class VineAttractionTests {
        // Helper: compute vine endpoint position for a given phase
        private static (float endX, float endY) GetVineEndpoint(float phase, SwingConfig config, float vineAnchorX, float vineAnchorY) {
            var (offsetX, offsetY) = SwingSimulation.GetSwingPosition(phase, config.Amplitude, config.RopeLength);
            return (vineAnchorX + offsetX, vineAnchorY + offsetY);
        }
        
        [Fact]
        public void AdjustmentIncreasesWhenVineFarFromPlayer() {
            var config = TestHelpers.DefaultConfig();
            // Vine is far from the player — should speed up (positive adjustment)
            float vineAnchorX = 10f;
            float vineAnchorY = 10f;
            // Player is far to the left of the vine
            float playerX = 0f;
            float playerY = 5f;
            
            float result = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, config, 0.1f,
                playerX, playerY, vineAnchorX, vineAnchorY);
            Assert.True(result > 0f, "Adjustment should be positive (speed up) when vine is far from player");
        }
        
        [Fact]
        public void AdjustmentSmallWhenVineCloseToPlayer() {
            var config = TestHelpers.DefaultConfig();
            float vineAnchorX = 10f;
            float vineAnchorY = 10f;
            // Player is right at the vine endpoint (hanging straight down)
            float playerX = vineAnchorX;
            float playerY = vineAnchorY - config.RopeLength;
            
            float resultClose = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, config, 0.1f,
                playerX, playerY, vineAnchorX, vineAnchorY);
            float resultFar = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, config, 0.1f,
                0f, 0f, vineAnchorX, vineAnchorY);
            
            Assert.True(Math.Abs(resultClose) < Math.Abs(resultFar),
                "Adjustment magnitude should be smaller when vine is close to player");
        }

        [Fact]
        public void AdjustmentZeroWhenVineVeryCloseToPlayer() {
            var config = TestHelpers.DefaultConfig();
            float vineAnchorX = 10f;
            float vineAnchorY = 10f;
            // Player is exactly at vine endpoint — distance ≈ 0, speedMultiplier clamped to 1.0 → no additional adjustment
            float playerX = vineAnchorX;
            float playerY = vineAnchorY - config.RopeLength;
            
            float result = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, config, 0.1f,
                playerX, playerY, vineAnchorX, vineAnchorY);
            Assert.Equal(0f, result);
        }

        [Fact]
        public void AdjustmentAccumulatesAcrossFrames() {
            var config = TestHelpers.DefaultConfig();
            float vineAnchorX = 10f;
            float vineAnchorY = 10f;
            float playerX = 0f;
            float playerY = 5f;
            
            float oneFrameAdjustment = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, config, 0.1f,
                playerX, playerY, vineAnchorX, vineAnchorY);
            float twoFrameAdjustment = VineAttraction.UpdatePhaseAdjustment(oneFrameAdjustment, 0f, 2f, 0f, config, 0.1f,
                playerX, playerY, vineAnchorX, vineAnchorY);
            Assert.True(Math.Abs(twoFrameAdjustment) > Math.Abs(oneFrameAdjustment));
        }

        [Fact]
        public void AdjustmentCappedAtMax() {
            var config = TestHelpers.DefaultConfig();
            float result = VineAttraction.UpdatePhaseAdjustment(config.MaxPhaseAdjustment - 0.001f, 0f, 2f, 0f, config, 0.1f,
                0f, 0f, 10f, 10f);
            Assert.True(result <= config.MaxPhaseAdjustment);
        }

        [Fact]
        public void NoChangeWhenDeltaTimeIsZero() {
            var config = TestHelpers.DefaultConfig();
            float result = VineAttraction.UpdatePhaseAdjustment(0.5f, 0f, 2f, 0f, config, 0f,
                0f, 0f, 10f, 10f);
            Assert.Equal(0.5f, result);
        }

        [Fact]
        public void HigherMaxSpeedMultiplierGivesLargerAdjustment() {
            var lowConfig = TestHelpers.DefaultConfig(maxSpeedMultiplier: 1.5f);
            var highConfig = TestHelpers.DefaultConfig(maxSpeedMultiplier: 5f);
            
            float resultLow = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, lowConfig, 0.1f,
                0f, 0f, 10f, 10f);
            float resultHigh = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, highConfig, 0.1f,
                0f, 0f, 10f, 10f);
            Assert.True(resultHigh > resultLow);
        }

        [Fact]
        public void SmallerReferenceDistanceGivesLargerAdjustmentWhenFar() {
            // Smaller reference distance means the same physical distance is "further" in relative terms
            var largeRefConfig = TestHelpers.DefaultConfig(attractionReferenceDistance: 10f);
            var smallRefConfig = TestHelpers.DefaultConfig(attractionReferenceDistance: 1f);
            
            float resultLargeRef = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, largeRefConfig, 0.1f,
                0f, 0f, 10f, 10f);
            float resultSmallRef = VineAttraction.UpdatePhaseAdjustment(0f, 0f, 2f, 0f, smallRefConfig, 0.1f,
                0f, 0f, 10f, 10f);
            Assert.True(resultSmallRef > resultLargeRef);
        }

        [Fact]
        public void GrabSucceedsWhenAttractionAdjustmentBringsVineIntoRange() {
            var config = TestHelpers.DefaultConfig(grabRadius: 1.5f, ropeLength: 5f, grabLookaheadFrames: 0);
            float[] vineXPositions = { 0f, 10f };
            float[] vinePhases = { 0f, (float)Math.PI / 2f };
            float[] vinePeriods = { 2f, 100f };
            float vineAnchorYPosition = 100f;

            float playerXPosition = 10f;
            float playerYPosition = vineAnchorYPosition - config.RopeLength;
            
            int resultWithoutAdjustment = GrabEvaluator.CheckGrab(playerXPosition, playerYPosition, vineXPositions, vineAnchorYPosition, config, 1, vinePhases, vinePeriods, 0f, 0f, 0f);
            
            int resultWithAdjustment = GrabEvaluator.CheckGrab(playerXPosition, playerYPosition, vineXPositions, vineAnchorYPosition, config, 1, vinePhases, vinePeriods, 0f, 0f, 0f, attractionTargetVineIndex: 1, attractionPhaseAdjustment: -MathF.PI/2f);
            
            Assert.Equal(-1, resultWithoutAdjustment);
            Assert.Equal(1, resultWithAdjustment);
        }

        [Fact]
        public void AttractionAccumulatesWhileAirborneAndMovingForwards() {
            var config = TestHelpers.DefaultConfig(vineSpacing: 20f, minimumReleaseVelocityX: 5f);
            float[] vineXPositions = { 0f, 10f };
            float[] vinePhases = { 0f, (float)Math.PI / 2f };
            float[] vinePeriods = { 2f, 100f };
            var stateMachine = new PlayerStateMachine(config, vineXPositions, 10f, vinePhases, vinePeriods);
            stateMachine.Start(0);
            stateMachine.Update(0.01f, true); //move to airborne state
            
            Assert.Equal(PlayerStateType.Airborne, stateMachine.PlayerContext.CurrentStateType);
            Assert.True(stateMachine.PlayerContext.VelocityX > 0);
            
            stateMachine.Update(0.1f, false);
            Assert.True(stateMachine.PlayerContext.VineAttractionPhaseAdjustment != 0f);
            Assert.Equal(1, stateMachine.PlayerContext.AttractionTargetVineIndex);
        }

        [Fact]
        public void AttractionResetsOnVineGrab() {
            var config = TestHelpers.DefaultConfig(vineSpacing: 20f, grabRadius: 2f);
            float[] vineXPositions = { 0f, 20f };
            float[] vinePhases = { 0f, 0f };
            float[] vinePeriods = { 2f, 2f };
            var stateMachine = new PlayerStateMachine(config, vineXPositions, 10f, vinePhases, vinePeriods);
            stateMachine.Start(0);
            stateMachine.Update(0.01f, true); //move to airborne state

            stateMachine.PlayerContext.PositionX = 20f;
            stateMachine.PlayerContext.PositionY = 10f - config.RopeLength;
            stateMachine.PlayerContext.VelocityX = 1f;
            stateMachine.Update(0.01f, false);
            
            Assert.Equal(PlayerStateType.Swinging, stateMachine.PlayerContext.CurrentStateType);
            Assert.Equal(0f, stateMachine.PlayerContext.VineAttractionPhaseAdjustment);
            Assert.Equal(-1, stateMachine.PlayerContext.AttractionTargetVineIndex);
        }

        [Fact]
        public void AttractionResetsOnFall() {
            var config = TestHelpers.DefaultConfig(vineSpacing: 20f, fallThresholdY: -20f);
            float[] vineXPositions = { 0f, 20f };
            float[] vinePhases = { 0f, MathF.PI/2f };
            float[] vinePeriods = { 2f, 100f };
            
            var stateMachine = new PlayerStateMachine(config, vineXPositions, 10f, vinePhases, vinePeriods);
            stateMachine.Start(0);
            stateMachine.Update(0.01f, true);
            
            stateMachine.PlayerContext.PositionY = -21f;
            stateMachine.PlayerContext.VelocityX = 1f;
            stateMachine.Update(0.01f, false);
            
            Assert.Equal(PlayerStateType.Falling, stateMachine.PlayerContext.CurrentStateType);
            Assert.Equal(0f, stateMachine.PlayerContext.VineAttractionPhaseAdjustment);
            Assert.Equal(-1, stateMachine.PlayerContext.AttractionTargetVineIndex);
        }

        [Fact]
        public void AttractionOnlyTargetsNextVine() {
            var config = TestHelpers.DefaultConfig(vineSpacing: 20f, minimumReleaseVelocityX: 5f);
            float[] vineXPositions = { 0f, 20f, 40f };
            float[] vinePhases = { 0f, 0f, MathF.PI / 2f };
            float[] vinePeriods = { 2f, 2f, 100f};
            var stateMachine = new PlayerStateMachine(config, vineXPositions, 10f, vinePhases, vinePeriods);
            stateMachine.Start(0);
            stateMachine.Update(0.01f, true);
            
            stateMachine.Update(0.1f, false);
            Assert.Equal(1, stateMachine.PlayerContext.AttractionTargetVineIndex);
        }

        [Fact]
        public void NoAttractionWhenMovingBackwards() {
            var config = TestHelpers.DefaultConfig(vineSpacing: 20f);
            float[] vineXPositions = { 0f, 20f };
            float[] vinePhases = { 0f, MathF.PI/2f };
            float[] vinePeriods = { 2f, 100f };
            
            var stateMachine = new PlayerStateMachine(config, vineXPositions, 10f, vinePhases, vinePeriods);
            stateMachine.Start(0);
            stateMachine.Update(0.01f, true);

            stateMachine.PlayerContext.VelocityX = -5f;
            stateMachine.Update(0.1f, false);
            
            Assert.Equal(-1, stateMachine.PlayerContext.AttractionTargetVineIndex);
        }
        
        [Fact]
        public void NoAttractionOnLastVine() {
            var config = TestHelpers.DefaultConfig(vineSpacing: 20f, minimumReleaseVelocityX: 5f);
            float[] vineXPositions = { 0f, 20f };
            float[] vinePhases = { 0f, 0f };
            float[] vinePeriods = { 2f, 2f };
            var stateMachine = new PlayerStateMachine(config, vineXPositions, 10f, vinePhases, vinePeriods);
            stateMachine.Start(0);
            stateMachine.Update(0.01f, true);

            stateMachine.PlayerContext.PositionX = 20f;
            stateMachine.PlayerContext.PositionY = 10f - config.RopeLength;
            stateMachine.PlayerContext.VelocityX = 1f;
            stateMachine.Update(0.01f, false);
            Assert.Equal(1, stateMachine.PlayerContext.CurrentVineIndex);
            
            stateMachine.Update(0.01f, true);
            Assert.Equal(PlayerStateType.Airborne, stateMachine.PlayerContext.CurrentStateType);

            stateMachine.Update(0.1f, false);
            Assert.Equal(-1, stateMachine.PlayerContext.AttractionTargetVineIndex);
            Assert.Equal(0f, stateMachine.PlayerContext.VineAttractionPhaseAdjustment);
        }

        [Fact]
        public void SwingPhaseIncludesAdjustmentOnGrab() {
            var config = TestHelpers.DefaultConfig(vineSpacing: 20f, grabRadius: 10f, minimumReleaseVelocityX: 5f);
            float[] vineXPositions = { 0f, 20f };
            float vinePhaseOffset = MathF.PI / 4f;
            float[] vinePhases = { 0f, vinePhaseOffset };
            float[] vinePeriods = { 2f, 100f };
            var stateMachine = new PlayerStateMachine(config, vineXPositions, 10f, vinePhases, vinePeriods);
            stateMachine.Start(0);
            stateMachine.Update(0.01f, true);
            
            stateMachine.PlayerContext.VelocityX = 5f;
            stateMachine.PlayerContext.VelocityY = 0f;
            stateMachine.PlayerContext.PositionY = 5f;
            stateMachine.Update(0.1f, false);

            stateMachine.PlayerContext.VelocityX = 5f;
            stateMachine.PlayerContext.VelocityY = 0f;
            stateMachine.PlayerContext.PositionY = 5f;
            stateMachine.Update(0.1f, false);
            
            stateMachine.PlayerContext.PositionX = 20f;
            stateMachine.PlayerContext.PositionY = 10f - config.RopeLength;
            stateMachine.PlayerContext.VelocityX = 1f;
            stateMachine.PlayerContext.VelocityY = 0f;
            
            var stateMachine2 = new PlayerStateMachine(config, vineXPositions, 10f, vinePhases, vinePeriods);
            stateMachine2.Start(0);
            stateMachine2.Update(0.1f, true);
            stateMachine2.PlayerContext.VelocityX = 5f;
            stateMachine2.PlayerContext.PositionY = 5f;
            
            stateMachine2.PlayerContext.PositionX = 20f;
            stateMachine2.PlayerContext.PositionY = 10f - config.RopeLength;
            stateMachine2.PlayerContext.VelocityX = 1f;
            stateMachine2.PlayerContext.VelocityY = 0f;
            
            stateMachine.Update(0.01f, false);
            Assert.Equal(PlayerStateType.Swinging, stateMachine.PlayerContext.CurrentStateType);

            float basePhase = vinePhaseOffset + (2 * MathF.PI / vinePeriods[1]) * stateMachine.ElapsedTime;
            float phaseDifference = stateMachine.PlayerContext.SwingPhase - basePhase;
            Assert.NotEqual(0f, phaseDifference);
        }
    }
}