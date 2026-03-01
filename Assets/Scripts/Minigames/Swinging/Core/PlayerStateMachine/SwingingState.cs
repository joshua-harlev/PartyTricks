using System;

namespace VineSwinging.Core {
    public class SwingingState : IPlayerState {
        private readonly PlayerStateMachine playerStateMachine;
        private const float FrameDuration = 1f / 60f;

        public SwingingState(PlayerStateMachine playerStateMachine) {
            this.playerStateMachine = playerStateMachine;
        }
        public void Enter(PlayerContext playerContext, SwingConfig swingConfig) {
            playerContext.CurrentStateType = PlayerStateType.Swinging;
            float vinePeriod = playerStateMachine.VinePeriods[playerContext.CurrentVineIndex];
            playerContext.SwingPhase = playerStateMachine.VinePhaseOffsets[playerContext.CurrentVineIndex]
                + (float)(2*Math.PI / vinePeriod) * playerStateMachine.ElapsedTime;
        }

        public void Update(PlayerContext playerContext, SwingConfig swingConfig, float deltaTime, bool releasePressed) {
            float vinePeriod = playerStateMachine.VinePeriods[playerContext.CurrentVineIndex];
            playerContext.SwingPhase += (float)(((2 * Math.PI) / vinePeriod) * deltaTime);
            var positionOffset = SwingSimulation.GetSwingPosition(playerContext.SwingPhase, swingConfig.Amplitude,
                swingConfig.RopeLength);
            playerContext.PositionX = playerStateMachine.VineXPositions[playerContext.CurrentVineIndex] + positionOffset.offsetX;
            playerContext.PositionY = playerStateMachine.VineAnchorY + positionOffset.offsetY;
            float playerSwingAngle = swingConfig.Amplitude * (float)Math.Sin(playerContext.SwingPhase);
            playerContext.SwingAngle = playerSwingAngle;
            
            if(releasePressed) Release(playerContext, swingConfig, vinePeriod);
        }

        private void Release(PlayerContext playerContext, SwingConfig swingConfig, float vinePeriod) {
                playerContext.VelocityX = Math.Max(releaseVelocity.vx, swingConfig.MinimumReleaseVelocityX);
            playerContext.PendingEvents.Add(PlayerEvent.Launched);
            playerStateMachine.TransitionTo(new AirborneState(playerStateMachine));
        }

        public static float EstimateHorizontalDistance(float xVelocity, float yVelocity, float releaseOffsetY, float gravity) {
            float changeInY = -releaseOffsetY; // approx how far above the ground the player is
            float airTime = (yVelocity + (float)Math.Sqrt(yVelocity * yVelocity + 2f * gravity * changeInY)) / gravity;
            return xVelocity * airTime;
        }

        public void Exit(PlayerContext playerContext) { }
    }
}