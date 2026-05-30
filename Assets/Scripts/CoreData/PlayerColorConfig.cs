using UnityEngine;

namespace CoreData {
    [CreateAssetMenu(fileName = "PlayerColorConfig", menuName = "Scriptable Objects/Player Color Config")]
    public class PlayerColorConfig : ScriptableObject {
        [SerializeField] private PlayerColorSet[] playerColors = new PlayerColorSet[4];
        
        public PlayerColorSet GetPlayerColors(int playerIndex) => playerColors[playerIndex];
        public Color GetMainColor(int playerIndex) => playerColors[playerIndex].MainColor;
        public Color GetLightColor(int playerIndex) => playerColors[playerIndex].LightColor;
        public Color GetEffectColor(int playerIndex) => playerColors[playerIndex].EffectColor;
    }
}