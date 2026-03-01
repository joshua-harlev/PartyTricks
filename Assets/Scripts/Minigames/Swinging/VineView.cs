using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging {
    public class VineView : MonoBehaviour {
        [SerializeField] private LineRenderer lineRenderer;

        private float amplitude;
        private float ropeLength;
        private float period;
        private float phaseOffset;
        private float elapsedTime;

        public void Initialize(float amplitude, float ropeLength, float period, float phaseOffset) {
            this.amplitude = amplitude;
            this.ropeLength = ropeLength;
            this.period = period;
            this.phaseOffset = phaseOffset;
            lineRenderer.positionCount = 2;
        }

        public void SetElapsedTime(float time) {
            elapsedTime = time;
        }

        private void Update() {
            float currentPhase = phaseOffset + (2f * Mathf.PI / period) * elapsedTime;

            var (offsetX, offsetY) = SwingSimulation.GetSwingPosition(currentPhase, amplitude, ropeLength);
            Vector3 anchorPosition = transform.position;
            Vector3 endPosition = anchorPosition + new Vector3(offsetX, offsetY, 0);
            
            lineRenderer.SetPosition(0, anchorPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
    }
}