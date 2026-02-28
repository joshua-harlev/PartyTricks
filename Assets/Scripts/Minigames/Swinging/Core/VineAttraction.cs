using System;

namespace VineSwinging.Core {
    public static class VineAttraction {
        public static float UpdatePhaseAdjustment(float currentAdjustment, float vinePhaseOffset, float vinePeriod,
            float elapsedTime, SwingConfig config, float deltaTime,
            float playerX, float playerY, float vineAnchorX, float vineAnchorY) {
            
            double naturalPhase = vinePhaseOffset + (2 * Math.PI / vinePeriod) * elapsedTime;
            double effectivePhase = naturalPhase + currentAdjustment;

            // Vine endpoint position
            var (offsetX, offsetY) = SwingSimulation.GetSwingPosition(
                (float)effectivePhase, config.Amplitude, config.RopeLength);
            float vineEndX = vineAnchorX + offsetX;
            float vineEndY = vineAnchorY + offsetY;

            // Distance to player
            float dx = vineEndX - playerX;
            float dy = vineEndY - playerY;
            float distance = MathF.Sqrt(dx * dx + dy * dy);

            // Speed multiplier: >1 when far (vine speeds up), 1.0 when close (natural speed)
            // Never below 1.0 — the vine should never appear to slow down
            float speedMultiplier = distance / config.AttractionReferenceDistance;
            float maxMultiplier = Math.Max(config.MaxSpeedMultiplier, 1.0f);
            speedMultiplier = Math.Clamp(speedMultiplier, 1.0f, maxMultiplier);

            // Phase adjustment rate (additional radians/sec beyond natural)
            float additionalPhaseRate = (float)(2 * Math.PI / vinePeriod) * (speedMultiplier - 1f);

            return Math.Clamp(
                currentAdjustment + additionalPhaseRate * deltaTime,
                -config.MaxPhaseAdjustment, config.MaxPhaseAdjustment);
        }
    }
}