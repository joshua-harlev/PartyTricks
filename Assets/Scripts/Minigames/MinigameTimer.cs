using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MinigameTimer : MonoBehaviour {
    [SerializeField] private GameObject TimerPanel;
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private CanvasGroup TimerCanvasGroup;
    public event Action OnTimerEnd;
    public event Action<float> OnHalfwayPointReached;
    
    // Elapsed time, Remaining Time
    public event Action<float, float> OnTimerTick;
    private string endOfGameText;
    private float RemainingTimeInSeconds { get; set; }
    private float originalTimerDuration;
    private bool halfwayPointEventTriggered;
    private bool isPaused;
    private Coroutine timerCoroutine = null;

    public void Initialize(float gameLengthInSeconds, string endOfGameText = "Game!") {
        RemainingTimeInSeconds = Mathf.Ceil(gameLengthInSeconds);
        originalTimerDuration = RemainingTimeInSeconds;
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
            DebugLogger.Log(LogChannel.Systems, "Game timer ticked: " + RemainingTimeInSeconds  + " seconds remaining.");
            yield return new WaitForSeconds(1f);
            RemainingTimeInSeconds--;
        }
        OnTimeUp();
        OnTimerEnd?.Invoke();
        timerCoroutine = null;
    }

    private void OnTimeUp() {
        TimerText.text = "Time!";
    }

    private void OnTick(float remainingTimeInSeconds) {
        OnTimerTick?.Invoke(originalTimerDuration-remainingTimeInSeconds, remainingTimeInSeconds);
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
