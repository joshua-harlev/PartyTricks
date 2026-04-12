using System;

namespace Minigames.BeatBattle.Core {
    public class RoundState {
        private readonly BeatBattleConfig config;
        
        public int[] TurnOrder { get; }
        public int CurrentRoundIndex { get; private set; }
        public int CurrentCreatorIndex => TurnOrder[CurrentRoundIndex];
        
        public RoundState(BeatBattleConfig config, int seed) {
            this.config = config;
            TurnOrder = new[] { 0, 1, 2, 3 };
            ShuffleTurnOrder(new Random(seed));
            CurrentRoundIndex = 0;
        }
        
        public bool AdvanceRound() {
            if (CurrentRoundIndex >= 3) return false;
            CurrentRoundIndex++;
            return true;
        }

        private void ShuffleTurnOrder(Random random) {
            for (int i = TurnOrder.Length - 1; i > 0; i--) {
                int j = random.Next(i + 1);
                (TurnOrder[i], TurnOrder[j]) = (TurnOrder[j], TurnOrder[i]);
            }
        }
    }
}