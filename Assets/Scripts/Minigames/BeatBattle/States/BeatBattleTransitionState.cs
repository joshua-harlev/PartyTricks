namespace Minigames.BeatBattle.States {
    public class BeatBattleTransitionState : IBeatBattleGameState {
        private readonly BeatBattleMinigameManager manager;
        private int phaseStartTimeInMs;
        
        public BeatBattleTransitionState(BeatBattleMinigameManager manager) {
            this.manager = manager;
        }

        public void Enter() {
            phaseStartTimeInMs = manager.GetTimelinePositionInMs();
        }

        public void OnUpdate() {
            float elapsedTimeInSeconds = (manager.GetTimelinePositionInMs() - phaseStartTimeInMs) / 1000f;

            if (elapsedTimeInSeconds >= manager.ConfigSO.TransitionDurationInSeconds) {
                if (manager.RoundState.AdvanceRound()) {
                    manager.InvokeTransitionStart(manager.RoundState.CurrentCreatorIndex);
                    manager.ChangeState(new BeatBattleCreationState(manager));
                }
                else {
                    manager.ChangeState(new BeatBattleResultsState(manager));
                }
            }
        }

        public void Exit() { }
    }
}