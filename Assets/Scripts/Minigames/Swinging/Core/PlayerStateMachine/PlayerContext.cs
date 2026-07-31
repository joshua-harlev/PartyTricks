using System;
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

        public float TargetHintLevel = 1f;
        public float DisplayedHintLevel = 1f;

        public void UpdateSweetSpotHint(bool madeProgress, bool fell, float deltaTime, float successDecrement, float failureIncrement, float fadeSpeed) {
            if (madeProgress) TargetHintLevel -= successDecrement;
            if (fell) TargetHintLevel += failureIncrement;
            TargetHintLevel = Math.Clamp(TargetHintLevel, 0f, 1f);
            
            float maxStep = fadeSpeed * deltaTime;
            float difference = TargetHintLevel - DisplayedHintLevel;
            if (Math.Abs(difference) <= maxStep) {
                DisplayedHintLevel = TargetHintLevel;
            }
            else {
                DisplayedHintLevel += Math.Sign(difference) * maxStep;
            }
        }
    }
}
