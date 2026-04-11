namespace Minigames.BeatBattle.Core {
    public struct BeatBattleConfig {
        public float BPM { get; set; }
        public int GridSubdivision { get; }
        public int MaxNotesPerTurn { get; }
        public float HitWindowInMs { get; }

        public BeatBattleConfig(float bpm, int gridSubdivision, int maxNotesPerTurn, float hitWindowInMs) {
            BPM = bpm;
            GridSubdivision = gridSubdivision;
            MaxNotesPerTurn = maxNotesPerTurn;
            HitWindowInMs = hitWindowInMs;
        }
        
        // Seconds between adjacent grid slots
        public float GridSlotDuration => 60f / (BPM * GridSubdivision);
    }
}