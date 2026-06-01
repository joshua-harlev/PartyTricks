using System;

namespace Minigames.Swinging.Core.PlayerStateMachine {
    public class SwingingState : IPlayerState {
        private readonly PlayerStateMachine playerStateMachine;
        private const float FrameDuration = 1f / 60f;

        public SwingingState(PlayerStateMachine playerStateMachine) {
            this.playerStateMachine = playerStateMachine;
        }
        public void Enter(PlayerContext playerContext, SwingConfig swingConfig) {
            playerContext.CurrentStateType = PlayerStateType.Swinging;
            playerContext.PendingEvents.Add(PlayerEvent.GrabbedVine);
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
            float phaseRate = (float)(2 * Math.PI / vinePeriod);
            
            var (fallbackVx, fallbackVy) = SwingSimulation.GetShapedReleaseVelocity(
                playerContext.SwingPhase, swingConfig.Amplitude, vinePeriod, swingConfig.LaunchForce,
                swingConfig.RopeLength, swingConfig.ReleaseCurveExponent);
            fallbackVy *= swingConfig.VerticalLaunchScale;
            
            float bestVx = fallbackVx;
            float bestVy = fallbackVy;
            float bestDistance = 0f;

            for (int i = 0; i <= swingConfig.ReleaseLookaheadFrames; i++) {
                float futurePhase = playerContext.SwingPhase + phaseRate * i * FrameDuration;
                var (vx, vy) = SwingSimulation.GetShapedReleaseVelocity(
                    futurePhase, swingConfig.Amplitude, vinePeriod,
                    swingConfig.LaunchForce, swingConfig.RopeLength,
                    swingConfig.ReleaseCurveExponent);
                vy *= swingConfig.VerticalLaunchScale;

                var (_, offsetY) =
                    SwingSimulation.GetSwingPosition(futurePhase, swingConfig.Amplitude, swingConfig.RopeLength);

                float distance = EstimateHorizontalDistance(vx, vy, offsetY, swingConfig.Gravity);
                if (distance > bestDistance) {
                    bestDistance = distance;
                    bestVx = vx;
                    bestVy = vy;
                }
            }

            if (bestVx >= 0) {
                playerContext.VelocityX = Math.Max(bestVx, swingConfig.MinimumReleaseVelocityX);
            } else {
                playerContext.VelocityX = bestVx;
            }
            
            playerContext.VelocityY = bestVy;
            playerContext.PendingEvents.Add(PlayerEvent.Launched);
            playerStateMachine.TransitionTo(playerStateMachine.AirborneState);
        }

        public static float EstimateHorizontalDistance(float xVelocity, float yVelocity, float releaseOffsetY, float gravity) {
            float changeInY = -releaseOffsetY; // approx how far above the ground the player is
            float discriminant = yVelocity * yVelocity + 2f * gravity * changeInY;
            if (discriminant < 0f) return 0f;
            float airTime = (yVelocity + (float)Math.Sqrt(discriminant)) / gravity;
            return xVelocity * airTime;
        }

        public void Exit(PlayerContext playerContext) { }
    }
}