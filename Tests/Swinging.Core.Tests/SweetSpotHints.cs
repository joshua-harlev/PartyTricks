using Minigames.Swinging.Core.PlayerStateMachine;
using Xunit;

namespace Swinging.Core.Tests {
    public class SweetSpotHints {
        private const float SuccessDecrement = 0.34f;
        private const float FailureIncrement = 0.5f;
        private const float FadeSpeed = 2.5f;
        private const float SettleDeltaTime = 1f;

        private static void Advance(PlayerContext context, bool madeProgress, bool fell,
            float deltaTime = SettleDeltaTime) {
            context.UpdateSweetSpotHint(madeProgress, fell, deltaTime, SuccessDecrement, FailureIncrement, FadeSpeed);
        }

        [Fact]
        public void NewContextStartsWithHintFullyShown() {
            PlayerContext context = new();
            Assert.Equal(1f, context.TargetHintLevel);
            Assert.Equal(1f, context.DisplayedHintLevel);
        }

        [Fact]
        public void ProgressLowersTargetByDecrement() {
            PlayerContext context = new();
            Advance(context, true, false);
            Assert.Equal(1f-SuccessDecrement, context.TargetHintLevel, 4);
        }

        [Fact]
        public void FallRaisesTargetByIncrement() {
            PlayerContext context = new();
            Advance(context, true, false);
            Advance(context, true, false);
            float previousTargetHintLevel = context.TargetHintLevel;
            Advance(context, false, true);
            Assert.Equal(previousTargetHintLevel + FailureIncrement, context.TargetHintLevel, 4);
        }

        [Fact]
        public void ConsistentProgressFullyHidesHint() {
            PlayerContext context = new();
            for(int i = 0; i<3; i++) Advance(context, true, false);
            Assert.Equal(0f, context.TargetHintLevel);
            Assert.Equal(0f, context.DisplayedHintLevel);
        }

        [Fact]
        public void FallingAfterHiddenBringsHintBack() {
            PlayerContext context = new();
            for(int i = 0; i<3; i++) Advance(context, true, false);
            Assert.Equal(0f, context.DisplayedHintLevel);
            
            Advance(context, false, true);
            Assert.True(context.DisplayedHintLevel > 0f);
            Advance(context, false, true);
            Assert.Equal(1f, context.TargetHintLevel);
            Assert.Equal(1f, context.DisplayedHintLevel);
        }
        
        [Fact]
        public void TargetNeverGoesBelowZero() {
            PlayerContext context = new();
            for(int i = 0; i<10; i++) Advance(context, true, false);
            Assert.Equal(0f, context.TargetHintLevel);
        }
        
        [Fact]
        public void TargetNeverExceedsOne() {
            PlayerContext context = new();
            for(int i = 0; i<10; i++) Advance(context, false, true);
            Assert.Equal(1f, context.TargetHintLevel);
        }

        [Fact]
        public void DisplayedEasesTowardTargetWhenStepIsSmall() {
            PlayerContext context = new();
            context.UpdateSweetSpotHint(true, false, 0f, SuccessDecrement, FailureIncrement, FadeSpeed);
            Assert.Equal(1f-SuccessDecrement, context.TargetHintLevel, 4);
            Assert.Equal(1f, context.DisplayedHintLevel);
            
            context.UpdateSweetSpotHint(false, false, 0.1f, SuccessDecrement, FailureIncrement, FadeSpeed);
            Assert.Equal(0.75f, context.DisplayedHintLevel, 4);
        }

        [Fact]
        public void DisplayedSnapsToTargetWhenWithinStep() {
            PlayerContext context = new();
            context.UpdateSweetSpotHint(true, false, 0f, SuccessDecrement, FailureIncrement, FadeSpeed);
            context.UpdateSweetSpotHint(false, false, 0.2f, SuccessDecrement, FailureIncrement, FadeSpeed);
            Assert.Equal(context.TargetHintLevel, context.DisplayedHintLevel, 4);
        }
    }
}