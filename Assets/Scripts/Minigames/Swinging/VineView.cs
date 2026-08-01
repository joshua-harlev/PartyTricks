using System;
using Minigames.Swinging.Core;
using UnityEngine;

namespace Minigames.Swinging {
    public class VineView : MonoBehaviour {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Boolean DoDebug = false;

        private float amplitude;
        private float ropeLength;
        private float period;
        private float phaseOffset;
        private float elapsedTime;
        private float smoothedCurveOffset;
        private RopeSimulation ropeSimulation;
        private const int RopePointCount = 6;
        private const float DriveStiffness = 0.5f;
        private const float Tautness = 0.3f;
        private const float CurveAmount = 0.6f;
        // lower value = more lag on the rope.
        private const float CurveResponse = 1f;
        
        private float sweetSpotHintLevel = 0f;

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
        
        public void SetSweetSpotHintLevel(float level) {
            this.sweetSpotHintLevel = level;
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

            float glowAmount = InSweetSpot(currentPhase) ? sweetSpotHintLevel : 0f;
            Color vineColor = Color.Lerp(Color.white, Color.yellow, glowAmount);
            lineRenderer.startColor = vineColor;
            lineRenderer.endColor = vineColor;
            if(DoDebug && glowAmount > 0f) UnityEngine.Debug.Log($"{Mathf.Sin(currentPhase)} {Mathf.Cos(currentPhase)}");
        }
        
        private bool InSweetSpot(float currentPhase) {
            return Mathf.Sin(currentPhase) > 0.25f && Mathf.Cos(currentPhase) > 0.3f && Mathf.Sin(currentPhase) < 0.67f;
        }
    }
}