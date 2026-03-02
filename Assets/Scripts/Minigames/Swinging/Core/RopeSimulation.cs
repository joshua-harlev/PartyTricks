using System;

namespace VineSwinging.Core {
    public class RopeSimulation {
        public readonly int PointCount;
        public readonly float RopeLength;
        
        private Vec2 driveTarget;
        private float driveStiffness;
        private bool hasDriveTarget;
        
        private Vec2[] positions;
        private Vec2[] previousPositions;
        private float segmentLength;
        private static readonly Vec2 Gravity = new Vec2(0, -9.81f);
        private const float Damping = 0.98f;

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
            if (hasDriveTarget) {
                int tip = PointCount - 1;
                positions[tip] = positions[tip] + (driveTarget - positions[tip]) * driveStiffness;
            }
            
            for (int i = 1; i < PointCount; i++) {
                Vec2 temp = positions[i];
                Vec2 velocity = positions[i] - previousPositions[i];
                positions[i] = positions[i] + velocity * Damping + Gravity * (deltaTime * deltaTime);
                previousPositions[i] = temp;
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
                    
                    if (i == 0) {
                        positions[i + 1] -= direction * error;
                    }
                    else {
                        Vec2 correction = direction * error * 0.5f;
                        positions[i] += correction;
                        positions[i + 1] -= correction;
                    }
                }
            }
        }

        public void SetDriveTarget(Vec2 target, float stiffness) {
            driveTarget = target;
            driveStiffness = stiffness;
            hasDriveTarget = true;
        }
    }
}