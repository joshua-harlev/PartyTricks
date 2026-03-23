namespace VineSwinging.Core {
    public readonly struct SwingConfig {
        public readonly float Amplitude;
        public readonly float RopeLength;
        public readonly float Period;
        public readonly float LaunchForce;
        public readonly float GrabRadius;
        public readonly float FallThresholdY;
        public readonly float RespawnDelay;
        public readonly float VineSpacing;
        public readonly float Gravity;

        public readonly int CoinsPerGap;
        public readonly int VineScoreValue;
        public readonly float CoinBaseHeightRatio;
        public readonly float CoinArcHeight;

        public readonly int GrabLookaheadFrames;
        public readonly float MinimumReleaseVelocityX;
        public readonly float ReleaseCurveExponent;
        public readonly int ReleaseLookaheadFrames;
        
        public readonly float PhaseChainOffset;
        public readonly float VerticalLaunchScale;

        public SwingConfig(float amplitude, float ropeLength, float period, float launchForce, float grabRadius,
            float fallThresholdY, float respawnDelay, float vineSpacing, float gravity, int coinsPerGap,
            int vineScoreValue, float coinArcHeight, int grabLookaheadFrames, float minimumReleaseVelocityX,
            float releaseCurveExponent = 0.6f, float phaseChainOffset = 0f, int releaseLookaheadFrames = 4, float coinBaseHeightRatio = 0.5f, float verticalLaunchScale = 1.0f) {
            Amplitude = amplitude;
            RopeLength = ropeLength;
            Period = period;
            LaunchForce = launchForce;
            GrabRadius = grabRadius;
            FallThresholdY = fallThresholdY;
            RespawnDelay = respawnDelay;
            VineSpacing = vineSpacing;
            Gravity = gravity;
            CoinsPerGap = coinsPerGap;
            VineScoreValue = vineScoreValue;
            CoinArcHeight = coinArcHeight;
            GrabLookaheadFrames = grabLookaheadFrames;
            MinimumReleaseVelocityX = minimumReleaseVelocityX;
            ReleaseCurveExponent = releaseCurveExponent;
            ReleaseLookaheadFrames = releaseLookaheadFrames;
            PhaseChainOffset = phaseChainOffset;
            CoinBaseHeightRatio = coinBaseHeightRatio;
            VerticalLaunchScale = verticalLaunchScale;
        }
    }
}