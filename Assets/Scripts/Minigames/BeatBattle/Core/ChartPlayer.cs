using System;

namespace Minigames.BeatBattle.Core {
    public struct HitResult {
        public bool IsHit { get; }
        public int NoteIndex { get; }
        public float TimingOffsetInMs { get; }

        public HitResult(bool isHit, int noteIndex, float timingOffsetInMs) {
            IsHit = isHit;
            NoteIndex = noteIndex;
            TimingOffsetInMs = timingOffsetInMs;
        }
        
        public static HitResult Miss => new HitResult(false, -1, 0);
    }
    public class ChartPlayer {
        private readonly BeatBattleChart chart;
        private readonly BeatBattleConfig config;
        private readonly bool[] hitNotes;

        public event Action<int, float> NoteHit;
        
        public ChartPlayer(BeatBattleChart chart, BeatBattleConfig config) {
            this.chart = chart;
            this.config = config;
            this.hitNotes = new bool[chart.Notes.Count];
        }

        public HitResult ProcessInput(float timeInSeconds, NoteType noteType) {
            float hitWindowInSeconds = config.HitWindowInMs / 1000f;

            for (int i = 0; i < chart.Notes.Count; i++) {
                if (hitNotes[i]) continue;
                
                var note = chart.Notes[i];
                if (note.Type != noteType) continue;

                float noteTime = note.GridSlot * config.GridSlotDuration;
                float offset = timeInSeconds - noteTime;

                if (Math.Abs(offset) <= hitWindowInSeconds) {
                    hitNotes[i] = true;
                    NoteHit?.Invoke(i, offset * 1000f);
                    return new HitResult(true, i, offset * 1000f);
                }
            }

            return HitResult.Miss;
        }

        public int GetMissedNoteCount() {
            int missedCount = 0;
            for (int i = 0; i < hitNotes.Length; i++) {
                if (!hitNotes[i]) {
                    missedCount++;
                }
            }
            return missedCount;
        }
    }
}