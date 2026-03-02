using VineSwinging.Core;
using Xunit;

namespace Swinging.Core.Tests {
    public class RopeSimulationTests {
        private const int PointCount = 6;
        private const float RopeLength = 5f;
        private const float SegmentLength = RopeLength / (PointCount - 1);

        [Fact]
        public void InitializePlacesPointsInStraightLineDownward() {
            var rope = new RopeSimulation(PointCount, RopeLength);
            var positions = rope.GetPositions();

            Assert.Equal(PointCount, positions.Length);

            Assert.Equal(0f, positions[0].X);
            Assert.Equal(0f, positions[0].Y);

            for (int i = 1; i < PointCount; i++) {
                Assert.Equal(0f, positions[i].X, precision: 4);
                Assert.Equal(-SegmentLength * i, positions[i].Y, precision: 4);
            }
        }

        [Fact]
        public void GravityPullsNonAnchorPointsDown() {
            var rope = new RopeSimulation(PointCount, RopeLength);
            var previousYPosition = rope.GetPositions()[3].Y;
            rope.Simulate(1f / 60f);
            var newYPosition = rope.GetPositions()[3].Y;
            
            Assert.True(newYPosition < previousYPosition, $"Expected Y to decrease. Before: {previousYPosition}, After: {newYPosition}");
            Assert.Equal(0f, rope.GetPositions()[0].X);
            Assert.Equal(0f, rope.GetPositions()[0].Y);
        }

        [Fact]
        public void ConstrainMaintainsSegmentLengths() {
            var rope = new RopeSimulation(PointCount, RopeLength);

            for (int i = 0; i < 10; i++) {
                rope.Simulate(1f / 60f);
            }

            rope.Constrain();

            var positions = rope.GetPositions();
            for (int i = 0; i < PointCount - 1; i++) {
                float distance = (positions[i+1] - positions[i]).Length;
                Assert.Equal(SegmentLength, distance, precision: 1);
            }
        }

        [Fact]
        public void AnchorRemainsAtOriginWhenConstrained() {
            var rope = new RopeSimulation(PointCount, RopeLength);

            for (int i = 0; i < 10; i++) {
                rope.Simulate(1f / 60f);
            }
            
            rope.Constrain();
            
            Assert.Equal(0f, rope.GetPositions()[0].X);
            Assert.Equal(0f, rope.GetPositions()[0].Y);
        }

        [Fact]
        public void TipMovesTowardsDriveTarget() {
            var rope = new RopeSimulation(PointCount, RopeLength);

            var target = new Vec2(2f, -RopeLength);
            rope.SetDriveTarget(target, stiffness: 0.5f);

            for (int i = 0; i < 30; i++) {
                rope.SetDriveTarget(target, stiffness: 0.5f);
                rope.Simulate(1f/60f);
                rope.Constrain();
            }
            
            var tip = rope.GetPositions()[PointCount - 1];
            
            Assert.True(tip.X > 0f, $"Expected tip to move right toward target. Tip X: {tip.X}");
        }
    }
}