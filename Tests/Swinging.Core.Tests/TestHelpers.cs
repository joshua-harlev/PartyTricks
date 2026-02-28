using VineSwinging.Core;

namespace Swinging.Core.Tests {
    public static class TestHelpers {
        public static SwingConfig DefaultConfig(
            float amplitude = 0.8f,
            float ropeLength = 5f, 
            float period = 2f,
            float launchForce = 1f,
            float grabRadius = 1.5f,
            float fallThresholdY = -20f,
            float respawnDelay = 1f,
            float vineSpacing = 10f,
            float gravity = 9.81f,
            int coinsPerGap = 3,
            int vineScoreValue = 10,
            float coinArcHeight = 2f,
            int grabLookaheadFrames = 3,
            float minimumReleaseVelocityX = 1f,
            float attractionReferenceDistance = 2.5f,
            float maxSpeedMultiplier = 2.5f,
            float maxPhaseAdjustment = 1.571f // pi/2
        ) {
            return new SwingConfig(amplitude, ropeLength, period, launchForce, grabRadius, fallThresholdY, respawnDelay,
                vineSpacing, gravity, coinsPerGap, vineScoreValue, coinArcHeight, grabLookaheadFrames,
                minimumReleaseVelocityX, attractionReferenceDistance, maxSpeedMultiplier, maxPhaseAdjustment);
        }
    }
}