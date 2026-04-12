namespace Minigames.BeatBattle.States {
    public interface IBeatBattleGameState {
        void Enter();
        void OnUpdate();
        void Exit();
    }
}