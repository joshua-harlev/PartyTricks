using Game;
using TMPro;
using UnityEngine;

namespace Shop {
    public class ShopTimerDisplay : MonoBehaviour
    {
        [SerializeField] private CountdownTimer CountdownTimer;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text allReadyText;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private GameObject timerTextGameObject;
        [SerializeField] private Color allReadyColor = Color.red;

        private void Awake() {
            CountdownTimer.OnTick += UpdateTimerText;
            CountdownTimer.OnTimerEnd += OnTimerEnd;
            CountdownTimer.OnReset += Reset;
            HideAllReady();
        }

        private void Reset() {
            labelText.alignment = TextAlignmentOptions.Right;
            timerTextGameObject.SetActive(true);
            labelText.text = "Time Remaining: ";
            HideAllReady();
        }
    
        private void OnDestroy() {
            CountdownTimer.OnTick -= UpdateTimerText;
            CountdownTimer.OnTimerEnd -= OnTimerEnd;
            CountdownTimer.OnReset -= Reset;
        }

        private void UpdateTimerText(int timeRemaining) {
            timerText.text = timeRemaining + " seconds";
        }

        private void OnTimerEnd() {
            labelText.text = "Time's up!";
            labelText.alignment = TextAlignmentOptions.Center;
            timerTextGameObject.SetActive(false);
            HideAllReady();
        }

        public void ShowAllReady() {
            if (allReadyText == null) return;
            allReadyText.color = allReadyColor;
            allReadyText.gameObject.SetActive(true);
        }

        public void HideAllReady() {
            if(allReadyText != null) allReadyText.gameObject.SetActive(false);
        }
    }
}
