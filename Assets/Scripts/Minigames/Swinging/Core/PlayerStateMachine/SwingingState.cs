using System;

namespace VineSwinging.Core {
    public class SwingingState : IPlayerState {
        private readonly PlayerStateMachine playerStateMachine;

        public SwingingState(PlayerStateMachine playerStateMachine) {
            this.playerStateMachine = playerStateMachine;
        }
        public void Enter(PlayerContext playerContext, SwingConfig swingConfig) {
            playerContext.CurrentStateType = PlayerStateType.Swinging;
            float vinePeriod = playerStateMachine.VinePeriods[playerContext.CurrentVineIndex];
            playerContext.SwingPhase = playerStateMachine.VinePhaseOffsets[playerContext.CurrentVineIndex]
                + (float)(2*Math.PI / vinePeriod) * playerStateMachine.ElapsedTime;
            playerContext.SwingPhase += playerContext.VineAttractionPhaseAdjustment;
            playerContext.VineAttractionPhaseAdjustment = 0f;
            playerContext.AttractionTargetVineIndex = -1;
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
            var releaseVelocity = SwingSimulation.GetReleaseVelocity(playerContext.SwingPhase, swingConfig.Amplitude, vinePeriod, swingConfig.LaunchForce, swingConfig.RopeLength);
            bool isMovingBackwards = releaseVelocity.vx < 0;
            if (!isMovingBackwards) {
                playerContext.VelocityX = Math.Max(releaseVelocity.vx, swingConfig.MinimumReleaseVelocityX);
            } else playerContext.VelocityX = releaseVelocity.vx;
            playerContext.VelocityY = releaseVelocity.vy;
            playerContext.PendingEvents.Add(PlayerEvent.Launched);
            playerStateMachine.TransitionTo(new AirborneState(playerStateMachine));
        }

        public void Exit(PlayerContext playerContext) { }
    }
}