using System;

namespace VineSwinging.Core {
    public static class VineAttraction {
        public static float DistanceToNearestMultipleOfPi(float phase) {
            double intervalOfPI = phase % Math.PI;
            if (intervalOfPI < 0) intervalOfPI += Math.PI;
            double distanceToLeftEnd = intervalOfPI;
            double distanceToRightEnd = Math.PI - intervalOfPI;
            return (float)Math.Min(distanceToLeftEnd, distanceToRightEnd);
        }

        public static float UpdatePhaseAdjustment(float currentAdjustment, float vinePhaseOffset, float vinePeriod, float elapsedTime, SwingConfig config, float deltaTime) {
            double naturalPhase = vinePhaseOffset + (2 * Math.PI / vinePeriod) * elapsedTime;
            double effectivePhase = naturalPhase + currentAdjustment;
            float distanceToIdeal = DistanceToNearestMultipleOfPi((float)effectivePhase);
            float newAdjustment;
            if (distanceToIdeal > config.SlowdownThreshold) {
                newAdjustment = config.BaseAttractionRate * (distanceToIdeal / (MathF.PI / 2));
            }
            else {
                float normalizedDistance = distanceToIdeal / config.SlowdownThreshold;
                newAdjustment = -config.SlowdownRate * (1-normalizedDistance);
            }

            return Math.Clamp(currentAdjustment + newAdjustment * deltaTime, -config.MaxPhaseAdjustment,
                config.MaxPhaseAdjustment);
        }
    }
}