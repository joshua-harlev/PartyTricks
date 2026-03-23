using UnityEngine;

namespace Minigames.Swinging {
    public class ParallaxLayer : MonoBehaviour {
        [SerializeField] private Transform cameraTransform;
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactor = 0.5f;

        private float startCamX;
        private float startLocalX;

        private void Start() {
            startCamX = cameraTransform.localPosition.x;
            startLocalX = transform.localPosition.x;
        }

        private void LateUpdate() {
            if (cameraTransform == null) return;
            float camDelta = cameraTransform.localPosition.x - startCamX;
            transform.localPosition = new Vector3(
                startLocalX + camDelta * parallaxFactor,
                transform.localPosition.y,
                transform.localPosition.z
            );
        }
    }
}
