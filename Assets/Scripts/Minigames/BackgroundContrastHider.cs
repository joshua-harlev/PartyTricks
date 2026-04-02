using UnityEngine;

namespace Minigames {
    // Use to hide background objects for increased contrast
    public class BackgroundContrastHider : MonoBehaviour {
        [SerializeField] private Renderer backgroundRenderer;
        private bool shouldHide;
        private void Awake() {
            GameSettings.OnApplySettings += UpdateHide;
            if (backgroundRenderer == null) {
                backgroundRenderer = GetComponent<Renderer>();
            }
            UpdateHide();
        }

        private void UpdateHide() {
            shouldHide = GameSettings.Accessibility.IncreaseBackgroundVisibility;
            backgroundRenderer.enabled = !shouldHide;
        }

        private void OnDestroy() {
            GameSettings.OnApplySettings -= UpdateHide;
        }
    }
}