using Minigames.Swinging.Core;

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
            int grabLookaheadFrames = 3,
            float minimumReleaseVelocityX = 1f
        ) {
            return new SwingConfig
            {
                Amplitude = amplitude,
                RopeLength = ropeLength,
                Period = period,
                LaunchForce = launchForce,
                GrabRadius = grabRadius,
                FallThresholdY = fallThresholdY,
                RespawnDelay = respawnDelay,
                VineSpacing = vineSpacing,
                Gravity = gravity,
                CoinsPerGap = coinsPerGap,
                VineScoreValue = vineScoreValue,
                CoinBaseHeightRatio = 0.5f,
                GrabLookaheadFrames = grabLookaheadFrames,
                MinimumReleaseVelocityX = minimumReleaseVelocityX,
                ReleaseCurveExponent = SwingConfig.DefaultReleaseCurveExponent,
                ReleaseLookaheadFrames = 4,
                PhaseChainOffset = 0f,
                VerticalLaunchScale = 1f
            };
        }
    }
}