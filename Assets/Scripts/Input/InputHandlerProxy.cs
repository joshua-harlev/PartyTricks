using UnityEngine;

namespace Input {
    // Holds a reference to the current Input Handler so that they can be safely swapped out mid-game.
    public class InputHandlerProxy : IDirectionalTwoButtonInputHandler {
        private IDirectionalTwoButtonInputHandler target;
        
        public void SetTarget(IDirectionalTwoButtonInputHandler handler) {
            target = handler;
        }

        public InputHandlerProxy(IDirectionalTwoButtonInputHandler target) {
            this.target = target;
        }
        
        public Vector2 GetNavigate() => target?.GetNavigate() ?? Vector2.zero;
        public bool SelectIsPressed() => target?.SelectIsPressed() ?? false;
        public bool CancelIsPressed() => target?.CancelIsPressed() ?? false;
        public bool IsActive() => target?.IsActive() ?? false;
        public bool ChargeIsPressed() => target?.ChargeIsPressed() ?? false;
        public bool ChargeIsHeld() => target?.ChargeIsHeld() ?? false;
        public bool ChargeIsReleased() => target?.ChargeIsReleased() ?? false;
        public bool IsOccupied => target != null;
    }
}