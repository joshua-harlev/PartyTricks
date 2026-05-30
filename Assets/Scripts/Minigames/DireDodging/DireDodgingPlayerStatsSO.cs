using FMODUnity;
using UnityEngine;

namespace Minigames.DireDodging {
    [CreateAssetMenu(fileName = "DireDodgingPlayerStatsSO", menuName = "Scriptable Objects/DireDodgingPlayerStatsSO")]
    public class DireDodgingPlayerStatsSO : ScriptableObject {
        [Header("Movement Settings")] 
        public float MoveSpeed = 15f;
        public float ProjectileScale = 1f;
        public float ProjectileSpeed = 15f;
        public float ProjectileShootRate = 1f;
        public float BaseDamage = 1f;
        public float BaseHealth = 15f;
        public float DamageAnimationTimeInSeconds = 0.2f;
        public float DeathAnimationTimeInSeconds = 0.05f;
    
        [Header("Charge Attack Settings")]
        public float ChargeTimeRequired = 2f;
        public float ChargedProjectileScale = 2f;
        public float ChargedProjectileSpeed = 2f;
        public float ChargedProjectileSpeedReductionPerPowerup = 0.75f;
    
        [Header("Ghost Mode Settings")]
        public float GhostChargeTime = 1f;
        public float GhostProjectileSpeed = 1.2f;
        public float GhostMoveSpeedMultiplier = 0.3f;
    
        [Header("Stun Settings")]
        public float StunDuration = 1f;

        [Header("Intensity Multipliers")]
        public float ProjectileSpeedIncrease = 2.5f;
        public float ProjectileScaleIncrease = 2f;
        public float ShootRateDivisor = 0.3f; //TODO make this easier to deal with
        [Tooltip("Not directly scaled; see code.")]
        public float ChargeTimeDecrease = 1.5f; 

        [Header("Sound Events")] 
        public EventReference GetHitEvent;
        public EventReference DeathEvent;
        public EventReference ChargeLoopEvent;
        public EventReference ChargeReleaseEvent;
        public EventReference ChargeShootEvent;
        public EventReference ChargeCompleteEvent;
        public EventReference BasicShootEvent;

        public DireDodgingIntensityStats GetIntensityStats() {
            return new DireDodgingIntensityStats(ProjectileSpeedIncrease, ProjectileScaleIncrease, ShootRateDivisor,
                ChargeTimeDecrease);
        }
    }
}
