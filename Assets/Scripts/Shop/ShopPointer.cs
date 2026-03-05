using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShopPointer : MonoBehaviour {
    [SerializeField] private Sprite lockedIcon;
    [SerializeField] private Sprite pointerIcon;
    [Tooltip("Icon when player can't afford an item")]
    [SerializeField] private Image imageComponent;
    
    [Header("Sprite Shake")]
    [SerializeField] private Sprite NoIcon;
    [SerializeField] private float shakeDurationInSeconds = 0.3f;
    [SerializeField] private float shakeStrength = 10f;
    [SerializeField] private int shakeVibrato = 6;
    
    private bool isLocked = false;

    public void SetLocked() {
        if (isLocked) return;
        isLocked = true;
        imageComponent.sprite = lockedIcon;
    }

    public void SetUnlocked() {
        if (!isLocked) return;
        isLocked = false;
        imageComponent.sprite = pointerIcon;
    }

    public void PlayCannotAffordFeedback() {
        imageComponent.sprite = NoIcon;
        
        var rectTransform = imageComponent.rectTransform;
        rectTransform.DOShakeAnchorPos(shakeDurationInSeconds, new Vector2(shakeStrength, 0), shakeVibrato, 0)
            .OnComplete(() =>
            {
                if(isLocked) imageComponent.sprite = lockedIcon;
                else imageComponent.sprite = pointerIcon;
            });
    }
    
    public void SetCannotAffordPermanent() {
        imageComponent.sprite = NoIcon;
    }
}
