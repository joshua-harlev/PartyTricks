namespace Minigames.DireDodging {
    public struct DireDodgingIntensityStats {
        public float ProjectileSpeedIncrease { get; private set; }
        public float ProjectileScaleIncrease { get; private set; }
        public float ShootRateDivisor { get; private set; }
        public float ChargeTimeDecrease { get; private set; }

        public DireDodgingIntensityStats(float projectileSpeedIncrease, float projectileScaleIncrease, float shootRateDivisor, float chargeTimeDecrease) {
            ProjectileSpeedIncrease = projectileSpeedIncrease;
            ProjectileScaleIncrease = projectileScaleIncrease;
            ShootRateDivisor = shootRateDivisor;
            ChargeTimeDecrease = chargeTimeDecrease;
        }
    }
}