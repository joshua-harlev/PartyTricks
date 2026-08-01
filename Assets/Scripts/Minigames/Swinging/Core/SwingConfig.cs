namespace Minigames.Swinging.Core {
    public readonly struct SwingConfig {
        public const float DefaultReleaseCurveExponent = 0.6f;
        
        public float Amplitude { get; init; }
        public float RopeLength { get; init; }
        public float Period { get; init; }
        public float LaunchForce { get; init; }
        public float GrabRadius { get; init; }
        public float FallThresholdY { get; init; }
        public float RespawnDelay { get; init; }
        public float VineSpacing { get; init; }
        public float Gravity { get; init; }

        public int CoinsPerGap { get; init; }
        public int VineScoreValue { get; init; }
        public float CoinBaseHeightRatio { get; init; }

        public int GrabLookaheadFrames { get; init; }
        public float MinimumReleaseVelocityX { get; init; }
        public float ReleaseCurveExponent { get; init; }
        public int ReleaseLookaheadFrames { get; init; }
        
        public float PhaseChainOffset { get; init; }
        public float VerticalLaunchScale { get; init; }
    }
}