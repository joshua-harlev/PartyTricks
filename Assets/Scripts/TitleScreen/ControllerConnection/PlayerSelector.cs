using UnityEngine;

namespace Input.ControllerConnection {
    public class PlayerSelector : MonoBehaviour {
        private enum PlayerSelectorState {
            Disabled,
            Pointing,
            EmbodyingPlayer
        }

        private PlayerSelectorState currentState;
        private int associatedPlayerIndex = -1;
        private Player associatedPlayer;
        private IDirectionalTwoButtonInputHandler inputHandler;
        
        [SerializeField] private float pointerSpeed = 5f;
        [SerializeField] private Vector2 pointerTipOffset = new Vector2(0f, -0.5773587f);
        // To find the tip, if using a triangle, use the bottom point of a temp collider, make negative if flip y is true
        
        [Tooltip("Height above player to follow at")]
        [SerializeField] private Vector2 embodiedOffset = new Vector2(0f, 1f);
        [SerializeField] private SpriteRenderer pointerSprite;

        public void Initialize(Color pointerColor, IDirectionalTwoButtonInputHandler input) {
            pointerSprite.color = pointerColor;
            inputHandler = input;
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
            associatedPlayer.Disassociate();
            associatedPlayer = null;
            currentState = PlayerSelectorState.Pointing;
        }

        private void SelectPlayer() {
            Player playerToSelect = GetPlayerBelowPointer();
            if (playerToSelect == null) return;
            associatedPlayer = playerToSelect;
            AttachToPlayer();
        }

        
        private Player GetPlayerBelowPointer() {
            Vector2 pointToCheck = transform.TransformPoint(pointerTipOffset);
            foreach (var hit in Physics2D.OverlapPointAll(pointToCheck)) {
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
            currentState = PlayerSelectorState.EmbodyingPlayer;
        }

        public void Disable() {
            currentState = PlayerSelectorState.Disabled;
        }

        public void Enable() {
            currentState = PlayerSelectorState.Pointing;
        }
    }
}
