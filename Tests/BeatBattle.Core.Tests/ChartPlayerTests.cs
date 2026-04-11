using System.Collections.Generic;
using Minigames.BeatBattle.Core;
using Xunit;

namespace BeatBattle.Core.Tests {
    public class ChartPlayerTests {
        private static BeatBattleConfig CreateDefaultConfig() {
            return new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
        }

        private static BeatBattleChart CreateOneNoteChart(int gridSlotIndex, NoteType type) {
            return new BeatBattleChart(new List<ChartNote> { new ChartNote(gridSlotIndex, type) });
        }

        [Fact]
        public void InputWithinHitWindowRegistersAsHit() {
            var config = CreateDefaultConfig();
            var chart = CreateOneNoteChart(gridSlotIndex: 2, NoteType.A);

            var player = new ChartPlayer(chart, config);
            //window is 0.45s-0.55s (±100ms from 0.5s)
            var result = player.ProcessInput(0.48f, NoteType.A);

            Assert.True(result.IsHit);
            Assert.Equal(0, result.NoteIndex);
        }
        
        [Fact]
        public void InputOutsideHitWindowRegistersAsMiss() {
            var config = CreateDefaultConfig();
            var chart = CreateOneNoteChart(gridSlotIndex: 2, NoteType.A);
            var player = new ChartPlayer(chart, config);
            
            var result = player.ProcessInput(0.3f, NoteType.A);
            
            Assert.False(result.IsHit);
        }

        [Fact]
        public void WrongNoteTypeDoesNotHit() {
            var config = CreateDefaultConfig();
            var chart = CreateOneNoteChart(gridSlotIndex: 2, NoteType.A);
            var player = new ChartPlayer(chart, config);

            var result = player.ProcessInput(0.5f, NoteType.B);
            
            Assert.False(result.IsHit);
        }

        [Fact]
        public void CannotHitSameNoteTwice() {
            var config = CreateDefaultConfig();
            var chart = CreateOneNoteChart(gridSlotIndex: 2, NoteType.A);
            var player = new ChartPlayer(chart, config);
            
            var firstNote_shouldHit = player.ProcessInput(0.5f, NoteType.A);
            var secondNote_shouldMiss = player.ProcessInput(0.5f, NoteType.A);
            
            Assert.True(firstNote_shouldHit.IsHit);
            Assert.False(secondNote_shouldMiss.IsHit);
        }

        [Fact]
        public void HitRecordsTimingOffset() {
            var config = CreateDefaultConfig();
            var chart = CreateOneNoteChart(gridSlotIndex: 2, NoteType.A);
            var player = new ChartPlayer(chart, config);
            
            var result = player.ProcessInput(0.48f, NoteType.A);
            
            Assert.True(result.IsHit);
            Assert.Equal(-20f, result.TimingOffsetInMs, 1f);
        }

        [Fact]
        public void GetMissedNoteCountReturnsMissedNotes() {
            var config = CreateDefaultConfig();
            var chart = new BeatBattleChart(new List<ChartNote>
            {
                new ChartNote(0, NoteType.A),
                new ChartNote(1, NoteType.B),
                new ChartNote(2, NoteType.A),
            });
            var player = new ChartPlayer(chart, config);
            
            player.ProcessInput(0.0f, NoteType.A);
            
            Assert.Equal(2, player.GetMissedNoteCount());
        }

        [Fact]
        public void GetMissedNoteCountReturnsZeroWhenAllHit() {
            var config = CreateDefaultConfig();
            var chart = new BeatBattleChart(new List<ChartNote>
            {
                new ChartNote(0, NoteType.A),
                new ChartNote(1, NoteType.B)
            });
            var player = new ChartPlayer(chart, config);
            
            player.ProcessInput(0.0f, NoteType.A);
            player.ProcessInput(0.25f, NoteType.B);
            
            Assert.Equal(0, player.GetMissedNoteCount());
        }

        [Fact]
        public void NoteHitEventFiresWithCorrectData() {
            var config = CreateDefaultConfig();
            var chart = CreateOneNoteChart(gridSlotIndex: 2, NoteType.A);
            var player = new ChartPlayer(chart, config);

            int firedNoteIndex = -1;
            float firedOffset = 0f;
            player.NoteHit += (noteIndex, offsetInMs) =>
            {
                firedNoteIndex = noteIndex;
                firedOffset = offsetInMs;
            };
            
            player.ProcessInput(0.52f, NoteType.A);
            
            Assert.Equal(0, firedNoteIndex);
            Assert.Equal(20f, firedOffset, 1f);
        }

        [Fact]
        public void NoteHitEventDoesNotFireOnMiss() {
            var config = CreateDefaultConfig();
            var chart = CreateOneNoteChart(gridSlotIndex: 2, NoteType.A);
            var player = new ChartPlayer(chart, config);

            bool fired = false;
            player.NoteHit += (_, _) => fired = true;
            player.ProcessInput(0.3f, NoteType.A);
            Assert.False(fired);
        }
    }
}