using System;
using Minigames.Swinging.Core;
using Minigames.Swinging.Core.PlayerStateMachine;
using Xunit;

namespace Swinging.Core.Tests {
    public class SwingSimulationTests {
        private const float Tolerance = 0.001f;

        [Theory]
        [InlineData(0.8f, 5f)]
        [InlineData(0f, 5f)]
        [InlineData(0.8f, 0f)]
        [InlineData(0f, 0f)]
        public void SwingPosition_AtPhaseZero_IsStraightDown(float amplitude, float ropeLength) {
            var (offsetX, offsetY) = SwingSimulation.GetSwingPosition(
                phase: 0f, amplitude, ropeLength);
            
            Assert.Equal(0f, offsetX, Tolerance);
            Assert.Equal(-ropeLength, offsetY, Tolerance);
        }

        [Fact]
        public void SwingPositions_AtOppositePhases_HaveMirroredXValues() {
            const float phase = 1.0f;
            var (leftX, leftY) = SwingSimulation.GetSwingPosition(phase, 0.8f, 5f);
            var (rightX, rightY) = SwingSimulation.GetSwingPosition(-phase, 0.8f, 5f);
            
            Assert.Equal(-rightX, leftX, Tolerance);
            Assert.Equal(rightY, leftY, Tolerance);
        }

        [Fact]
        public void ReleaseVelocity_LaunchForceScalesOutput() {
            var (xVelocity1, yVelocity1) = SwingSimulation.GetShapedReleaseVelocity(phase: 0f, amplitude: 0.8f, period: 2f, launchForce: 1f, ropeLength: 5f, curveExponent: 1f);
            var (xVelocity2, yVelocity2) = SwingSimulation.GetShapedReleaseVelocity(phase: 0f, amplitude: 0.8f, period: 2f, launchForce: 2f, ropeLength: 5f, curveExponent: 1f);
            
            Assert.Equal(xVelocity1*2f, xVelocity2, Tolerance);
            Assert.Equal(yVelocity1*2f, yVelocity2, Tolerance);
        }

        [Fact]
        public void ReleaseVelocity_IsHorizontalAtPhaseZero() {
            var (xVelocity, yVelocity) = SwingSimulation.GetShapedReleaseVelocity(phase: 0f, amplitude: 0.8f, period: 2f,
                launchForce: 1f, ropeLength: 5f, curveExponent: 1f);
            
            Assert.True(xVelocity > 0f);
            Assert.Equal(0f, yVelocity, Tolerance);
        }

        [Fact]
        public void ReleaseVelocity_AtPeakSwing_IsZero() {
            float peakPhase = (float)(Math.PI / 2.0f);
            var (xVelocity, yVelocity) = SwingSimulation.GetShapedReleaseVelocity(phase: peakPhase, amplitude: 0.8f,
                period: 2f,
                launchForce: 1f, ropeLength: 5f, curveExponent: 1f);
            
            Assert.Equal(0f, xVelocity, Tolerance);
            Assert.Equal(0f, yVelocity, Tolerance);
        }

        [Fact]
        public void EstimateHorizontalDistance_NegativeDiscriminantReturnsZero() {
            float result = SwingingState.EstimateHorizontalDistance(5f, -1f, 5f, 20f);
            Assert.Equal(0f, result);
        }
    }
}