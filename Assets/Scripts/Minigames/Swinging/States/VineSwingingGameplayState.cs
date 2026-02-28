using System;
using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging.States {
    public class VineSwingingGameplayState : IVineSwingingGameState {
        private readonly VineSwingingMinigameManager minigameManager;
        private System.Random[] aiRandomGenerators;
        private int[] previousAttractionTargets;
        private int[] previousSwingVines;

        public VineSwingingGameplayState(VineSwingingMinigameManager minigameManager) {
            this.minigameManager = minigameManager;
        }
        
        public void Enter() {
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Entered Gameplay State.");
            InitializeAiRandomGenerators();
            previousAttractionTargets = new int[] { -1, -1, -1, -1 };
            previousSwingVines = new int[] { -1, -1, -1, -1 };
            minigameManager.IsInGameplay = true;
            minigameManager.GameTimer.OnTimerEnd += OnTimerEnd;
            minigameManager.GameTimer.StartTimer();
            minigameManager.StartMusic();
        }

        private void InitializeAiRandomGenerators() {
            aiRandomGenerators = new System.Random[4];
            for (int i = 0; i < 4; i++) {
                aiRandomGenerators[i] = new System.Random();
            }
        }

        public void OnUpdate() {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < minigameManager.PlayerStateMachines.Length; i++) {
                PlayerSlot slot = minigameManager.PlayerService.PlayerSlots[i];
                bool releasePressed;

                if (slot.IsAI) {
                    releasePressed = AIAutoRelease(minigameManager.PlayerStateMachines[i]);
                }
                else {
                    releasePressed = slot.InputHandler.SelectIsPressed();
                }
                
                minigameManager.PlayerStateMachines[i].Update(deltaTime, releasePressed);
                
                if (slot.IsAI) {
                    foreach (var pendingEvent in minigameManager.PlayerStateMachines[i].PlayerContext.PendingEvents) {
                        if (pendingEvent == PlayerEvent.GrabbedVine) {
                            minigameManager.PlayerStateMachines[i].PlayerContext.AIReleaseThreshold =
                                0.3f + (float)aiRandomGenerators[i].NextDouble() * 0.3f;
                        }
                    }
                }
                
                minigameManager.PlayerViews[i].Pull(minigameManager.PlayerStateMachines[i].PlayerContext);
                var playerContext = minigameManager.PlayerStateMachines[i].PlayerContext;
                int previousTarget = previousAttractionTargets[i];

                if (previousTarget >= 0 && previousTarget != playerContext.AttractionTargetVineIndex) {
                    minigameManager.TrackViews[i].GetVineView(previousTarget).SetPhaseAdjustment(0f);
                }

                previousAttractionTargets[i] = playerContext.AttractionTargetVineIndex;
                if (playerContext.AttractionTargetVineIndex >= 0) {
                    var targetVineView = minigameManager.TrackViews[i].GetVineView(playerContext.AttractionTargetVineIndex);
                    targetVineView.SetPhaseAdjustment(playerContext.VineAttractionPhaseAdjustment);
                }

                int currentSwingVine = playerContext.CurrentStateType == PlayerStateType.Swinging
                    ? playerContext.CurrentVineIndex
                    : -1;
                if (currentSwingVine >= 0) {
                    var vineView = minigameManager.TrackViews[i].GetVineView(currentSwingVine);
                    var stateMachine = minigameManager.PlayerStateMachines[i];
                    float vinePeriod = stateMachine.VinePeriods[currentSwingVine];
                    float naturalPhase = stateMachine.VinePhaseOffsets[currentSwingVine] +
                                         (2f * MathF.PI / vinePeriod) * stateMachine.ElapsedTime;
                    vineView.SetPhaseAdjustment(playerContext.SwingPhase - naturalPhase);
                    previousSwingVines[i] = currentSwingVine;
                }
                
                if (minigameManager.PlayerHasMagnet[i]) {
                    var collisions = Physics2D.OverlapCircleAll(minigameManager.PlayerViews[i].transform.position, minigameManager.PlayerMagnetRadii[i]);
                    foreach (var collision in collisions) {
                        var coin = collision.GetComponent<SwingingCoinView>();
                        coin?.StartPull(minigameManager.PlayerViews[i].transform, minigameManager.PlayerMagnetPullSpeed[i]);
                    }
                }
                
                var swingConfig = minigameManager.PlayerStateMachines[i].SwingConfig;
                int score = playerContext.FurthestVineIndex * swingConfig.VineScoreValue + playerContext.TotalCoinValue;
                minigameManager.PlayerCornerDisplays[i].UpdateScore(score);
            }
        }

        private bool AIAutoRelease(PlayerStateMachine stateMachine) {
            if (stateMachine.PlayerContext.CurrentStateType != PlayerStateType.Swinging) return false;
            float phase = stateMachine.PlayerContext.SwingPhase;
            float threshold = stateMachine.PlayerContext.AIReleaseThreshold;
            float sinPhase = Mathf.Sin(phase);
            float cosPhase = Mathf.Cos(phase);
            return sinPhase > threshold && cosPhase > threshold;
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