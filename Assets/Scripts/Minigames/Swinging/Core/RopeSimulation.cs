using System;

namespace Minigames.Swinging.Core {
    public class RopeSimulation {
        public readonly int PointCount;
        public readonly float RopeLength;
        
        private Vec2 driveTarget;
        private bool hasDriveTarget;
        
        private Vec2[] positions;
        private Vec2[] previousPositions;
        private float segmentLength;
        private static readonly Vec2 Gravity = new Vec2(0, -1f);
        private const float Damping = 0.99f;

        public RopeSimulation(int pointCount, float ropeLength) {
            PointCount = pointCount;
            RopeLength = ropeLength;
            positions = new Vec2[PointCount];
            previousPositions = new Vec2[PointCount];
            segmentLength = ropeLength / (pointCount - 1);
            for (int i = 0; i < pointCount; i++) {
                positions[i] = new Vec2(0, -segmentLength * i);
            }

            Array.Copy(positions, previousPositions, pointCount);
        }

        public Vec2[] GetPositions() => positions;

        public void Simulate(float deltaTime) {
            int end = hasDriveTarget ? PointCount - 1 : PointCount;
            
            for (int i = 1; i < end; i++) {
                Vec2 temp = positions[i];
                Vec2 velocity = positions[i] - previousPositions[i];
                positions[i] = positions[i] + velocity * Damping + Gravity * (deltaTime * deltaTime);
                previousPositions[i] = temp;
            }
            
            if (hasDriveTarget) {
                int tip = PointCount - 1;
                positions[tip] = driveTarget;
            }
        }

        public void Constrain() {
            // more iterations -> closer to valid solution
            int iterations = 5;
            
            for (int j = 0; j < iterations; j++) {
                for (int i = 0; i < PointCount-1; i++) {
                    Vec2 delta = positions[i + 1] - positions[i];
                    float distance = delta.Length;
                    float error = distance - segmentLength;
                    Vec2 direction = delta.Normalized;

                    bool isAnchor = (i == 0);
                    bool isDrivenTip = (hasDriveTarget && i + 1 == PointCount - 1);

                    if (isAnchor && isDrivenTip) {
                        // both ends are pinned
                    } else if (isAnchor) {
                        positions[i+1] -= direction * error;
                    } else if (isDrivenTip) {
                        positions[i] += direction * error;
                    } else {
                        Vec2 correction = direction * error * 0.5f;
                        positions[i] += correction;
                        positions[i + 1] -= correction;
                    }
                }
            }
        }

        public void SetDriveTarget(Vec2 target) {
            driveTarget = target;
            hasDriveTarget = true;
        }
        
        public void ApplyTautness(float tautness, float curveOffset = 0f) {
            if (!hasDriveTarget || tautness <= 0) return;
            
            Vec2 anchor = positions[0];
            Vec2 tip = positions[PointCount - 1];
            Vec2 line = tip - anchor;
            if (line.Length < 0.0001f) return;
            
            Vec2 lineDirection = line.Normalized;
            Vec2 perpendicular = new Vec2(-lineDirection.Y, lineDirection.X);
            
            Vec2 midpoint = (anchor + tip) * 0.5f;
            Vec2 control = midpoint + perpendicular * curveOffset;
            
            for (int i = 1; i < PointCount - 1; i++) {
                // bezier interpolation (variable names are typical)
                float t = (float)i / (PointCount - 1);
                float u = 1f - t;
                Vec2 ideal = anchor * (u * u) + control * (2f * u * t) + tip * (t * t);
                positions[i] = positions[i] + (ideal - positions[i]) * tautness;
            }
        }
    }
}