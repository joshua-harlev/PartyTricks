using CoreData;
using Minigames.Swinging.Core;
using UnityEngine;

namespace Minigames.Swinging {
    [CreateAssetMenu(fileName = "VineSwingingPlayerStatsSO", menuName = "Scriptable Objects/VineSwingingPlayerStatsSO")]
    public class VineSwingingPlayerStatsSO : ScriptableObject {
        [Header("Swing Settings")] 
        [SerializeField] public float Amplitude = 1.4f;
        [SerializeField] public float RopeLength = 4f;
        [SerializeField] public float Period = 3.5f;
        [SerializeField] public float LaunchForce = 1.3f;
        [SerializeField] public float GrabRadius = 2.5f;
        [SerializeField] public float VineSpacing = 10f;
        [SerializeField] public float Gravity = 15f;
    
        [Header("Fall/Respawn")] 
        [SerializeField] public float FallThresholdY = -8f;
        [SerializeField] public float RespawnDelayInSeconds = 1f;
    
        [Header("Coins")]
        [SerializeField] public int CoinsPerGap = 5;
        [SerializeField] public int VineScoreValue = 5;
        [SerializeField] public CoinTypeSO[] CoinTypes;
        [Tooltip("How high do the coins spawn? Higher values cause the coins to spawn lower, lower values -> higher spawns.")]
        [Range(0f, 1f)]
        [SerializeField] public float CoinBaseHeightRatio = 0.5f;
    
        [Header("Powerups")]
        [SerializeField] public float MagnetRadius = 3f;
        [SerializeField] public float MagnetPullSpeed = 8f;
        [SerializeField] public float GrabRadiusPercentBoostPerMoveBoost = 0.10f;
        [SerializeField] [Range(0,1)] public float VerticalScalePerMoveBoost = 0.78f;
    
        [Header("Lookahead")]
        [Tooltip("Applies on move boost powerup")] 
        [SerializeField] public int GrabLookaheadFramesPerBoost = 2;
        [SerializeField] public int ReleaseLookaheadFramesPerBoost = 2;

        [Header("Release Forgiveness")]
        [SerializeField] public float MinimumReleaseVelocityX = 0.2f;

        [Header("Phase Tuning")] [SerializeField]
        [Tooltip("Nudge chained vine phases earlier (+) or later (-). In radians.")]
        [Range(-1f, 1f)]
        public float PhaseChainOffset = 0f;
    
        public SwingConfig CreateConfig(MovementModifiers movementModifiers, int coinsPerGapBoost) {
            float modifiedPeriod = Period;
            float modifiedRespawnDelay = RespawnDelayInSeconds;
            float modifiedLaunchForce = LaunchForce;
            float modifiedGrabRadius = GrabRadius;
            float modifiedVerticalScale = 1f;
            // decrease period by 18% for each move modifier; swing faster
            // decrease respawn delay by 30% for each move modifier; respawn faster
            // increase grab radius by specified % for each move modifier; grab more easily
            // decrease launch force to partially compensate for velocity increase from period
            // decrease vertical launch scale to flatten arc and avoid overshooting
            for (int i = 0; i < movementModifiers.MoveBoostCount; i++) {
                modifiedPeriod *= 0.82f;
                modifiedRespawnDelay *= 0.70f;
                modifiedGrabRadius *= 1f + GrabRadiusPercentBoostPerMoveBoost;
                modifiedLaunchForce *= 0.85f;
                modifiedVerticalScale *= VerticalScalePerMoveBoost;
            }

            int modifiedCoinsPerGap = CoinsPerGap + coinsPerGapBoost;
            int grabLookaheadFrames = movementModifiers.MoveBoostCount * GrabLookaheadFramesPerBoost;
            int releaseLookaheadFrames = 4 + movementModifiers.MoveBoostCount * ReleaseLookaheadFramesPerBoost;

            return new SwingConfig(Amplitude, RopeLength, modifiedPeriod, modifiedLaunchForce, modifiedGrabRadius, FallThresholdY,
                modifiedRespawnDelay, VineSpacing, Gravity, modifiedCoinsPerGap, VineScoreValue,
                grabLookaheadFrames, MinimumReleaseVelocityX, releaseLookaheadFrames: releaseLookaheadFrames,
                phaseChainOffset: PhaseChainOffset, coinBaseHeightRatio: CoinBaseHeightRatio, verticalLaunchScale: modifiedVerticalScale);
        }
    }
}
