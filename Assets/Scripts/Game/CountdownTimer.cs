using System;
using System.Collections;
using UnityEngine;

namespace Game {
    public class CountdownTimer : MonoBehaviour {
        private float countdownDurationInSeconds;
        private float timeRemaining;
        private float speedMultiplier = 1f;
        private const float MinimumSpeedMultiplier = 0.1f;
        public Action OnTimerEnd;
        public Action<int> OnTick;
        public Action OnReset;

        public void StartTimer(float ShopDurationInSeconds) {
            this.countdownDurationInSeconds = ShopDurationInSeconds;
            StartCoroutine(Countdown());
        }
    
        private IEnumerator Countdown() {
            timeRemaining = Mathf.Ceil(countdownDurationInSeconds);
            while (timeRemaining > 0) {
                OnTick?.Invoke((int)timeRemaining);
                yield return new WaitForSeconds(1f/ (speedMultiplier * DebugMenu.DebugTimerSpeedUpMultiplier));
                timeRemaining--;
            }

            OnTimerEnd?.Invoke();
        }

        public void Reset() {
            StopAllCoroutines();
            OnReset?.Invoke();
        }

        public void SetSpeedMultiplier(float newMultiplier) {
            speedMultiplier = Math.Max(newMultiplier, MinimumSpeedMultiplier);
        }

        public void ResetSpeed() {
            speedMultiplier = 1f;
        }
    }
}
