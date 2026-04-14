using Game;

namespace Minigames.BeatBattle.States {
    public class BeatBattleResultsState : IBeatBattleGameState {
        private readonly BeatBattleMinigameManager manager;
        public BeatBattleResultsState(BeatBattleMinigameManager manager) {
            this.manager = manager;
        }

        public void Enter() {
            var rankings = manager.Scorer.GetRankings();
            int multiplier = 1;
            if (manager.IsDoubleRound) multiplier = 2;

            var results = new PlayerMinigameResult[4];
            for (int i = 0; i < 4; i++) {
                int baseFunds = MinigamePayouts.GetBaseFundsPerRank()[rankings[i]] * multiplier;
                results[i] = new PlayerMinigameResult(i, rankings[i], baseFunds);
                manager.HUD.SetStatus(i, $"{results[i].PlayerPlace} Place");
            }
            manager.InvokeMinigameFinished(results);
        }

        public void OnUpdate() { }

        public void Exit() { }
    }
}