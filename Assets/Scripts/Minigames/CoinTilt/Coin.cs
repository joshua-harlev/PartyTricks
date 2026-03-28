using System.Collections;
using UnityEngine;

namespace Minigames.CoinTilt {
    public class Coin : MonoBehaviour {
        [SerializeField] private CoinTypeSO coinType;
        [SerializeField] private ParticleSystem pullTrailParticles;
        [SerializeField] private Renderer coinMeshRenderer;
        [SerializeField] private Transform modelTransform;
        [SerializeField] private CoinAnimationSO coinAnimationConfig;
        private float spawnHeight;
        private int pointValue;
        private bool hasBeenCollected;
        private Transform pullTarget;
        private bool isBeingPulled;
        private float pullSpeed;
        public bool IsSpecialCoin => coinType.IsSpecialCoin;
        private Coroutine flashCoroutine;
        private Vector3 initialModelEulerAngles;
        private float bobPhase;
        private float spinPhase;
        private float maxHeight;
        private float jitter;
        private int spinSign = 1;

        private const bool randomizeSpinDirection = false;

        public void StartPull(Transform target, float speed) {
            if (!isBeingPulled) {
                isBeingPulled = true;
                pullTarget = target;
                pullSpeed = speed;
                pullTrailParticles?.Play();
            }
        }

        private void Update() {
            float bobHeight = Mathf.Sin(Time.time * 2f + bobPhase) * coinAnimationConfig.BobMultiplier;
            modelTransform.localPosition = new Vector3(0f, bobHeight + maxHeight/2f, 0f);
            modelTransform.localRotation = Quaternion.Euler(initialModelEulerAngles.x, Time.time *
                (coinAnimationConfig.SpinMultiplier * spinSign * jitter) + spinPhase, initialModelEulerAngles.z);
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

            StartCoroutine(SpawnIn());
            
            RandomizePhases();
            jitter = Random.Range(0.8f, 1.2f);
            if(randomizeSpinDirection) RandomizeSpinDirection();
            maxHeight = coinAnimationConfig.BobMultiplier;
            initialModelEulerAngles = modelTransform.localEulerAngles;
        }

        private void RandomizePhases() {
            bobPhase = Random.Range(0f, Mathf.PI * 2f);
            spinPhase = Random.Range(0f, 360f);
        }

        private void RandomizeSpinDirection() {
            if (Random.value >= 0.5f) {
                spinSign = 1;
            } else spinSign = -1;
        }

        private IEnumerator SpawnIn() {
            Vector3 idealScale = this.modelTransform.localScale;
            modelTransform.localScale = Vector3.zero;
            float currentTime = 0f;
            while (currentTime < coinAnimationConfig.AnimateInTimeInSeconds) {
                currentTime += Time.deltaTime;
                float t = currentTime / coinAnimationConfig.AnimateInTimeInSeconds;
                modelTransform.localScale = Vector3.Lerp(Vector3.zero, idealScale, t);
                yield return null;
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
            if (coinMeshRenderer == null) yield break;

            float flashDuration = Mathf.Min(timeInSeconds * 0.3f, 3f);
            float timeBeforeFlashing = timeInSeconds - flashDuration;
            
            yield return new WaitForSeconds(timeBeforeFlashing);

            float elapsedTime = 0f;
            while (elapsedTime < flashDuration) {
                float progress = elapsedTime / flashDuration;
                float interval = Mathf.Lerp(0.25f, 0.05f, progress);
                
                coinMeshRenderer.enabled = !coinMeshRenderer.enabled;
                yield return new WaitForSeconds(interval);
                elapsedTime += interval;
            }
        }
    }
}