using Minigames.Swinging.Core;
using Xunit;

namespace Swinging.Core.Tests {
    public class SweetSpotTests {
        [Fact]
        public void IdealReleasePhaseIsInsideGlowWindow() {
            Assert.True(SweetSpot.IsInGlowWindow(SweetSpot.IdealReleasePhase));
        }

        [Theory]
        [InlineData(0.3f)]
        [InlineData(0.7f)]
        public void GlowWindowContainsPhasesAroundIdeal(float phase) {
            Assert.True(SweetSpot.IsInGlowWindow(phase));
        }

        [Theory]
        [InlineData(0.2f)]
        [InlineData(0.8f)]
        [InlineData(2.6f)]
        public void GlowWindowExcludesPhasesOutsideWindow(float phase) {
            Assert.False(SweetSpot.IsInGlowWindow(phase));
        }

        [Fact]
        public void IdealPhasePassesModerateThreshold() {
            Assert.True(SweetSpot.IsInReleaseWindow(SweetSpot.IdealReleasePhase, threshold: 0.3f));
        }

        [Fact]
        public void IdealPhaseFailsStrictThreshold() {
            Assert.False(SweetSpot.IsInReleaseWindow(SweetSpot.IdealReleasePhase, threshold: 0.9f));
        }
        
        [Fact]
        public void BottomOfSwingAlwaysFailsPositiveThreshold() {
            Assert.False(SweetSpot.IsInReleaseWindow(0f, threshold: 0.3f));
        }
    }
}