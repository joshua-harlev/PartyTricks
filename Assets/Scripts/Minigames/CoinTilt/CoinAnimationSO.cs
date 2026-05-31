using UnityEngine;

namespace Minigames.CoinTilt {
    [CreateAssetMenu(fileName = "CoinAnimationConfig", menuName = "Scriptable Objects/Coin Animation Config")]
    public class CoinAnimationSO : ScriptableObject {
        [Range(0f, 1f)]
        public float BobMultiplier = 0.25f;

        [Range(0f, 500f)]
        public float SpinMultiplier = 30f;

        public float AnimateInTimeInSeconds = 0.3f;

        public float BobFrequency = 2f;
    }
}
