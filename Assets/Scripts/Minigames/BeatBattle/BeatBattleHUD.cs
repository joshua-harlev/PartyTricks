using TMPro;
using UnityEngine;

namespace Minigames.BeatBattle {
    public class BeatBattleHUD : MonoBehaviour {
        [SerializeField] private TMP_Text[] scoreTexts;
        [SerializeField] private TMP_Text[] statusLabels;
        [SerializeField] private TMP_Text roundIndicator;

        public void UpdateScore(int playerIndex, int score) {
            scoreTexts[playerIndex].text = score.ToString();
        }
        
        public void SetStatus(int playerIndex, string text) {
            statusLabels[playerIndex].text = text;
        }

        public void SetRound(int roundIndex) {
            roundIndicator.text = $"Round {roundIndex + 1}/4";
        }

        public void ClearAllStatusLabels() {
            for (int i = 0; i < 4; i++) {
                statusLabels[i].text = "";
            }
        }
    }
}