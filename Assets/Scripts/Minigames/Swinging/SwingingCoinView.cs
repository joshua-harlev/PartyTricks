using UnityEngine;

namespace Minigames.Swinging {
    public class SwingingCoinView : MonoBehaviour {
        private int coinValue;
        private bool isCollected;
        private Transform magnetTarget;
        private float pullSpeed;
        private bool isBeingPulled;
        [SerializeField] private ParticleSystem pullTrailParticles;

        public void StartPull(Transform target, float speed) {
            if(!isBeingPulled) {
                isBeingPulled = true;
                pullTrailParticles?.Play();
            }
            magnetTarget = target;
            pullSpeed = speed;
        }
        
        public void Collect(VineSwingingPlayerView playerView) {
            if (isCollected) return;
            isCollected = true;
            playerView.CollectCoin(coinValue);
            pullTrailParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
            var playerView = other.GetComponentInParent<VineSwingingPlayerView>();
            if (playerView == null) return;
            Collect(playerView);
        }
    }
}