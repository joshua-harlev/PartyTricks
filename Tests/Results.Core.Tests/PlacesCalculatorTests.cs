using Xunit;

namespace ResultsScreen.Core {
    public class PlacesCalculatorTests {
        [Fact]
        public void DistinctFundsAreSortedDescending() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 1, 4, 2, 3 });
            Assert.Equal(1, entries[0].PlayerIndex);
            Assert.Equal(3, entries[1].PlayerIndex);
            Assert.Equal(2, entries[2].PlayerIndex);
            Assert.Equal(0, entries[3].PlayerIndex);
        }

        [Fact]
        public void RanksAreAssignedByPosition() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 1, 4, 2, 3 });
            Assert.Equal(0, entries[0].Rank);
            Assert.Equal(1, entries[1].Rank);
            Assert.Equal(2, entries[2].Rank);
            Assert.Equal(3, entries[3].Rank);
        }

        [Fact]
        public void LowerIndexFirstAndSameWhenPlayersTied() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 3, 3, 1, 1 });
            Assert.Equal(0, entries[0].PlayerIndex);
            Assert.Equal(1, entries[1].PlayerIndex);
            Assert.Equal(0, entries[0].Rank);
            Assert.Equal(0, entries[1].Rank);
            Assert.Equal(2, entries[2].Rank);
            Assert.Equal(2, entries[3].Rank);
        }

        [Fact]
        public void EveryoneIsRankZeroWhenAllTied() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 1, 1, 1, 1 });
            for (int i = 0; i < entries.Length; i++) {
                Assert.Equal(0, entries[i].Rank);
            }
        }

        [Fact]
        public void FundsValuesArePreserved() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 50, 200, 150, 75 });
            Assert.Equal(200, entries[0].Funds);
            Assert.Equal(150, entries[1].Funds);
            Assert.Equal(75, entries[2].Funds);
            Assert.Equal(50, entries[3].Funds);
        }
    }
}