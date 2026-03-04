using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MinigameTimer : MonoBehaviour {
    [SerializeField] private GameObject TimerPanel;
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private CanvasGroup TimerCanvasGroup;
    public event Action OnTimerEnd;
    public event Action<int> OnHalfwayPointReached;
    private string endOfGameText;
    private int RemainingTimeInSeconds { get; set; }
    private int originalTimerDuration;
    private bool halfwayPointEventTriggered;
    private bool isPaused;
    private Coroutine timerCoroutine = null;

    public void Initialize(int gameLengthInSeconds, string endOfGameText = "Game!") {
        originalTimerDuration = gameLengthInSeconds;
        RemainingTimeInSeconds = gameLengthInSeconds;
        this.endOfGameText = endOfGameText;
        HidePanel();
    }

    public void OverrideText(string text) {
        TimerText.text = text;
    }

    private void ShowPanel() {
        TimerCanvasGroup.alpha = 1;
        TimerCanvasGroup.blocksRaycasts = true;
    }

    private void HidePanel() {
        TimerCanvasGroup.alpha = 0;
        TimerCanvasGroup.blocksRaycasts = false;
    }

    public void StartTimer() {
        ShowPanel();
        timerCoroutine = StartCoroutine(Timer());
    }

    private IEnumerator Timer() {
        while (RemainingTimeInSeconds > 0) {
            while(isPaused) yield return null;
            OnTick(RemainingTimeInSeconds);
            if (!halfwayPointEventTriggered && RemainingTimeInSeconds <= (originalTimerDuration / 2f)) {
                OnHalfwayPointReached?.Invoke(RemainingTimeInSeconds);
                halfwayPointEventTriggered = true;
            }
            RemainingTimeInSeconds--;
            DebugLogger.Log(LogChannel.Systems, "Game timer ticked: " + RemainingTimeInSeconds  + " seconds remaining.");
            yield return new WaitForSeconds(1f);
        }
        OnTimeUp();
        OnTimerEnd?.Invoke();
        timerCoroutine = null;
    }

    private void OnTimeUp() {
        TimerText.text = "Time!";
    }

    private void OnTick(int remainingTimeInSeconds) {
        TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTimeInSeconds);
        string timeInMinutes = timeSpan.Minutes.ToString("00");
        string timeInSeconds = timeSpan.Seconds.ToString("00");
        TimerText.text = timeInMinutes + ":" + timeInSeconds;
    }

    public void StopIfRunning() {
        if (timerCoroutine != null) {
            StopCoroutine(this.timerCoroutine);
            if (!string.IsNullOrEmpty(endOfGameText)) {
                TimerText.text = endOfGameText;
            }
            timerCoroutine = null;
        }
    }

    public void Resume() {
        isPaused = false;
    }

    public void Pause() {
        isPaused = true;
    }
    
    public void SetVisible(bool visible) {
        if (visible) ShowPanel();
        else HidePanel();
    }
}
