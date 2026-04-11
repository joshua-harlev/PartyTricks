using Minigames.BeatBattle.Core;
using Xunit;

namespace BeatBattle.Core.Tests {
    public class RoundScorerTests {
        [Fact]
        public void HitAwardsPointToPlayer() {
            var scorer = new RoundScorer();
            scorer.RegisterHit(playerIndex: 1);
            
            var scores = scorer.GetTotalScores();
            Assert.Equal(1, scores[1]);
            Assert.Equal(0, scores[0]);
        }

        [Fact]
        public void MissAwardsPointToCreator() {
            var scorer = new RoundScorer();
            scorer.RegisterCreatorMissBonus(creatorIndex: 2);
            
            var scores = scorer.GetTotalScores();
            Assert.Equal(1, scores[2]);
        }

        [Fact]
        public void ScoresAccumulateAcrossCalls() {
            var scorer = new RoundScorer();
            
            scorer.RegisterHit(0);
            scorer.RegisterHit(0);
            scorer.RegisterHit(0);
            scorer.RegisterCreatorMissBonus(1);
            scorer.RegisterCreatorMissBonus(1);
            
            var scores = scorer.GetTotalScores();
            Assert.Equal(3, scores[0]);
            Assert.Equal(2, scores[1]);
            Assert.Equal(0, scores[2]);
            Assert.Equal(0, scores[3]);
        }
        
        [Fact]
        public void RankingsOrderByScoreDescending() {
            var scorer = new RoundScorer();
            scorer.RegisterHit(0);
            scorer.RegisterHit(2);
            scorer.RegisterHit(2);
            scorer.RegisterHit(3);
            scorer.RegisterHit(3);
            scorer.RegisterHit(3);

            var rankings = scorer.GetRankings();
            Assert.Equal(4, rankings.Length);
            Assert.Equal(2, rankings[0]);
            Assert.Equal(3, rankings[1]);
            Assert.Equal(1, rankings[2]);
            Assert.Equal(0, rankings[3]);
        }

        [Fact]
        public void TiedPlayersHaveTheSameRank() {
            var scorer = new RoundScorer();
            scorer.RegisterHit(0);
            scorer.RegisterHit(1);
            scorer.RegisterHit(2);
            scorer.RegisterHit(2);
            
            var rankings = scorer.GetRankings();
            Assert.Equal(4, rankings.Length);
            Assert.Equal(0, rankings[2]);
            Assert.Equal(1, rankings[0]);
            Assert.Equal(1, rankings[1]);
            Assert.Equal(3, rankings[3]);
        }
    }
}