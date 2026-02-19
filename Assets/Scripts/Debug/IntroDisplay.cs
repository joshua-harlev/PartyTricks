using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.UIElements;

public class IntroDisplay : MonoBehaviour {
    [SerializeField]
    private UIDocument uiDocument;
    public GameObject MainMenuDocument;
    private Button continueButton;
    private void Awake() { 
        VisualElement root = uiDocument.rootVisualElement; 
        MainMenuDocument.SetActive(false); 
        continueButton = root.Query<Button>("ContinueButton");
        continueButton.clicked += OnContinueClick;
        StartCoroutine(FocusFirstButtonAfterOneFrame());
    }

    private void Update() {
        foreach (var gamepad in Gamepad.all) {
            bool switchAButtonPressed = gamepad is SwitchProControllerHID && gamepad.buttonEast.wasPressedThisFrame;
            bool otherControllerSelectButtonPressed =
                gamepad is not SwitchProControllerHID && gamepad.buttonSouth.wasPressedThisFrame;
            if (switchAButtonPressed || otherControllerSelectButtonPressed) {
                using (var navigationSubmitEvent = NavigationSubmitEvent.GetPooled()) {
                    navigationSubmitEvent.target = continueButton;
                    continueButton.SendEvent(navigationSubmitEvent);
                }
                break;
            }
        }
    }

    private void OnDestroy() {
        continueButton.clicked -= OnContinueClick;
    }

    private void OnContinueClick() {
        MainMenuDocument.SetActive(true);
        Destroy(gameObject);
    }
    
    private IEnumerator FocusFirstButtonAfterOneFrame() {
        yield return null;
        FocusFirstButton();
    }
    
    private void FocusFirstButton() {
        if (continueButton != null) {
            continueButton.Focus();
        }
    }
}
