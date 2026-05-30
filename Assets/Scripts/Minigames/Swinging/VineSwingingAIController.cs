using Minigames.Swinging.Core.PlayerStateMachine;
using UnityEngine;

namespace Minigames.Swinging {
    public class VineSwingingAIController {
        private double missChance = 0.2;
        private double fallChance = 0.3;
        private float maxPhaseOffset = 0.6f;
        private float minReleaseThreshold = 0.3f;
        private float releaseThresholdRange = 0.3f;
        private float minPhaseOffset = 0.2f;

        private readonly System.Random[] randomGenerators;
        private readonly bool[] skipWindow;
        private readonly bool[] wasInWindow;
        private readonly float[] phaseOffsetFromOptimal;

        public VineSwingingAIController(VineSwingingAIConfigSO config) {
            randomGenerators = new System.Random[4];
            skipWindow = new bool[4];
            wasInWindow = new bool[4];
            phaseOffsetFromOptimal = new float[4];
            for (int i = 0; i < 4; i++) {
                randomGenerators[i] = new System.Random();
            }

            if(config) InitializeFromConfig(config);
        }

        private void InitializeFromConfig(VineSwingingAIConfigSO config) {
            missChance = config.MissChance;
            fallChance = config.FallChance;
            maxPhaseOffset = config.MaxPhaseOffset;
            minReleaseThreshold = config.MinReleaseThreshold;
            releaseThresholdRange = config.ReleaseThresholdRange;
            minPhaseOffset = config.MinPhaseOffset;
        }

        public bool ShouldRelease(int playerIndex, PlayerStateMachine stateMachine) {
            switch(stateMachine.PlayerContext.CurrentStateType) {
                case PlayerStateType.Swinging:
                    bool inWindow = EvaluateWindow(stateMachine.PlayerContext);

                    if (inWindow && !wasInWindow[playerIndex]) {
                        RollMissDecision(playerIndex);
                    }

                    bool shouldRelease;
                    if (phaseOffsetFromOptimal[playerIndex] != 0f) {
                        shouldRelease = EvaluateWindow(stateMachine.PlayerContext, phaseOffsetFromOptimal[playerIndex]);
                    }
                    else {
                        shouldRelease = inWindow && !skipWindow[playerIndex];
                    }

                    wasInWindow[playerIndex] = inWindow;
                    return shouldRelease;
                default:
                    ResetState(playerIndex);
                    return false;
            }
        }

        public void OnVineGrabbed(int playerIndex, PlayerStateMachine stateMachine) {
            stateMachine.PlayerContext.AIReleaseThreshold =
                minReleaseThreshold + (float)randomGenerators[playerIndex].NextDouble() * releaseThresholdRange;
            ResetState(playerIndex);
        }

        private void ResetState(int playerIndex) {
            skipWindow[playerIndex] = false;
            wasInWindow[playerIndex] = false;
            phaseOffsetFromOptimal[playerIndex] = 0f;
        }

        private void RollMissDecision(int playerIndex) {
            skipWindow[playerIndex] = randomGenerators[playerIndex].NextDouble() < missChance;
            if (skipWindow[playerIndex]) {
                bool shouldFall = randomGenerators[playerIndex].NextDouble() < fallChance;
                if (shouldFall) {
                    float sign = randomGenerators[playerIndex].NextDouble() < 0.5 ? -1f : 1f;
                    phaseOffsetFromOptimal[playerIndex] = sign *
                                                          (minPhaseOffset + (float)randomGenerators[playerIndex].NextDouble() *
                                                              (maxPhaseOffset - minPhaseOffset));
                }
            }
        }

        private bool EvaluateWindow(PlayerContext context, float offset = 0f) {
            if (context.CurrentStateType != PlayerStateType.Swinging) return false;
            float phase = context.SwingPhase + offset;
            float threshold = context.AIReleaseThreshold;
            return Mathf.Sin(phase) > threshold && Mathf.Cos(phase) > threshold;
        }
    }
}