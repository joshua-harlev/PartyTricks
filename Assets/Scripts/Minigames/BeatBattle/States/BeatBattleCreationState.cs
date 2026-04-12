using Minigames.BeatBattle.Core;

namespace Minigames.BeatBattle.States {
    public class BeatBattleCreationState : IBeatBattleGameState {
        private readonly BeatBattleMinigameManager manager;
        private readonly ChartCreator creator;
        private readonly int creatorIndex;
        private int phaseStartTimeInMs;
        
        public BeatBattleCreationState(BeatBattleMinigameManager manager) {
            this.manager = manager;
            this.creatorIndex = manager.RoundState.CurrentCreatorIndex;
            this.creator = new ChartCreator(manager.ConfigSO.GetConfig());
        }
        
        public void Enter() {
            phaseStartTimeInMs = manager.GetTimelinePositionInMs();
            manager.InvokeCreationPhaseStarted(creatorIndex);

            creator.NoteCreated += OnNoteCreated;
        }

        public void OnUpdate() {
            float elapsedTimeInSeconds = (manager.GetTimelinePositionInMs() - phaseStartTimeInMs) / 1000f;

            if (elapsedTimeInSeconds >= manager.ConfigSO.CreationDurationInSeconds) {
                manager.ChangeState(new BeatBattlePlaybackState(manager, creator.FinalizeChart()));
                return;
            }
            
            var input = manager.GetInputHandler(creatorIndex);
            if (input.SelectIsPressed()) {
                creator.TryToAddNote(elapsedTimeInSeconds, NoteType.A);
            } else if (input.CancelIsPressed()) {
                creator.TryToAddNote(elapsedTimeInSeconds, NoteType.B);
            }
        }

        public void Exit() {
            creator.NoteCreated -= OnNoteCreated;
        }

        private void OnNoteCreated(int gridSlot, NoteType type) {
            manager.InvokeNoteCreated(creatorIndex, gridSlot, type);
        }
    }
}