using Minigames.BeatBattle.Core;
using Xunit;

namespace BeatBattle.Core.Tests {
    public class ChartCreatorTests {
        [Fact]
        public void NoteSnapsToNearestGridSlot() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
            var creator = new ChartCreator(config);

            bool addedSuccessfully = creator.TryToAddNote(0.1f, NoteType.A);
            
            Assert.True(addedSuccessfully);
            var chart = creator.FinalizeChart();
            Assert.Single(chart.Notes);
            Assert.Equal(0, chart.Notes[0].GridSlot);
            Assert.Equal(NoteType.A, chart.Notes[0].Type);
        }

        [Fact]
        public void SlotsRejectExtraNotes() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
            var creator = new ChartCreator(config);
            
            bool firstNoteAdded_shouldSucceed = creator.TryToAddNote(0f, NoteType.A);
            bool secondNoteAdded_shouldFail = creator.TryToAddNote(0.1f, NoteType.B);
            
            Assert.True(firstNoteAdded_shouldSucceed);
            Assert.False(secondNoteAdded_shouldFail);
            
            var chart = creator.FinalizeChart();
            Assert.Single(chart.Notes);
        }

        [Fact]
        public void RespectsMaxNoteLimit() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 3,
                hitWindowInMs: 100f);
            var creator = new ChartCreator(config);
            
            Assert.True(creator.TryToAddNote(0f, NoteType.A));
            Assert.True(creator.TryToAddNote(0.25f, NoteType.B));
            Assert.True(creator.TryToAddNote(0.5f, NoteType.A));
            Assert.False(creator.TryToAddNote(0.75f, NoteType.B));
            
            var chart = creator.FinalizeChart();
            Assert.Equal(3, chart.Notes.Count);
        }

        [Fact]
        public void NoteAtExactMidpointSnapsToNextSlot() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
            var creator = new ChartCreator(config);
            
            bool added = creator.TryToAddNote(0.125f, NoteType.A);
            
            Assert.True(added);
            var chart = creator.FinalizeChart();
            Assert.Single(chart.Notes);
            Assert.Equal(1, chart.Notes[0].GridSlot);
        }

        [Fact]
        public void NoteCreatedEventFiresWithCorrectData() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
            var creator = new ChartCreator(config);

            int firedGridSlot = -1;
            NoteType firedType = default;
            creator.NoteCreated += (gridSlot, type) =>
            {
                firedGridSlot = gridSlot;
                firedType = type;
            };
            
            creator.TryToAddNote(0.3f, NoteType.B);
            
            Assert.Equal(1, firedGridSlot);
            Assert.Equal(NoteType.B, firedType);
        }
        
        [Fact]
        public void NoteCreatedEventDoesNotFireOnRejection() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
            var creator = new ChartCreator(config);

            bool fired = false;
            creator.TryToAddNote(0.0f, NoteType.A);
            
            creator.NoteCreated += (_, _) => fired = true;
            creator.TryToAddNote(0.1f, NoteType.B);
            
            Assert.False(fired);
        }
    }
}