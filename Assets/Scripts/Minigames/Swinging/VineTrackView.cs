using Minigames.Swinging.Core;
using UnityEngine;

namespace Minigames.Swinging {
    public class VineTrackView : MonoBehaviour {
        [SerializeField] private GameObject vineAnchorPrefab;
        private VineView[] vineViews;
        
        // change this to enable/disable jitter (makes phases slightly more random)
        private const bool JitterOn = true;

        public (float[] positions, float[] phaseOffsets, float[] periods) SpawnVines(int count, float spacing, float anchorY, SwingConfig config, System.Random randomNumberGenerator, float countdownDuration = 0f) {
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
                    float idealReleasePhase = 0.55f;
                    var (releaseVelocityX, _) = SwingSimulation.GetShapedReleaseVelocity(
                        idealReleasePhase, config.Amplitude, config.Period,
                        config.LaunchForce, config.RopeLength, 0.6f);
                    float flightTimeToNextVine = config.VineSpacing / releaseVelocityX;
                    float phaseAdvanceDuringFlight = radiansPerSecond * flightTimeToNextVine;
                    float jitter = JitterOn ? (float)((randomNumberGenerator.NextDouble() - 0.5) * 0.8) : 0f;
                    phaseOffsets[i] = phaseOffsets[i - 1] - phaseAdvanceDuringFlight + (config.PhaseChainOffset) + jitter;
                }
                periods[i] = config.Period;
                
                var anchor = Instantiate(vineAnchorPrefab, transform);
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
    }
}