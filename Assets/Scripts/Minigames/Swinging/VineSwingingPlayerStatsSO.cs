using CoreData;
using UnityEngine;
using VineSwinging.Core;

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
    [SerializeField] [Range(0, 0.3f)] [Tooltip("How much variation should be present in vine swing positions?")]
    public float PeriodVariation = 0.1f;
    
    [Header("Fall/Respawn")] 
    [SerializeField] public float FallThresholdY = -8f;
    [SerializeField] public float RespawnDelayInSeconds = 1f;
    
    [Header("Coins")]
    [SerializeField] public int CoinsPerGap = 5;
    [SerializeField] public int VineScoreValue = 5;
    [SerializeField] public CoinTypeSO[] CoinTypes;
    [SerializeField] public float CoinArcHeight = 2f;
    
    [Header("Magnet")]
    [SerializeField] public float MagnetRadius = 3f;
    [SerializeField] public float MagnetPullSpeed = 8f;
    
    [Header("Grab Lookahead")]
    [Tooltip("Applies on move boost powerup")] 
    [SerializeField] public int GrabLookaheadFramesPerBoost = 8;

    [Header("Release Forgiveness")]
    [SerializeField] public float MinimumReleaseVelocityX = 0.2f;
    
    [Header("Vine Attraction")]
    [SerializeField] public float AttractionReferenceDistance = 2.5f;
    [SerializeField] public float MaxSpeedMultiplier = 1.05f;
    [SerializeField] public float MaxPhaseAdjustment = 1.571f;
    [SerializeField] public float AttractionBoostPerMoveBoost = 0.5f;
    
    public SwingConfig CreateConfig(MovementModifiers movementModifiers, int coinsPerGapBoost) {
        float modifiedPeriod = Period;
        float modifiedRespawnDelay = RespawnDelayInSeconds;
        float modifiedLaunchForce = LaunchForce;
        // decrease period for each move modifier; swing faster
        // decrease respawn delay for each move modifier; respawn faster
        // increase launch force for each move modifier; move further
        for (int i = 0; i < movementModifiers.MoveBoostCount; i++) {
            modifiedPeriod *= 0.82f;
            modifiedRespawnDelay *= 0.70f;
            modifiedLaunchForce *= 1.03f;
        }

        int modifiedCoinsPerGap = CoinsPerGap + coinsPerGapBoost;
        
        float periodRatio = modifiedPeriod / Period;
        modifiedLaunchForce *= periodRatio;
        int grabLookaheadFrames = movementModifiers.MoveBoostCount * GrabLookaheadFramesPerBoost;

        float scaledMaxSpeedMultiplier = MaxSpeedMultiplier + movementModifiers.MoveBoostCount * AttractionBoostPerMoveBoost;

        return new SwingConfig(Amplitude, RopeLength, modifiedPeriod, modifiedLaunchForce, GrabRadius, FallThresholdY,
            modifiedRespawnDelay, VineSpacing, Gravity, modifiedCoinsPerGap, VineScoreValue, CoinArcHeight,
            grabLookaheadFrames, MinimumReleaseVelocityX, AttractionReferenceDistance,
            scaledMaxSpeedMultiplier, MaxPhaseAdjustment);
    }
}
