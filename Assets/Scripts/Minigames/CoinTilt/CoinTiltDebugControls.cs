using Game;
using Minigames.Debug;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Minigames.CoinTilt {
    public static class CoinTiltDebugControls {
        private static readonly MinigameSceneDebugLoader debugLoader = new("Load Coin Tilt Minigame", "CoinTiltGame");
        private static LightingVariant cachedLightingVariant;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init() {
            DebugMenu.RegisterSceneLoadButton(order: 20, debugLoader.DrawLoadButton);
            DebugMenu.RegisterActiveSceneControl(order: 20, DrawLightingToggle);
        }
        
        private static void DrawLightingToggle() {
            if (SceneManager.GetActiveScene().name != "CoinTiltGame") {
                return;
            }
            
            if (GUILayout.Button("Toggle Lighting", GUILayout.Height(30))) {
                if (cachedLightingVariant == null) {
                    cachedLightingVariant = Object.FindAnyObjectByType<LightingVariant>();
                }
                cachedLightingVariant?.ToggleLighting();
            }
            
            GUILayout.Space(10);
        }
    }
}
