using Minigames.Swinging.Core;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace Minigames.Swinging {
    public class VineTrackView : MonoBehaviour {
        [FormerlySerializedAs("vineAnchorPrefab")] [SerializeField] private GameObject VineAnchorPrefab;
        private VineView[] vineViews;
        
        [SerializeField] [Tooltip("Make phases slightly more random")]
        private bool JitterOn = true;

        public (float[] positions, float[] phaseOffsets, float[] periods) SpawnVines(int count, float spacing, float anchorY, SwingConfig config, Random randomNumberGenerator, float countdownDuration = 0f) {
            float[] xPositions = new float[count];
            float[] phaseOffsets = new float[count];
            float[] periods = new float[count];
            vineViews = new VineView[count];

            float radiansPerSecond = 2f * Mathf.PI / config.Period;
            float topLeftSwingPhase = 3f * Mathf.PI / 2f;
            
            for (int i = 0; i < count; i++) {
                xPositions[i] = i * spacing;
                
                if (i == 0) {
                    phaseOffsets[i] = topLeftSwingPhase - radiansPerSecond * countdownDuration;
                }
                else {
                    var (releaseVelocityX, _) = SwingSimulation.GetShapedReleaseVelocity(
                        SweetSpot.IdealReleasePhase, config.Amplitude, config.Period,
                        config.LaunchForce, config.RopeLength, config.ReleaseCurveExponent);
                    float flightTimeToNextVine = config.VineSpacing / releaseVelocityX;
                    float phaseAdvanceDuringFlight = radiansPerSecond * flightTimeToNextVine;
                    float jitter = JitterOn ? (float)((randomNumberGenerator.NextDouble() - 0.5) * 0.8) : 0f;
                    phaseOffsets[i] = phaseOffsets[i - 1] - phaseAdvanceDuringFlight + (config.PhaseChainOffset) + jitter;
                }
                periods[i] = config.Period;
                
                var anchor = Instantiate(VineAnchorPrefab, transform);
                anchor.transform.localPosition = new Vector3(xPositions[i], anchorY);
                
                var vineView = anchor.GetComponent<VineView>();
                vineViews[i] = vineView;
                vineView.Initialize(config.Amplitude, config.RopeLength, periods[i], phaseOffsets[i]);
            }

            return (xPositions, phaseOffsets, periods);
        }

        public void UpdateElapsedTime(float elapsedTime) {
            foreach (var vineView in vineViews) {
                vineView.SetElapsedTime(elapsedTime);
            }
        }

        public void SetSweetSpotHintLevel(int activeVineIndex, float hintLevel) {
            for (int i = 0; i < vineViews.Length; i++) {
                float level = 0f;
                if (i == activeVineIndex) level = hintLevel;
                vineViews[i].SetSweetSpotHintLevel(level);
            }
        }
    }
}