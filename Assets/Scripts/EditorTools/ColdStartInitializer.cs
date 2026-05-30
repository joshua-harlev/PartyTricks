// Lets you initialize a game without running from the main menu

using Debug;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
namespace EditorTools {
    public static class ColdStartInitializer {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureSessionState() {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == "MainMenu") return;

            var gameFlowManager = ServiceLocatorAccessor.GetService<IGameFlowService>() as GameFlowManager;
            if (gameFlowManager == null || gameFlowManager.HasActiveSession) return;

            var config = Resources.Load<GameBootstrapConfig>(GameBootstrapConfig.ResourcePath);
            if (config == null) {
                DebugLogger.Log(LogChannel.Systems, $"Cold Start Initializer: no MinigameType mapped for scene '{activeScene.name}'", LogLevel.Warning);
                return;
            }

            MinigameType minigameType = config.MinigameConfig.GetTypeForMinigame(activeScene.name);
            
            DebugLogger.Log(LogChannel.Systems, $"Cold Start Initializer: stubbing for '{activeScene.name}' as {minigameType}");
            gameFlowManager.StartGameForColdStart(minigameType, activeScene.name == "Shop", activeScene.name == "Results");
        }
    }
}
#endif