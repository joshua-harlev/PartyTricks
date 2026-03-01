using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging {
    public class VineTrackView : MonoBehaviour {
        [SerializeField] private GameObject vineAnchorPrefab;
        private VineView[] vineViews;

        public (float[] positions, float[] phaseOffsets, float[] periods) SpawnVines(int count, float spacing, float anchorY, SwingConfig config, float periodVariation, System.Random randomNumberGenerator) {
            float[] xPositions = new float[count];
            float[] phaseOffsets = new float[count];
            float[] periods = new float[count];
            vineViews = new VineView[count];
            
            for (int i = 0; i < count; i++) {
                xPositions[i] = i * spacing;
                phaseOffsets[i] = (float)(randomNumberGenerator.NextDouble() * 2f * Mathf.PI);
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