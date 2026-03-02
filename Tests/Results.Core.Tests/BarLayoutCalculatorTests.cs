using Xunit;

namespace ResultsScreen.Core {
    public class BarLayoutCalculatorTests {
        [Fact]
        public void LeaderGetsMaxHeight() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 4, 3, 2, 1 });
            float[] heights = BarLayoutCalculator.ComputeBarHeights(entries, 500f, 50f);
            Assert.Equal(500f, heights[0]);
        }
        
        [Fact]
        public void ZeroFundsGetsMinHeight() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 4, 3, 2, 0 });
            float[] heights = BarLayoutCalculator.ComputeBarHeights(entries, 500f, 50f);
            Assert.Equal(50f, heights[3]);
        }
        
        [Fact]
        public void HeightsScaleLinearly() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 0, 4, 2, 0 });
            float[] heights = BarLayoutCalculator.ComputeBarHeights(entries, 500f, 100f);
            Assert.Equal(500f, heights[0]);
            Assert.Equal(300f, heights[1]);
        }

        [Fact]
        public void AllMaxHeightWhenAllZeroFunds() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 0, 0, 0, 0 });
            float[] heights = BarLayoutCalculator.ComputeBarHeights(entries, 500f, 50f);
            foreach (var height in heights) {
                Assert.Equal(500f, height);
            }
        }

        [Fact]
        public void AllMaxHeightWhenAllEqualFunds() {
            var entries = PlacesCalculator.CalculatePlaces(new[] { 3, 3, 3, 3 });
            float[] heights = BarLayoutCalculator.ComputeBarHeights(entries, 500f, 50f);
            foreach (var height in heights) {
                Assert.Equal(500f, height);
            }
        }
    }
}