using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUITwoButtonInputHandler : MonoBehaviour, IDirectionalTwoButtonInputHandler {
    private PlayerInput playerInput;
    private Gamepad gamepad;
    private InputAction navigateAction;
    private InputAction cancelAction;
    private InputAction selectAction;
    private InputAction chargeAction;
    private bool selectIsPressed;
    private bool cancelIsPressed;
    private bool chargeIsPressed;
    
    public bool OneHandedMode { get; set; }

    public void Initialize(PlayerInput playerInput) {
        this.playerInput = playerInput;
        var actions = playerInput.currentActionMap;
        navigateAction = actions["Navigate"];
        cancelAction = actions["Cancel"];
        selectAction = actions["Submit"];
        chargeAction = actions["Charge"];
        foreach (var device in playerInput.devices) {
            if (device is Gamepad gp) { gamepad = gp; break; }
        }
    }

    private void Update() {
        selectIsPressed = selectAction.WasPressedThisFrame();
        cancelIsPressed = cancelAction.WasPressedThisFrame();
        chargeIsPressed = chargeAction.WasPressedThisFrame();
        if (OneHandedMode && gamepad != null) {
            bool shoulderPressed = gamepad.leftShoulder.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame;
            bool triggerPressed = gamepad.leftTrigger.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame;
            selectIsPressed |= shoulderPressed;
            cancelIsPressed |= triggerPressed;
            chargeIsPressed |= shoulderPressed;
        }
    }

    public Vector2 GetNavigate() {
        return navigateAction.ReadValue<Vector2>();
    }

    public bool SelectIsPressed() {
        return selectIsPressed;
    }

    public bool CancelIsPressed() {
        return cancelIsPressed;
    }

    public bool IsActive() {
        return true;
    }
    
    public bool ChargeIsPressed() {
        return chargeAction.WasPressedThisFrame();
    }

    public bool ChargeIsHeld() {
        if (OneHandedMode && gamepad != null) {
            return chargeAction.IsPressed() || gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed;
        }
        return chargeAction.IsPressed();
    }
    
    public bool ChargeIsReleased() {
        if (OneHandedMode && gamepad != null) {
            return chargeAction.WasReleasedThisFrame() || gamepad.leftShoulder.wasReleasedThisFrame || gamepad.rightShoulder.wasReleasedThisFrame;
        }
        return chargeAction.WasReleasedThisFrame();
    }
}
