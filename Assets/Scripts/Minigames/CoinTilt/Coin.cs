using System.Collections;
using UnityEngine;

namespace Minigames.CoinTilt {
    public class Coin : MonoBehaviour {
        [SerializeField] private CoinTypeSO coinType;
        [SerializeField] private ParticleSystem pullTrailParticles;
        [SerializeField] private Renderer renderer;
        private float spawnHeight;
        private int pointValue;
        private bool hasBeenCollected;
        private Transform pullTarget;
        private bool isBeingPulled;
        private float pullSpeed;
        public bool IsSpecialCoin => coinType.IsSpecialCoin;
        private Coroutine flashCoroutine;

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

        public void PrepareForDestruction(float timeInSeconds) {
            Destroy(gameObject, timeInSeconds);
            if (flashCoroutine == null) {
                flashCoroutine = StartCoroutine(FlashOut(timeInSeconds));
            }
        }

        private IEnumerator FlashOut(float timeInSeconds) {
            if (renderer == null) yield break;

            float flashDuration = Mathf.Min(timeInSeconds * 0.3f, 3f);
            float timeBeforeFlashing = timeInSeconds - flashDuration;
            
            yield return new WaitForSeconds(timeBeforeFlashing);

            float elapsedTime = 0f;
            while (elapsedTime < flashDuration) {
                float progress = elapsedTime / flashDuration;
                float interval = Mathf.Lerp(0.25f, 0.05f, progress);
                
                renderer.enabled = !renderer.enabled;
                yield return new WaitForSeconds(interval);
                elapsedTime += interval;
            }
        }
    }
}