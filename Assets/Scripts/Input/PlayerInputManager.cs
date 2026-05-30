using UnityEngine;
using UnityEngine.InputSystem;

namespace Input {
    public class PlayerInputManager : MonoBehaviour {
        public Mode mode = Mode.UI;

        public enum Mode {
            Gameplay,
            UI
        }
    
        private void Awake() {
            if (this.mode == Mode.UI) {
                InputSystem.actions.FindActionMap("Player").Disable();
                InputSystem.actions.FindActionMap("UI").Enable();
            }
            else {
                InputSystem.actions.FindActionMap("UI").Disable();
                InputSystem.actions.FindActionMap("Player").Enable();
            }
        }
    }
}
