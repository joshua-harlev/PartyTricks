using UnityEngine;

namespace Game {
    [CreateAssetMenu(fileName = "Minigame Payout Config", menuName = "Scriptable Objects/Minigame Payout Config")]
    public class MinigamePayoutConfigSO : ScriptableObject {
        public int[] BaseFundsPerRank = new int[4];
    }
}
