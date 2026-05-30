using Minigames.Swinging.Core;
using Minigames.Swinging.Core.PlayerStateMachine;
using Xunit;

namespace Swinging.Core.Tests {
    public class PlayerStateMachineTests {
        private const float OneFrame = 0.01f;
        private const float VineAnchorYPosition = 10f;
        private const float Vine1XPosition = 20f;
        
        private static PlayerStateMachine CreateTwoVineMachine(SwingConfig config = default) {
            if (config.Equals(default(SwingConfig))) {
                config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition);
            }

            float[] vineXPositions = { 0f, Vine1XPosition };
            float[] vinePhases = { 0f, 0f };
            float[] vinePeriods = { 2f, 2f };
            return new PlayerStateMachine(config, vineXPositions, vineAnchorY: VineAnchorYPosition, vinePhases, vinePeriods);
        }

        private static void TeleportToVineTip(PlayerStateMachine stateMachine, int vineIndex, SwingConfig config) {
            stateMachine.PlayerContext.PositionX = stateMachine.VineXPositions[vineIndex];
            stateMachine.PlayerContext.PositionY = VineAnchorYPosition - config.RopeLength;
            stateMachine.PlayerContext.VelocityX = 0f;
            stateMachine.PlayerContext.VelocityY = 0f;
        }
        
        private static void TeleportBelowFallThreshold(PlayerStateMachine stateMachine, SwingConfig config) {
            stateMachine.PlayerContext.PositionX = stateMachine.VineXPositions[stateMachine.PlayerContext.CurrentVineIndex];
            stateMachine.PlayerContext.PositionY = config.FallThresholdY - 1f;
            stateMachine.PlayerContext.VelocityX = 0f;
            stateMachine.PlayerContext.VelocityY = 0f;
        }

        [Fact]
        public void Start_BeginsInSwingingStateAtFirstVine() {
            var stateMachine = CreateTwoVineMachine();
            stateMachine.Start(0);
            Assert.Equal(PlayerStateType.Swinging, stateMachine.PlayerContext.CurrentStateType);
            Assert.Equal(0, stateMachine.PlayerContext.CurrentVineIndex);
        }

        [Fact]
        public void Release_TransitionsToAirborneState() {
            var stateMachine = CreateTwoVineMachine();
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            Assert.Equal(PlayerStateType.Airborne, stateMachine.PlayerContext.CurrentStateType);
        }

        [Fact]
        public void Release_FiresLaunchedEvent() {
            var stateMachine = CreateTwoVineMachine();
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);

            Assert.Contains(PlayerEvent.Launched, stateMachine.PlayerContext.PendingEvents);
        }

        [Fact]
        public void Release_EnforcesMinimumXReleaseVelocity() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, minimumReleaseVelocityX: 100f);
            var stateMachine = CreateTwoVineMachine(config);
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            
            Assert.Equal(PlayerStateType.Airborne, stateMachine.PlayerContext.CurrentStateType);
            Assert.Equal(100f, stateMachine.PlayerContext.VelocityX);
        }

        [Fact]
        public void Airborne_GravityReducesYVelocity() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, minimumReleaseVelocityX: 100f);
            var stateMachine = CreateTwoVineMachine(config);
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            var initialYVelocity = stateMachine.PlayerContext.VelocityY;
            stateMachine.Update(0.4f, true);
            var currentYVelocity = stateMachine.PlayerContext.VelocityY;
            Assert.True(initialYVelocity > currentYVelocity);
        }

        [Fact]
        public void Airborne_TransitionsToSwingingWhenNextVineGrabbed() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, grabRadius: 2f);
            var stateMachine = CreateTwoVineMachine(config);
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            Assert.Equal(PlayerStateType.Airborne, stateMachine.PlayerContext.CurrentStateType);

            TeleportToVineTip(stateMachine, 1, config);
            stateMachine.Update(OneFrame, false);
            
            Assert.Equal(PlayerStateType.Swinging, stateMachine.PlayerContext.CurrentStateType);
            Assert.Equal(1, stateMachine.PlayerContext.CurrentVineIndex);
        }
        
        [Fact]
        public void Airborne_GrabEventIsFiredOnGrab() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, grabRadius: 2f);
            var stateMachine = CreateTwoVineMachine(config);
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            
            TeleportToVineTip(stateMachine, 1, config);
            stateMachine.PlayerContext.ClearEvents();
            stateMachine.Update(OneFrame, false);

            Assert.Contains(PlayerEvent.GrabbedVine, stateMachine.PlayerContext.PendingEvents);
        }

        [Fact]
        public void Airborne_FurthestVineIndexUpdatedOnGrab() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, grabRadius: 2f);
            var stateMachine = CreateTwoVineMachine(config);
            
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            int originalVineIndex = stateMachine.PlayerContext.CurrentVineIndex;
            
            TeleportToVineTip(stateMachine, 1, config);
            
            stateMachine.Update(OneFrame, false);
            int newVineIndex = stateMachine.PlayerContext.CurrentVineIndex;
            Assert.True(newVineIndex > originalVineIndex);
        }

        [Fact]
        public void Airborne_TransitionsToFallingWhenFallsBelowThreshold() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, grabRadius: 2f);
            var stateMachine = CreateTwoVineMachine(config);
            
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            
            TeleportBelowFallThreshold(stateMachine, config);
            
            stateMachine.Update(OneFrame, false);
            
            Assert.Equal(PlayerStateType.Falling, stateMachine.PlayerContext.CurrentStateType);
        }

        [Fact]
        public void Falling_FiresFellEvent() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, grabRadius: 2f);
            var stateMachine = CreateTwoVineMachine(config);
            
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            
            TeleportBelowFallThreshold(stateMachine, config);
            
            stateMachine.Update(OneFrame, false);
            
            Assert.Contains(PlayerEvent.Fell, stateMachine.PlayerContext.PendingEvents);
        }

        [Fact]
        public void Falling_RespawnsAfterDelay() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, grabRadius: 2f);
            var stateMachine = CreateTwoVineMachine(config);
            
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            
            TeleportBelowFallThreshold(stateMachine, config);
            
            stateMachine.Update(OneFrame, false);
            
            Assert.Equal(PlayerStateType.Falling, stateMachine.PlayerContext.CurrentStateType);
            
            stateMachine.Update(config.RespawnDelay, false);
            Assert.Equal(PlayerStateType.Swinging, stateMachine.PlayerContext.CurrentStateType);
        }

        [Fact]
        public void Falling_RespawnsAtFurthestVine() {
            var config = TestHelpers.DefaultConfig(vineSpacing: Vine1XPosition, grabRadius: 2f);
            var stateMachine = CreateTwoVineMachine(config);
            
            stateMachine.Start(0);
            stateMachine.Update(OneFrame, true);
            
            TeleportToVineTip(stateMachine, 1, config);
            stateMachine.Update(OneFrame, false);
            Assert.Equal(1, stateMachine.PlayerContext.FurthestVineIndex);
            
            stateMachine.Update(OneFrame, true);
            TeleportBelowFallThreshold(stateMachine, config);
            
            stateMachine.Update(OneFrame, false);
            Assert.Equal(PlayerStateType.Falling, stateMachine.PlayerContext.CurrentStateType);
            
            stateMachine.Update(config.RespawnDelay, false);
            Assert.Equal(PlayerStateType.Swinging, stateMachine.PlayerContext.CurrentStateType);
            Assert.Equal(1, stateMachine.PlayerContext.CurrentVineIndex);
            Assert.Equal(Vine1XPosition, stateMachine.PlayerContext.PositionX);
        }
    }
}