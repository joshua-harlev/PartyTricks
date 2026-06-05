using Game;
using Minigames.Debug;
using UnityEngine;

namespace Minigames.Swinging {
    public static class VineSwingingDebugControls {
        private static readonly MinigameSceneDebugLoader debugLoader = new("Load Vine Swinging", "VineSwinging");
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init() {
            DebugMenu.RegisterSceneLoadButton(order: 40, debugLoader.DrawLoadButton);
        }
    }
}