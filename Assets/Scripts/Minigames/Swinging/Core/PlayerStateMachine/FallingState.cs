namespace Minigames.Swinging.Core.PlayerStateMachine {
    public class FallingState : IPlayerState {
        private readonly PlayerStateMachine playerStateMachine;

        public FallingState(PlayerStateMachine playerStateMachine) {
            this.playerStateMachine = playerStateMachine;
        }
        public void Enter(PlayerContext playerContext, SwingConfig swingConfig) {
            playerContext.CurrentStateType = PlayerStateType.Falling;
            playerContext.RespawnTimer = swingConfig.RespawnDelay;
            playerContext.PendingEvents.Add(PlayerEvent.Fell);
            playerContext.SwingAngle = 0f;
        }

        public void Update(PlayerContext playerContext, SwingConfig swingConfig, float deltaTime, bool releasePressed) {
            playerContext.RespawnTimer -= deltaTime;
            if (playerContext.RespawnTimer <= 0f) {
                playerContext.CurrentVineIndex = playerContext.FurthestVineIndex;
                playerContext.PositionX = playerStateMachine.VineXPositions[playerContext.FurthestVineIndex];
                playerContext.PositionY = playerStateMachine.VineAnchorY;
                playerStateMachine.TransitionTo(playerStateMachine.SwingingState);
            }
        }

        public void Exit(PlayerContext playerContext) { }
    }
}