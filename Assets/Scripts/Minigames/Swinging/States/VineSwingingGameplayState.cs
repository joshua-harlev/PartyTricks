using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging.States {
    public class VineSwingingGameplayState : IVineSwingingGameState {
        private readonly VineSwingingMinigameManager minigameManager;
        private VineSwingingAIController aiController;

        public VineSwingingGameplayState(VineSwingingMinigameManager minigameManager) {
            this.minigameManager = minigameManager;
            aiController = new VineSwingingAIController(minigameManager.AIConfig);
        }
        
        public void Enter() {
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Entered Gameplay State.");
            minigameManager.IsInGameplay = true;
            minigameManager.GameTimer.OnTimerEnd += OnTimerEnd;
            minigameManager.GameTimer.StartTimer();
            minigameManager.StartMusic();
        }

        public void OnUpdate() {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < minigameManager.PlayerStateMachines.Length; i++) {
                PlayerSlot slot = minigameManager.PlayerService.PlayerSlots[i];
                bool releasePressed;

                if (slot.IsAI) {
                    releasePressed = aiController.ShouldRelease(i, minigameManager.PlayerStateMachines[i]);
                }
                else {
                    releasePressed = slot.InputHandler.SelectIsPressed();
                }
                
                minigameManager.PlayerStateMachines[i].Update(deltaTime, releasePressed);
                
                if (slot.IsAI) {
                    foreach (var pendingEvent in minigameManager.PlayerStateMachines[i].PlayerContext.PendingEvents) {
                        if (pendingEvent == PlayerEvent.GrabbedVine) {
                            aiController.OnVineGrabbed(i, minigameManager.PlayerStateMachines[i]);
                        }
                    }
                }
                
                minigameManager.PlayerViews[i].Pull(minigameManager.PlayerStateMachines[i].PlayerContext);
                if (minigameManager.PlayerHasMagnet[i]) {
                    var collisions = Physics2D.OverlapCircleAll(minigameManager.PlayerViews[i].transform.position, minigameManager.PlayerMagnetRadii[i]);
                    foreach (var collision in collisions) {
                        var coin = collision.GetComponent<SwingingCoinView>();
                        coin?.StartPull(minigameManager.PlayerViews[i].transform, minigameManager.PlayerMagnetPullSpeed[i]);
                    }
                }

                var playerContext = minigameManager.PlayerStateMachines[i].PlayerContext;
                var swingConfig = minigameManager.PlayerStateMachines[i].SwingConfig;
                int score = playerContext.FurthestVineIndex * swingConfig.VineScoreValue + playerContext.TotalCoinValue;
                minigameManager.PlayerCornerDisplays[i].UpdateScore(score);
            }
        }

        private void OnTimerEnd() {
            minigameManager.GameTimer.OnTimerEnd -= OnTimerEnd;
            minigameManager.ChangeState(new VineSwingingResultsState(minigameManager));
        }

        public void Exit() {
            minigameManager.IsInGameplay = false;
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Exited Gameplay State.");
        }
    }
}