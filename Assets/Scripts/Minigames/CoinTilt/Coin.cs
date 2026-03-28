using UnityEngine;

namespace Minigames.CoinTilt {
    public class Coin : MonoBehaviour {
        [SerializeField] private CoinTypeSO coinType;
        [SerializeField] private ParticleSystem pullTrailParticles;
        private float spawnHeight;
        private int pointValue;
        private bool hasBeenCollected;
        private Transform pullTarget;
        private bool isBeingPulled;
        private float pullSpeed;

        public void StartPull(Transform target, float speed) {
            if (!isBeingPulled) {
                isBeingPulled = true;
                pullTarget = target;
                pullSpeed = speed;
                pullTrailParticles?.Play();
            }
        }

        private void Update() {
            if (isBeingPulled && pullTarget != null) {
                Vector3 direction = pullTarget.position - transform.position;
                transform.position += direction * pullSpeed * Time.deltaTime;
                
                EnsureSpawnHeight();
            }
        }

        private void EnsureSpawnHeight() {
            Vector3 localPosition = transform.localPosition;
            localPosition.y = spawnHeight;
            transform.localPosition = localPosition;
        }

        private void Awake() {
            if (coinType != null) {
                pointValue = coinType.PointValue;
            }
            else {
                Debug.LogError("No coin type specified");
            }
        }

        public int Collect() {
            if (hasBeenCollected) return 0;
            hasBeenCollected = true;
            pullTrailParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Destroy(gameObject);
            return pointValue;
        }

        public void InitializeWithType(CoinTypeSO type) {
            coinType = type;
            if (coinType != null) {
                pointValue = coinType.PointValue;
            }
        }

        public void SetSpawnHeight(float height) {
            spawnHeight = height;
        }
    }
}