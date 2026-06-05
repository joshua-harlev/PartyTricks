using Game;
using UnityEngine;

namespace Minigames.Blackjack {
    public static class BlackjackDebugControls {
        private static readonly MinigameSceneDebugLoader debugLoader = new("Load Blackjack", "Blackjack");
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init() {
            DebugMenu.RegisterSceneLoadButton(order: 10, debugLoader.DrawLoadButton);
        }
    }
}