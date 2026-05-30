using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TitleScreen {
    public class ButtonScaleEffect : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {
        private static ButtonScaleEffect activeButton;
    
        private Vector3 originalScale;
        private Tween activeTween;

        [RuntimeInitializeOnLoadMethod]
        private static void ResetActiveButton() {
            activeButton = null;
        }

        private void Awake() {
            originalScale = transform.localScale;
        }

        private void ScaleTo(float multiplier, float duration) {
            activeTween?.Kill();
            activeTween = transform.DOScale(originalScale * multiplier, duration).SetUpdate(true);
        }

        private void Activate() {
            if (activeButton == this) return;
            activeButton?.ScaleDown();
            activeButton = this;
            ScaleTo(1.2f, 0.3f);
        }
    
        private void Deactivate() {
            if (activeButton != this) return;
            activeButton = null;
            ScaleDown();
        }

        private void ScaleDown() {
            ScaleTo(1f, 0.1f);
        }

        public void OnSelect(BaseEventData eventData) {
            Activate();
        }

        public void OnDeselect(BaseEventData eventData) {
            Deactivate();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            Activate();
        }

        public void OnPointerExit(PointerEventData eventData) {
            var selectedGO = EventSystem.current?.currentSelectedGameObject;
            if (selectedGO && selectedGO.TryGetComponent<ButtonScaleEffect>(out var selectedEffect) &&
                selectedEffect != this) {
                selectedEffect.Activate();
                return;
            }
            Deactivate();
        }

        private void OnDestroy() {
            if(activeButton == this) activeButton = null;
            activeTween?.Kill();
        }
    }
}
