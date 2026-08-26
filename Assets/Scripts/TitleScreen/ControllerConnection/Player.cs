using CoreData;
using Services;
using UnityEngine;

namespace Input.ControllerConnection {
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour {
        private Rigidbody2D playerRigidbody;
        public bool HasAssociatedSelector { get; private set; }
        private Vector2 lastMoveDirection = Vector2.zero;

        [SerializeField] private int PlayerSlotIndex;
        [SerializeField] private SpriteRenderer SpriteRenderer;
        
        [SerializeField] private float MaxGroundAngle = 30f;
        private ContactFilter2D groundFilter;
        
        [SerializeField] private float JumpForce = 10f;
        [SerializeField] private float Speed = 5f;
        [SerializeField] private float JumpBufferTimeInSeconds = 0.1f;
        
        private float jumpBufferedUntil;

        private void Awake() {
            playerRigidbody = GetComponent<Rigidbody2D>();
            groundFilter.useNormalAngle = true;
            groundFilter.SetNormalAngle(90f - MaxGroundAngle, 90f + MaxGroundAngle);
            Color playerColor = ServiceLocatorAccessor.GetService<PlayerColorConfig>().GetMainColor(PlayerSlotIndex);
            SpriteRenderer.color = playerColor;
        }

        public void Move(Vector2 moveDirection, bool jumpWasPressed) {
            moveDirection.y = 0f;
            lastMoveDirection = moveDirection;
            if(jumpWasPressed) jumpBufferedUntil = Time.time + JumpBufferTimeInSeconds;
        }

        private void FixedUpdate() {
            Vector2 velocity = playerRigidbody.linearVelocity;
            velocity.x = lastMoveDirection.x * Speed;
            playerRigidbody.linearVelocity = velocity;
            
            if (Time.time <= jumpBufferedUntil && IsGrounded) {
                playerRigidbody.AddForceY(JumpForce, ForceMode2D.Impulse);
                jumpBufferedUntil = 0f;
            }
        }

        private bool IsGrounded => playerRigidbody.IsTouching(groundFilter);

        public void Associate() {
            HasAssociatedSelector = true;
        }
        
        public void Disassociate() {
            HasAssociatedSelector = false;
            lastMoveDirection = Vector2.zero;
            jumpBufferedUntil = 0f;
        }
    }
}