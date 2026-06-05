namespace Minigames.DireDodging {
    public struct DireDodgingIntensityStats {
        public float ProjectileSpeedIncrease { get; private set; }
        public float ProjectileScaleIncrease { get; private set; }
        public float ShootRateMultiplierAtMaxIntensity { get; private set; }
        public float ChargeTimeDecrease { get; private set; }

        public DireDodgingIntensityStats(float projectileSpeedIncrease, float projectileScaleIncrease, float shootRateMultiplierAtMaxIntensity, float chargeTimeDecrease) {
            ProjectileSpeedIncrease = projectileSpeedIncrease;
            ProjectileScaleIncrease = projectileScaleIncrease;
            ShootRateMultiplierAtMaxIntensity = shootRateMultiplierAtMaxIntensity;
            ChargeTimeDecrease = chargeTimeDecrease;
        }
    }
}