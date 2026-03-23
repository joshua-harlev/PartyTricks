using UnityEngine;

namespace Minigames.Swinging {
    public class SwingingCoinView : MonoBehaviour {
        private int coinValue;
        private bool isCollected;
        private Transform magnetTarget;
        private float pullSpeed;
        private bool isBeingPulled;

        public void StartPull(Transform target, float speed) {
            magnetTarget = target;
            pullSpeed = speed;
            isBeingPulled = true;
        }
        
        public void ForceCollect(VineSwingingPlayerView playerView) {
            if (isCollected) return;
            isCollected = true;
            playerView.CollectCoin(coinValue);
            Destroy(gameObject);
        }

        private void Update() {
            if (isBeingPulled && magnetTarget != null) {
                Vector3 direction = magnetTarget.position - transform.position;
                transform.position += direction * pullSpeed * Time.deltaTime;
            }
        }

        public void Initialize(int coinValue) {
            this.coinValue = coinValue;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (isCollected) return;
            var playerView = other.GetComponentInParent<VineSwingingPlayerView>();
            if (playerView == null) return;
            isCollected = true;
            playerView.CollectCoin(coinValue);
            Destroy(gameObject);
        }
    }
}