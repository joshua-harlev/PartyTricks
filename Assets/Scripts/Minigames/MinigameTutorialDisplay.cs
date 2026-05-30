using System;
using System.Collections;
using Services;
using TMPro;
using UnityEngine;

public class MinigameTutorialDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private TMP_Text buttonPromptText;
    [SerializeField] private float displayDurationInSeconds = 7f;
    [SerializeField] private float blockSkipDurationInSeconds = 3f;
    private IPlayerService playerService;
    private IPauseService pauseService;
    public event Action OnDismissed;
    private bool displayIsActive;
    private bool canDismiss;

    private void Awake() {
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        pauseService = ServiceLocatorAccessor.GetService<IPauseService>();
        buttonPromptText.color = Color.black;
        pauseService.OnPause += OnPauseCallback;
        pauseService.OnUnpause += OnUnpauseCallback;
    }
    
    private void OnPauseCallback() => GetComponent<Canvas>().enabled = false;
    private void OnUnpauseCallback() => GetComponent<Canvas>().enabled = true;

    private void Update() {
        if (!canDismiss) return;
        foreach (var playerSlot in playerService.PlayerSlots) {
            if (playerSlot.IsAI) continue;
            if (playerSlot.InputHandler.SelectIsPressed() || playerSlot.InputHandler.CancelIsPressed()) {
                UnityEngine.Debug.Log($"Tutorial skip triggered by player {playerSlot.SlotIndex.ToString()}");
                Dismiss();
            }
        }
    }

    public void Show(string text) {
        tutorialText.text = text;
        displayIsActive = true;
        canDismiss = false;
        StartCoroutine(WaitToAllowInput());
        StartCoroutine(AutoDismissTimer());
    }

    private IEnumerator WaitToAllowInput() {
        yield return new WaitForSeconds(blockSkipDurationInSeconds);
        yield return null; // avoids stale values
        canDismiss = true;
        buttonPromptText.color = Color.white;
    }

    private IEnumerator AutoDismissTimer() {
        if (GameSettings.Gameplay.AutoDismissTutorials) {
            yield return new WaitForSeconds(displayDurationInSeconds);
            Dismiss();
        }
    }

    private void Dismiss() {
        if(!displayIsActive) return;
        displayIsActive = false;
        OnDismissed?.Invoke();
        Destroy(gameObject);
    }

    private void OnDestroy() {
        if (pauseService != null) {
            pauseService.OnPause -= OnPauseCallback;
            pauseService.OnUnpause -= OnUnpauseCallback;
        }
    }
}
