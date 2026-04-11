using System;
using System.Collections.Generic;

namespace Minigames.BeatBattle.Core {
    public class ChartCreator {
        private readonly BeatBattleConfig config;
        private readonly List<ChartNote> notes = new();

        public event Action<int, NoteType> NoteCreated;

        public ChartCreator(BeatBattleConfig config) {
            this.config = config;
        }

        public bool TryToAddNote(float timeInSeconds, NoteType type) {
            if (notes.Count >= config.MaxNotesPerTurn) return false;

            int gridSlotIndex = (int)Math.Round(timeInSeconds / config.GridSlotDuration, MidpointRounding.AwayFromZero);
            
            if (!ValidateNoteGridSlot(gridSlotIndex)) return false;
            
            notes.Add(new ChartNote(gridSlotIndex, type));
            NoteCreated?.Invoke(gridSlotIndex, type);
            return true;
        }

        public BeatBattleChart FinalizeChart() {
            return new BeatBattleChart(new List<ChartNote>(notes));
        }
        
        private bool ValidateNoteGridSlot(int gridSlotIndex) {
            foreach (var note in notes) {
                if (note.GridSlot == gridSlotIndex) return false;
            }

            return true;
        }
    }
}