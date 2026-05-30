using System.Collections.Generic;

namespace Minigames.Swinging.Core.PlayerStateMachine {
    public class PlayerContext {
        public float PositionX;
        public float PositionY;
        public float VelocityX;
        public float VelocityY;
        public float SwingPhase;
        public float SwingAngle;
        public int CurrentVineIndex;
        public int FurthestVineIndex;
        public float RespawnTimer;
        public int TotalCoinValue;
        public float AIReleaseThreshold = 0.3f;
        public PlayerStateType CurrentStateType;
        public List<PlayerEvent> PendingEvents = new();

        public void ClearEvents() => PendingEvents.Clear();
    }
}
