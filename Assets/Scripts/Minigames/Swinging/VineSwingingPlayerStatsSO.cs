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
    [SerializeField] [Range(0, 0.3f)] [Tooltip("DISABLED: How much variation should be present in vine swing positions?")]
    public float PeriodVariation = 0f;
    
    [Header("Fall/Respawn")] 
    [SerializeField] public float FallThresholdY = -8f;
    [SerializeField] public float RespawnDelayInSeconds = 1f;
    
    [Header("Coins")]
    [SerializeField] public int CoinsPerGap = 5;
    [SerializeField] public int VineScoreValue = 5;
    [SerializeField] public CoinTypeSO[] CoinTypes;
    [SerializeField] public float CoinArcHeight = 2f;
    
    [Header("Powerups")]
    [SerializeField] public float MagnetRadius = 3f;
    [SerializeField] public float MagnetPullSpeed = 8f;
    [SerializeField] public float GrabRadiusPercentBoostPerMoveBoost = 0.05f;
    
    [Header("Grab Lookahead")]
    [Tooltip("Applies on move boost powerup")] 
    [SerializeField] public int GrabLookaheadFramesPerBoost = 8;

    [Header("Release Forgiveness")]
    [SerializeField] public float MinimumReleaseVelocityX = 0.2f;
    
    public SwingConfig CreateConfig(MovementModifiers movementModifiers, int coinsPerGapBoost) {
        float modifiedPeriod = Period;
        float modifiedRespawnDelay = RespawnDelayInSeconds;
        float modifiedLaunchForce = LaunchForce;
        float modifiedGrabRadius = GrabRadius;
        // decrease period by 20% for each move modifier; swing faster
        // decrease respawn delay by 30% for each move modifier; respawn faster
        // increase launch force by 3% for each move modifier; launch further
        // increase grab radius by specified % for each move modifier; grab more easily
        for (int i = 0; i < movementModifiers.MoveBoostCount; i++) {
            modifiedPeriod *= 0.82f;
            modifiedRespawnDelay *= 0.70f;
            modifiedLaunchForce *= 1.01f;
            modifiedGrabRadius *= 1f + GrabRadiusPercentBoostPerMoveBoost;
        }

        int modifiedCoinsPerGap = CoinsPerGap + coinsPerGapBoost;
        int grabLookaheadFrames = movementModifiers.MoveBoostCount * GrabLookaheadFramesPerBoost;

        return new SwingConfig(Amplitude, RopeLength, modifiedPeriod, modifiedLaunchForce, modifiedGrabRadius, FallThresholdY,
            modifiedRespawnDelay, VineSpacing, Gravity, modifiedCoinsPerGap, VineScoreValue, CoinArcHeight, grabLookaheadFrames, MinimumReleaseVelocityX);
    }
}
