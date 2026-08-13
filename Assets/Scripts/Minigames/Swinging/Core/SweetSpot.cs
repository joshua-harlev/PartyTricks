using System;

namespace Minigames.Swinging.Core {
    public class SweetSpot {
        public const float IdealReleasePhase = 0.55f;

        private const float MinSin = 0.25f;
        private const float MinCos = 0.3f;
        private const float MaxSin = 0.67f;
        
        public static bool IsInGlowWindow(float phase) {
            float sin = (float)Math.Sin(phase);
            float cos = (float)Math.Cos(phase);
            return sin > MinSin && cos > MinCos && sin < MaxSin;
        }

        public static bool IsInReleaseWindow(float phase, float threshold) {
            bool sinIsInWindow = (float)Math.Sin(phase) > threshold;
            bool cosIsInWindow = (float)Math.Cos(phase) > threshold;
            return sinIsInWindow && cosIsInWindow;
        }
    }
}