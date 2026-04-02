using Game;
using UnityEngine;

public class DisappearingPlayerLabel : MonoBehaviour {
    [Tooltip("Should the label automatically disappear?")]
    [SerializeField] private bool LabelShouldDisappear = false;
    [Tooltip("How long until the label automatically disappears, if the above is true?")]
    [SerializeField] private float TimeUntilAutomaticDestructionInSeconds = 10f;
    [SerializeField] private bool UseCountdownTimeForDestruction = false;
    [Tooltip("Only enable if player rotates, for performance reasons")]
    [SerializeField] private bool LockRotation = false;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private float heightOffset;
    private Quaternion baseRotation;

    private void Awake() {
        baseRotation = transform.rotation;
        heightOffset = transform.localPosition.y;
        UpdateLabelVisibility();
        GameSettings.OnApplySettings += UpdateLabelVisibility;
    }

    private void UpdateLabelVisibility() {
        if (spriteRenderer == null) return;
        spriteRenderer.enabled = GameSettings.Misc.ShowPlayerLabels;
    }

    public void Start() {
        if (UseCountdownTimeForDestruction && LabelShouldDisappear) {
            Destroy(gameObject, TimerLengths.GetCountdownTimerLengthInSeconds());
        } else if(LabelShouldDisappear) {
            Destroy(gameObject, TimeUntilAutomaticDestructionInSeconds);
        }
    }

    private void Update() {
        if (LockRotation) {
            transform.rotation = baseRotation;
            transform.position = transform.parent.position + Vector3.up * heightOffset;
        }
    }

    private void OnDestroy() {
        GameSettings.OnApplySettings -= UpdateLabelVisibility;
    }
}
