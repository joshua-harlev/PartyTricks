using System.Linq;
using Debug;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Minigames.Debug {
    public class MinigameSceneDebugLoader {
        private readonly string buttonLabel;
		private readonly string sceneName;
		private bool pendingDoubleRound;

		public MinigameSceneDebugLoader(string buttonLabel, string sceneName) {
			this.buttonLabel = buttonLabel;
			this.sceneName = sceneName;
		}

		public void DrawLoadButton(bool isDoubleRound) {
			if (GUILayout.Button(buttonLabel, GUILayout.Height(40))) {
				pendingDoubleRound = isDoubleRound;
                
				DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Loading {sceneName}. Double: {isDoubleRound}");
				SceneManager.LoadScene(sceneName);
				SceneManager.sceneLoaded -= OnSceneLoaded;
				SceneManager.sceneLoaded += OnSceneLoaded;
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			if (scene.name != sceneName) return;
        
			SceneManager.sceneLoaded -= OnSceneLoaded;
        
			IMinigameManager minigameManager = GameObject.FindGameObjectsWithTag("MinigameManager").FirstOrDefault()?.GetComponent<IMinigameManager>();
			if (minigameManager != null) {
				minigameManager.Initialize(pendingDoubleRound);
				DebugLogger.Log(LogChannel.Systems, $"Debug Menu: {sceneName} minigame manager initialized. Double: {pendingDoubleRound}");
			} else {
				UnityEngine.Debug.LogError($"Debug Menu: Could not find IMinigameManager in {sceneName}!");
			}
		}
    }
}