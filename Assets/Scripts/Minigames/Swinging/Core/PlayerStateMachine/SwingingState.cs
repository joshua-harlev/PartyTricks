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
            
            float quality = (float)Math.Abs(Math.Cos(playerContext.SwingPhase));
            bool shouldRelease = quality >= 0.5f;
            if (releasePressed && !playerContext.ReleaseBuffered) {
                if (shouldRelease) {
                    Release(playerContext, swingConfig, vinePeriod);
                } else {
                    playerContext.ReleaseBuffered = true;
                    playerContext.ReleaseBufferTimer = swingConfig.ReleaseBufferDuration;
                }
            } else if (playerContext.ReleaseBuffered) {
                if (!shouldRelease) {
                    playerContext.ReleaseBufferTimer -= deltaTime;
                }
                if (playerContext.ReleaseBufferTimer <= 0 || shouldRelease) {
                    Release(playerContext, swingConfig, vinePeriod);
                }
            }
        }

        private void Release(PlayerContext playerContext, SwingConfig swingConfig, float vinePeriod) {
            var releaseVelocity = SwingSimulation.GetReleaseVelocity(playerContext.SwingPhase, swingConfig.Amplitude, vinePeriod, swingConfig.LaunchForce, swingConfig.RopeLength);
            playerContext.VelocityX = Math.Max(releaseVelocity.vx, swingConfig.MinimumReleaseVelocityX);
            playerContext.VelocityY = releaseVelocity.vy;
            playerContext.ReleaseBuffered = false;
            playerContext.PendingEvents.Add(PlayerEvent.Launched);
            playerStateMachine.TransitionTo(new AirborneState(playerStateMachine));
        }

        public void Exit(PlayerContext playerContext) { }
    }
}