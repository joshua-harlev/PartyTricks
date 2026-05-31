using System.Collections;
using CoreData;
using DG.Tweening;
using Options;
using UnityEngine;

namespace Minigames.CoinTilt {
    public class Coin : MonoBehaviour {
        [SerializeField] private CoinTypeSO coinType;
        [SerializeField] private ParticleSystem pullTrailParticles;
        [SerializeField] private Renderer coinMeshRenderer;
        [SerializeField] private Transform modelTransform;
        [SerializeField] private CoinAnimationSO coinAnimationConfig;
        
        public bool IsSpecialCoin => coinType.IsSpecialCoin;
        
        private float spawnHeight;
        private int pointValue;
        private bool hasBeenCollected;
        private Transform pullTarget;
        private bool isBeingPulled;
        private float pullSpeed;
        private Coroutine flashCoroutine;
        private Vector3 initialModelEulerAngles;
        private float bobPhase;
        private float spinPhase;
        private float maxHeight;
        private float jitter;
        private int spinSign = 1;

        public void StartPull(Transform target, float speed) {
            if (!isBeingPulled) {
                isBeingPulled = true;
                pullTarget = target;
                pullSpeed = speed;
                pullTrailParticles?.Play();
            }
        }

        private void Update() {
            AnimateCoinMovement();
            if (isBeingPulled && pullTarget != null) {
                MoveTowardsPullTarget();
                EnsureSpawnHeight();
            }
        }

        private void MoveTowardsPullTarget() {
            Vector3 direction = pullTarget.position - transform.position;
            transform.position += direction * pullSpeed * Time.deltaTime;
        }

        private void AnimateCoinMovement() {
            float bobHeight = Mathf.Sin(Time.time * 2f + bobPhase) * coinAnimationConfig.BobMultiplier;
            modelTransform.localPosition = new Vector3(0f, bobHeight + maxHeight/2f, 0f);
            modelTransform.localRotation = Quaternion.Euler(initialModelEulerAngles.x, Time.time *
                (coinAnimationConfig.SpinMultiplier * spinSign * jitter) + spinPhase, initialModelEulerAngles.z);
        }

        private void EnsureSpawnHeight() {
            Vector3 localPosition = transform.localPosition;
            localPosition.y = spawnHeight;
            transform.localPosition = localPosition;
        }

        private void Awake() {
            InitializeCoinValue();
            AnimateCoinScale();
            RandomizePhases();
            
            jitter = Random.Range(0.8f, 1.2f);
            if(GameSettings.Misc.RandomizeCoinSpinDirection) RandomizeSpinDirection();
            maxHeight = coinAnimationConfig.BobMultiplier;
            initialModelEulerAngles = modelTransform.localEulerAngles;
        }

        private void AnimateCoinScale() {
            Vector3 idealScale = this.modelTransform.localScale;
            modelTransform.localScale = Vector3.zero;
            modelTransform.DOScale(idealScale, coinAnimationConfig.AnimateInTimeInSeconds);
        }

        private void InitializeCoinValue() {
            if (coinType != null) {
                pointValue = coinType.PointValue;
            }
            else {
                UnityEngine.Debug.LogError("No coin type specified");
            }
        }

        private void RandomizePhases() {
            bobPhase = Random.Range(0f, Mathf.PI * coinAnimationConfig.BobFrequency);
            spinPhase = Random.Range(0f, 360f);
        }

        private void RandomizeSpinDirection() {
            if (Random.value >= 0.5f) {
                spinSign = 1;
            } else spinSign = -1;
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