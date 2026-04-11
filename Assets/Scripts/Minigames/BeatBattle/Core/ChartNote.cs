namespace Minigames.BeatBattle.Core {
    public readonly struct ChartNote {
        public int GridSlot { get; }
        public NoteType Type { get; }

        public ChartNote(int gridSlot, NoteType type) {
            GridSlot = gridSlot;
            Type = type;
        }
    }
}