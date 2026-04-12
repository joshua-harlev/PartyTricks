using System.ComponentModel;
using System.Linq;
using Minigames.BeatBattle.Core;
using Xunit;

namespace BeatBattle.Core.Tests {
    public class RoundStateTests {
        [Fact]
        public void TurnOrderIncludesAllPlayers() {
            var config = new BeatBattleConfig(
                    bpm: 120f,
                    gridSubdivision: 2,
                    maxNotesPerTurn: 5,
                    hitWindowInMs: 100f);
            var state = new RoundState(config, seed: 42);

            var turnOrder = state.TurnOrder;
            
            Assert.Equal(4, turnOrder.Length);
            Assert.Contains(0, turnOrder);
            Assert.Contains(1, turnOrder);
            Assert.Contains(2, turnOrder);
            Assert.Contains(3, turnOrder);
        }
        
        [Fact]
        public void CurrentCreatorFollowsTurnOrder() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
            var state = new RoundState(config, seed: 42);
            
            Assert.Equal(state.TurnOrder[0], state.CurrentCreatorIndex);

            state.AdvanceRound();
            Assert.Equal(state.TurnOrder[1], state.CurrentCreatorIndex);
            
            state.AdvanceRound();
            Assert.Equal(state.TurnOrder[2], state.CurrentCreatorIndex);
        }

        [Fact]
        public void AdvanceRoundReturnsFalseAfterAllRounds() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
            var state = new RoundState(config, seed: 42);
            
            Assert.True(state.AdvanceRound());
            Assert.True(state.AdvanceRound());
            Assert.True(state.AdvanceRound());
            Assert.False(state.AdvanceRound());
        }

        [Fact]
        public void DifferentSeedsProduceDifferentOrders() {
            var config = new BeatBattleConfig(
                bpm: 120f,
                gridSubdivision: 2,
                maxNotesPerTurn: 5,
                hitWindowInMs: 100f);
            var state = new RoundState(config, seed: 42);
            var state1 = new RoundState(config, seed: 1);
            var state2 = new RoundState(config, seed: 99);
            Assert.False(state1.TurnOrder.SequenceEqual(state2.TurnOrder));
        }
    }
}