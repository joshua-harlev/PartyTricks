using System.Collections.Generic;
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
        
        private HashSet<Collider2D> colliders;

        [SerializeField] private SpriteRenderer pointerSprite;

        public void Initialize(Color pointerColor) {
            pointerSprite.color = pointerColor;
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
                    position.x += navigateDirection.x * Time.deltaTime;
                    position.y += navigateDirection.y * Time.deltaTime;
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

        private void DetachPlayer() {
            associatedPlayer.Disassociate();
            associatedPlayer = null;
        }

        private void SelectPlayer() {
            Player playerToSelect = GetPlayerBelowPointer();
            if (playerToSelect == null) return;
            associatedPlayer = playerToSelect;
            AttachToPlayer();
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (!other.collider.CompareTag("Player")) return;
            colliders.Add(other.collider);
        }

        private void OnCollisionExit2D(Collision2D other) {
            if (!other.collider.CompareTag("Player")) return;
            if (colliders.Contains(other.collider)) {
                colliders.Remove(other.collider);
            }
        }

        private Player GetPlayerBelowPointer() {
            foreach (Collider2D availableCollider in colliders) {
                Player player = availableCollider.GetComponent<Player>();
                if (player == null) continue;
                if (player.HasAssociatedSelector) continue;
                else return player;
            }

            return null;
        }

        private void AttachToPlayer() {
            associatedPlayer.Associate();
        }

        public void Disable() {
            currentState = PlayerSelectorState.Disabled;
        }

        public void Enable() {
            currentState = PlayerSelectorState.Pointing;
        }
    }
}
