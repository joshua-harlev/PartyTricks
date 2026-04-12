using Game;

namespace Minigames.BeatBattle.States {
    public class BeatBattleCountdownState : IBeatBattleGameState {
        private readonly BeatBattleMinigameManager manager;
        private readonly MinigameStartCountdown countdown;
        
        public BeatBattleCountdownState(BeatBattleMinigameManager beatBattleMinigameManager, MinigameStartCountdown countdown) {
            manager = beatBattleMinigameManager;
            this.countdown = countdown;
        }

        public void Enter() {
            countdown.Initialize(TimerLengths.GetCountdownTimerLengthInSeconds());
            countdown.OnTimerEnd += OnCountdownEnd;
            countdown.StartTimer();
        }

        public void OnUpdate() { }

        public void Exit() { }

        private void OnCountdownEnd() {
            countdown.OnTimerEnd -= OnCountdownEnd;
            manager.StartMusic();
            manager.ChangeState(new BeatBattleCreationState(manager));
        }
    }
}