using System;

namespace VineSwinging.Core {
    public struct CoinPosition {
        public float RelativeXPosition; // 0.0 when releasing, 1.0 at next vine
        public float RelativeYPosition;
    }
    public class CoinTrailGenerator {
        public static CoinPosition[][] GenerateAllTrails(int vineCount, SwingConfig config, int seed) {
            float swingReach = config.RopeLength * (float)(Math.Sin(config.Amplitude));
            float safeMargin = 1.5f; // estimated value; player extents + coin radius
            float flightStartX = (swingReach + safeMargin) / config.VineSpacing;
            float flightEndX = 1f - flightStartX;

            float idealPhase = 0.55f;
            var (releaseVx, releaseVy) = SwingSimulation.GetShapedReleaseVelocity(idealPhase, config.Amplitude,
                config.Period, config.LaunchForce, config.RopeLength, config.ReleaseCurveExponent);
            releaseVy *= config.VerticalLaunchScale;

            var (releaseOffsetX, releaseOffsetY) =
                SwingSimulation.GetSwingPosition(idealPhase, config.Amplitude, config.RopeLength);
            
            CoinPosition[][] trails = new CoinPosition[vineCount-1][];
            trails[0] = new CoinPosition[0];
            
            for (int i = 1; i < vineCount - 1; i++) {
                trails[i] = new CoinPosition[config.CoinsPerGap];
                for (int coinIndex = 0; coinIndex < config.CoinsPerGap; coinIndex++) {
                    float fractionAlongArc = (coinIndex + 1f) / (config.CoinsPerGap + 1f);
                    float xFraction = flightStartX + fractionAlongArc * (flightEndX - flightStartX);

                    float worldXFromRelease = xFraction * config.VineSpacing - releaseOffsetX;
                    float t = 0f;
                    if(releaseVx > 0) t = worldXFromRelease / releaseVx;
                    if (t < 0) t = 0;
                    float trajectoryRelativeY = releaseOffsetY + releaseVy * t - 0.5f * config.Gravity * t * t;
                    trajectoryRelativeY -= config.CoinBaseHeightRatio * config.RopeLength;
                    
                    trails[i][coinIndex] = new CoinPosition
                    {
                        RelativeXPosition = xFraction,
                        RelativeYPosition = trajectoryRelativeY
                    };
                }
            }
            return trails;
        }
    }
}