using FMODUnity;
using UnityEngine;

namespace Minigames.CoinTilt {
    [CreateAssetMenu(fileName = "CoinTiltPlayerStatsSO", menuName = "Scriptable Objects/Coin Tilt Player Stats")]
    public class CoinTiltPlayerStatsSO : ScriptableObject {
        [Header("Movement Settings")] 
        public float MoveSpeed = 15f;
        public float Acceleration = 7f;
        public float SlipFactor = 1.7f;
        public float TurnSpeed = 10f;
        
        [Header("Jump Settings")]
        public float JumpForce = 8f;
        public float AirControlMultiplier = 1;
        public float GravityScale = 3f;
        public float CoyoteTimeInSeconds = 0.15f;
        public float MomentumCancelPercentageRegular = 0.5f;
        public float MomentumCancelPercentageBoosted = 0.75f;
    
        [Header("Fall Settings")] 
        public float FallThresholdY = -10f;
        public float RespawnDelayInSeconds = 0.75f;
        public float ShrinkAnimationDurationInSeconds = 0.5f;

        [Header("Magnet Settings")] 
        public float MagnetRadiusBoostFactor = 0.4f;
        public float MagnetPullSpeedBoostFactor = 0.5f;

        [Header("Move Boost Settings")] 
        public float AirControlIncreasePerMoveBoost = 0.3f;
        public float SlipFactorIncreasePerMoveBoost = 1.3f;

        [Header("SFX")] 
        public EventReference FallSound;
    }
}
