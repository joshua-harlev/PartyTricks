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
                periods[i] = config.Period * (1f-periodVariation + (float)(randomNumberGenerator.NextDouble() * 2f * periodVariation));
                
                var anchor = Instantiate(vineAnchorPrefab, transform);
                anchor.transform.localPosition = new Vector3(xPositions[i], anchorY);
                
                var vineView = anchor.GetComponent<VineView>();
                vineView.Initialize(config.Amplitude, config.RopeLength, periods[i], phaseOffsets[i]);
                vineViews[i] = vineView;
            }

            return (xPositions, phaseOffsets, periods);
        }

        public void UpdateElapsedTime(float elapsedTime) {
            for (int i = 0; i < vineViews.Length; i++) {
                vineViews[i].SetElapsedTime(elapsedTime);
            }
        }
        
        public VineView GetVineView(int index) {
            return vineViews[index];
        }
    }
}