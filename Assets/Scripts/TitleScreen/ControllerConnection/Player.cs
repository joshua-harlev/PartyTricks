using UnityEngine;

namespace Input.ControllerConnection {
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour {
        private Rigidbody2D playerRigidbody;
        public bool HasAssociatedSelector { get; private set; }
        
        [SerializeField] private float JumpForce = 10f;

        private void Awake() {
            playerRigidbody = GetComponent<Rigidbody2D>();
        }

        public void Move(Vector2 moveDirection, bool jumpWasPressed) {
            moveDirection.y = 0f;
            playerRigidbody.MovePosition(playerRigidbody.position + moveDirection * Time.deltaTime);
            if (jumpWasPressed && playerRigidbody.linearVelocity.y == 0) {
                playerRigidbody.AddForceY(JumpForce, ForceMode2D.Impulse);
            }
        }

        public void Associate() {
            HasAssociatedSelector = true;
        }
        
        public void Disassociate() {
            HasAssociatedSelector = false;
        }
    }
}