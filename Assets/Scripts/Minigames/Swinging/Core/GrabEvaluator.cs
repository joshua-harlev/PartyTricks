using System;

namespace Minigames.Swinging.Core {
    public static class GrabEvaluator {
        // used for lookahead
        private const float FrameDuration = 1f / 60f;
        public static int CheckGrab(float playerX, float playerY, float[] vineXPositions, float vineAnchorY,
            SwingConfig config, int minVineIndex, float[] vinePhaseOffsets, float[] vinePeriods, float elapsedTime,  float playerContextVelocityX, float playerContextVelocityY) {
            float lookAheadTime, futureX, futureY;
            for (int k = 0; k <= config.GrabLookaheadFrames; k++) {
                lookAheadTime = k * FrameDuration;
                futureX = playerX + playerContextVelocityX * lookAheadTime;
                futureY = playerY + playerContextVelocityY * lookAheadTime - 0.5f * config.Gravity * lookAheadTime * lookAheadTime;
                
                for (int i = minVineIndex; i < vineXPositions.Length; i++) {
                    float vinePhase = vinePhaseOffsets[i] + (float)(2*Math.PI / vinePeriods[i]) * (elapsedTime+lookAheadTime);

                    int sampleCount = 3; // samples along the rope/vine to check 
                    for (int s = 0; s < sampleCount; s++) {
                        float sampleFraction = (s + 1f) / sampleCount;
                        float sampleLength = config.RopeLength * sampleFraction;
                        var (offsetX, offsetY) =
                            SwingSimulation.GetSwingPosition(vinePhase, config.Amplitude, sampleLength);
                
                        float sampleX = vineXPositions[i] + offsetX;
                        float sampleY = vineAnchorY + offsetY;
                
                        float dx = futureX - sampleX;
                        float dy = futureY - sampleY;
                        if (dx * dx + dy * dy <= config.GrabRadius * config.GrabRadius) {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }
    }
}