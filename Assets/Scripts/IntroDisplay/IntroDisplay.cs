using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace IntroDisplay {
    public class IntroDisplay : MonoBehaviour {
        [SerializeField]
        private UIDocument uiDocument;
        public GameObject MainMenuDocument;
        private Button continueButton;
        private IPlayerService playerService;
        private readonly List<InputAction> subscribedSubmitActions = new();

        private void Awake() {
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            VisualElement root = uiDocument.rootVisualElement;
            MainMenuDocument.SetActive(false);
            continueButton = root.Query<Button>("ContinueButton");
            continueButton.clicked += OnContinueClick;
            StartCoroutine(FocusFirstButtonAfterOneFrame());
            SubscribeExistingGamepadPlayers();
            playerService.OnPlayerJoined += HandlePlayerJoined;
        }

        private void HandlePlayerJoined(int playerIndex, PlayerProfile profile = null) {
            var slot = playerService.PlayerSlots[playerIndex];
            if (slot.PlayerInput == null) return;
            if (slot.PlayerInput.devices.Any(device => device is Keyboard || device is Mouse)) return;
            SubscribeGamepadSubmitAction(slot.PlayerInput);
        }

        private void SubscribeExistingGamepadPlayers() {
            foreach (var slot in playerService.PlayerSlots) {
                if (!slot.IsOccupied || slot.PlayerInput == null) continue;
                if (slot.PlayerInput.devices.Any(device => device is Keyboard || device is Mouse)) continue;
                SubscribeGamepadSubmitAction(slot.PlayerInput);
            }
        }

        private void SubscribeGamepadSubmitAction(PlayerInput playerInput) {
            var submitAction = playerInput.actions.FindAction("UI/Submit");
            if (submitAction == null) return;
            submitAction.performed += OnGamepadSubmitPerformed;
            subscribedSubmitActions.Add(submitAction);
        }
    
        private void OnGamepadSubmitPerformed(InputAction.CallbackContext context) {
            using var navEvent = NavigationSubmitEvent.GetPooled();
            navEvent.target = continueButton;
            continueButton.SendEvent(navEvent);
        }

        private void OnDestroy() {
            continueButton.clicked -= OnContinueClick;
            playerService.OnPlayerJoined -= HandlePlayerJoined;
            foreach (var action in subscribedSubmitActions) {
                action.performed -= OnGamepadSubmitPerformed;
            }
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
}
