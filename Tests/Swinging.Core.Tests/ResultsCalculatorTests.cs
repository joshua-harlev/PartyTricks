using Minigames.Swinging.Core;
using Minigames.Swinging.Core.PlayerStateMachine;
using Xunit;

namespace Swinging.Core.Tests {
    public class ResultsCalculatorTests {
        [Fact]
        public void DistinctScoresAreRankedHighToLow() {
            int[] ranks = ResultsCalculator.CalculateRanks(new[] { 50, 100, 75 });
            Assert.Equal(new[] {2, 0, 1}, ranks);
        }

        [Fact]
        public void OnlyOnePlayerIsGivenFirstPlace() {
            int[] ranks = ResultsCalculator.CalculateRanks(new[] { 42 });
            Assert.Equal(new[] { 0 }, ranks);
        }

        [Theory]
        [InlineData(new[] {42, 42, 10, 10}, new[] {0, 0, 2, 2})]
        [InlineData(new[] {42, 42, 42, 42}, new[] {0, 0, 0, 0})]
        [InlineData(new[] {1, 0, 0, 0}, new[] {0, 1, 1, 1})]
        public void TiedPlayersAreGivenTheSameRanks(int[] scores, int[] expected) {
            int[] ranks = ResultsCalculator.CalculateRanks(scores);
            Assert.Equal(expected, ranks); 
        }

        [Fact]
        public void ScoreCombinesVineProgressAndCoins() {
            var context = new PlayerContext { FurthestVineIndex = 3, TotalCoinValue = 7 };
            var config = TestHelpers.DefaultConfig(vineScoreValue: 10);
            Assert.Equal(37, ResultsCalculator.CalculateScore(context, config));
        }

        [Fact]
        public void NewPlayerHasZeroScore() {
            Assert.Equal(0, ResultsCalculator.CalculateScore(new PlayerContext(), TestHelpers.DefaultConfig()));
        }

        [Fact]
        public void CoinsCountWithoutVineProgress() {
            var context = new PlayerContext { FurthestVineIndex = 0, TotalCoinValue = 12 };
            Assert.Equal(12, ResultsCalculator.CalculateScore(context, TestHelpers.DefaultConfig()));
        }
    }
}