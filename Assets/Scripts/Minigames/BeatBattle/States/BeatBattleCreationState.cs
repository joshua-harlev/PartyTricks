using Minigames.BeatBattle.Core;

namespace Minigames.BeatBattle.States {
    public class BeatBattleCreationState : IBeatBattleGameState {
        private readonly BeatBattleLaneView creatorLane;
        private readonly BeatBattleMinigameManager manager;
        private readonly ChartCreator creator;
        private readonly int creatorIndex;
        private int phaseStartTimeInMs;
        
        public BeatBattleCreationState(BeatBattleMinigameManager manager) {
            this.manager = manager;
            this.creatorIndex = manager.RoundState.CurrentCreatorIndex;
            this.creator = new ChartCreator(manager.ConfigSO.GetConfig());
            this.creatorLane = manager.GetLaneView(creatorIndex);
        }
        
        public void Enter() {
            phaseStartTimeInMs = manager.GetTimelinePositionInMs();
            manager.InvokeCreationPhaseStarted(creatorIndex);
            creator.NoteCreated += OnNoteCreated;

            manager.HUD.SetRound(manager.RoundState.CurrentRoundIndex);
            manager.HUD.SetStatus(creatorIndex, "Creating");
            for (int i = 0; i < 4; i++) {
                if (i == creatorIndex) continue;
                manager.HUD.SetStatus(i, "Get Ready...");
            }
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
            creatorLane.ClearNotes();
            manager.HUD.ClearAllStatusLabels();
        }

        private void OnNoteCreated(int gridSlot, NoteType type) {
            manager.InvokeNoteCreated(creatorIndex, gridSlot, type);
            float gridSlotDuration = manager.ConfigSO.GetConfig().GridSlotDuration;
            float creationDuration = manager.ConfigSO.CreationDurationInSeconds;
            creatorLane.SpawnCreationNote(type, gridSlot, gridSlotDuration, creationDuration);
        }
    }
}