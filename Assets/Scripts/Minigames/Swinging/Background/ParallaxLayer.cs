using Options;
using UnityEngine;

namespace Minigames.Swinging.Background {
    public class ParallaxLayer : MonoBehaviour {
        [SerializeField] private Transform cameraTransform;
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactor = 0.5f;

        private float startCamX;
        private float startLocalX;
        private bool wasDisabled;

        private void Start() {
            startCamX = cameraTransform.localPosition.x;
            startLocalX = transform.localPosition.x;
        }

        private void LateUpdate() {
            if (GameSettings.Accessibility.DisableParallax) {
                wasDisabled = true;
                return;
            }
            if (cameraTransform == null) return;

            if (wasDisabled) {
                startCamX = cameraTransform.localPosition.x;
                startLocalX = transform.localPosition.x;
                wasDisabled = false;
            }
            
            float camDelta = cameraTransform.localPosition.x - startCamX;
            transform.localPosition = new Vector3(
                startLocalX + camDelta * parallaxFactor,
                transform.localPosition.y,
                transform.localPosition.z
            );
        }
    }
}
