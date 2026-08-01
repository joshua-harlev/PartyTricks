using Debug;
using Input;
using Minigames.Swinging.Core;
using Minigames.Swinging.Core.PlayerStateMachine;
using UnityEngine;

namespace Minigames.Swinging.States {
    public class VineSwingingGameplayState : IVineSwingingGameState {
        private readonly VineSwingingMinigameManager minigameManager;
        private const int MaxMagnetHits = 16;
        private readonly Collider2D[] magnetHits = new Collider2D[MaxMagnetHits];
        private readonly ContactFilter2D magnetFilter = new ContactFilter2D
            { useTriggers = true, useLayerMask = false };
        
        private VineSwingingAIController aiController;

        private int[] previousScores;
        private int[] previousFurthestVineIndices;

        public VineSwingingGameplayState(VineSwingingMinigameManager minigameManager) {
            this.minigameManager = minigameManager;
        }
        
        public void Enter() {
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Entered Gameplay State.");

            previousScores = new int[minigameManager.PlayerCount];
            previousFurthestVineIndices = new int[minigameManager.PlayerCount];
            aiController = new VineSwingingAIController(minigameManager.AIConfig, minigameManager.PlayerCount);
            
            for (int i = 0; i < minigameManager.PlayerStateMachines.Length; i++) {
                previousFurthestVineIndices[i] = minigameManager.PlayerStateMachines[i].PlayerContext.FurthestVineIndex;
            }
            
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

                var playerContext = minigameManager.PlayerStateMachines[i].PlayerContext;
                bool madeProgress = playerContext.FurthestVineIndex > previousFurthestVineIndices[i];
                previousFurthestVineIndices[i] = playerContext.FurthestVineIndex;
                bool fell = playerContext.PendingEvents.Contains(PlayerEvent.Fell);
                playerContext.UpdateSweetSpotHint(madeProgress, fell, deltaTime, minigameManager.HintSuccessDecrement, minigameManager.HintFailureIncrement, minigameManager.HintFadeSpeed);
                minigameManager.PlayerViews[i].Pull(minigameManager.PlayerStateMachines[i].PlayerContext);
                if (minigameManager.PlayerHasMagnet[i]) {
                    Vector2 center = minigameManager.PlayerViews[i].transform.position;
                    int hitCount = Physics2D.OverlapCircle(center, minigameManager.PlayerMagnetRadii[i], magnetFilter, magnetHits);
                    for (var hitIndex = 0; hitIndex < hitCount; hitIndex++) {
                        var collision = magnetHits[hitIndex];
                        var coin = collision.GetComponent<SwingingCoinView>();
                        coin?.StartPull(minigameManager.PlayerViews[i].transform,
                            minigameManager.PlayerMagnetPullSpeed[i]);
                    }
                }
                
                int score = ResultsCalculator.CalculateScore(playerContext,
                    minigameManager.PlayerStateMachines[i].SwingConfig);
                if (score != previousScores[i]) {
                    previousScores[i] = score;
                    minigameManager.PlayerCornerDisplays[i].UpdateScore(score);
                }
            }
        }

        private void OnTimerEnd() {
            minigameManager.GameTimer.OnTimerEnd -= OnTimerEnd;
            minigameManager.ChangeState(minigameManager.VineSwingingResultsState);
        }

        public void Exit() {
            minigameManager.IsInGameplay = false;
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Exited Gameplay State.");
        }
    }
}