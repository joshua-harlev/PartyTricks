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
        private float smoothedCurveOffset;
        private RopeSimulation ropeSimulation;
        private const int RopePointCount = 6;
        private const float DriveStiffness = 1.0f;
        private const float Tautness = 0.3f;
        private const float CurveAmount = 0.6f;
        // lower value = more lag on the rope.
        private const float CurveResponse = 1f;

        public void Initialize(float amplitude, float ropeLength, float period, float phaseOffset) {
            this.amplitude = amplitude;
            this.ropeLength = ropeLength;
            this.period = period;
            this.phaseOffset = phaseOffset;
            ropeSimulation = new RopeSimulation(RopePointCount, ropeLength);
            lineRenderer.positionCount = RopePointCount;
        }

        public void SetElapsedTime(float time) {
            elapsedTime = time;
        }

        private void Update() {
            float currentPhase = phaseOffset + (2f * Mathf.PI / period) * elapsedTime;
            var (offsetX, offsetY) = SwingSimulation.GetSwingPosition(currentPhase, amplitude, ropeLength);
            float angularVelocity = amplitude * Mathf.Cos(currentPhase) * (2f * Mathf.PI / period);
            float targetCurveOffset = -angularVelocity * CurveAmount;
            smoothedCurveOffset = Mathf.Lerp(smoothedCurveOffset, targetCurveOffset, 1f-Mathf.Exp(-CurveResponse * Time.deltaTime));
            
            ropeSimulation.SetDriveTarget(new Vec2(offsetX, offsetY), DriveStiffness);
            ropeSimulation.Simulate(Time.deltaTime);
            ropeSimulation.Constrain();
            ropeSimulation.ApplyTautness(Tautness, smoothedCurveOffset);
            
            Vector3 anchorPosition = transform.position;
            Vec2[] ropePoints = ropeSimulation.GetPositions();
            for (int i = 0; i < RopePointCount; i++) {
                lineRenderer.SetPosition(i, anchorPosition + new Vector3(ropePoints[i].X, ropePoints[i].Y, 0));
            }
        }
    }
}