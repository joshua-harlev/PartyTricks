using DG.Tweening;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input.ControllerConnection {
    public class PlayerSelector : MonoBehaviour {
        private enum PlayerSelectorState {
            Disabled,
            Pointing,
            EmbodyingPlayer
        }

        private PlayerSelectorState currentState;
        private Player associatedPlayer;
        private IDirectionalTwoButtonInputHandler inputHandler;
        private IPlayerService playerService;
        private PlayerInput playerInput;
        
        [SerializeField] private float pointerSpeed = 5f;
        [SerializeField] private Vector2 pointerTipOffset = new Vector2(0f, -0.5773587f);
        // To find the tip, if using a triangle, use the bottom point of a temp collider, make negative if flip y is true
        
        [Tooltip("Height above player to follow at")]
        [SerializeField] private Vector2 embodiedOffset = new Vector2(0f, 1f);
        [SerializeField] private SpriteRenderer pointerSprite;
        [SerializeField] private float pointerSelectionRadius = 0.5f;
        

        public void Initialize(Color pointerColor, PlayerInput playerInput, IDirectionalTwoButtonInputHandler input, IPlayerService playerService) {
            pointerSprite.color = pointerColor;
            inputHandler = input;
            this.playerService = playerService;
            this.playerInput = playerInput;
        }

        private void Awake() {
            currentState = PlayerSelectorState.Pointing;
        }

        private void Update() {
            if (currentState == PlayerSelectorState.Disabled) return;
            Vector2 navigateDirection = inputHandler.GetNavigate();
            if (currentState == PlayerSelectorState.Pointing) {
                if (navigateDirection != Vector2.zero) {
                    var position = transform.position;
                    position.x += navigateDirection.x * Time.deltaTime * pointerSpeed;
                    position.y += navigateDirection.y * Time.deltaTime * pointerSpeed;
                    transform.position = position;
                }

                if (inputHandler.SelectIsPressed()) {
                    SelectPlayer();
                }
            } else if (currentState == PlayerSelectorState.EmbodyingPlayer) {
                if (inputHandler.CancelIsPressed()) {
                    DetachPlayer();
                } else associatedPlayer.Move(navigateDirection, inputHandler.SelectIsPressed());
            }
        }

        private void LateUpdate() {
            if (currentState != PlayerSelectorState.EmbodyingPlayer || associatedPlayer == null) return;
            transform.position = (Vector2)associatedPlayer.transform.position + embodiedOffset;
        }

        private void DetachPlayer() {
            playerService.TryReleaseSlot(associatedPlayer.SlotIndex);
            associatedPlayer.Disassociate();
            associatedPlayer = null;
            UnfadeOpacity();
            currentState = PlayerSelectorState.Pointing;
        }

        private void SelectPlayer() {
            Player playerToSelect = GetPlayerBelowPointer();
            if (playerToSelect == null) return;
            if(!playerService.TryAssignInputToSlot(playerInput, playerToSelect.SlotIndex)) return;
            associatedPlayer = playerToSelect;
            AttachToPlayer();
        }

        
        private Player GetPlayerBelowPointer() {
            Vector2 pointToCheck = transform.TransformPoint(pointerTipOffset);
            foreach (var hit in Physics2D.OverlapCircleAll(pointToCheck, pointerSelectionRadius)) {
                if (hit.TryGetComponent(out Player player)) {
                    if (!player.HasAssociatedSelector) {
                        return player;
                    }
                }
            }

            return null;
        }

        private void AttachToPlayer() {
            associatedPlayer.Associate();
            FadeOpacity();
            currentState = PlayerSelectorState.EmbodyingPlayer;
        }

        private void FadeOpacity() {
            pointerSprite.DOFade(0.5f, 0.5f);
        }

        private void UnfadeOpacity() {
            pointerSprite.DOFade(1f, 0.5f);
        }

        public void Disable() {
            currentState = PlayerSelectorState.Disabled;
        }

        public void Enable() {
            currentState = PlayerSelectorState.Pointing;
        }
    }
}
