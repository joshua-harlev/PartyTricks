using Debug;
using Game;
using Minigames.Debug;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Minigames.DireDodging {
    public static class DireDodgingDebugControls {
        private static readonly MinigameSceneDebugLoader debugLoader = new("Load Dire Dodging", "DireDodging");
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init() {
            DebugMenu.RegisterSceneLoadButton(order: 30, debugLoader.DrawLoadButton);
            DebugMenu.RegisterActiveSceneControl(order: 30, DrawDebugControls);
        }
        
        private static void DrawDebugControls() {
            if (SceneManager.GetActiveScene().name != "DireDodging") {
                return;
            }
            
            if (DireDodgingMinigameManager.Instance != null) {
                GUILayout.Label("Dire Dodging Controls", GUI.skin.box);
                GUILayout.Space(10);
        
                if (GUILayout.Button("Kill Player 1", GUILayout.Height(30))) {
                    KillDireDodgingPlayer(0);
                }
        
                if (GUILayout.Button("Kill Player 2", GUILayout.Height(30))) {
                    KillDireDodgingPlayer(1);
                }
        
                if (GUILayout.Button("Kill Player 3", GUILayout.Height(30))) {
                    KillDireDodgingPlayer(2);
                }
        
                if (GUILayout.Button("Kill Player 4", GUILayout.Height(30))) {
                    KillDireDodgingPlayer(3);
                }
            
                GUILayout.Space(10);
            
                GUILayout.Label("Stun Player", GUI.skin.box);
                GUILayout.BeginHorizontal();
                for (int i = 0; i < 4; i++) {
                    if (GUILayout.Button($"Stun P{i+1}", GUILayout.Height(40))) {
                        DireDodgingMinigameManager.Instance.DebugStunPlayer(i);
                    }
                }
                GUILayout.EndHorizontal();
        
                GUILayout.Space(20);
            }
            
            GUILayout.Space(10);
        }
        
        private static void KillDireDodgingPlayer(int playerIndex) {
            if (DireDodgingMinigameManager.Instance == null) {
                UnityEngine.Debug.LogWarning("Debug Menu: Dire Dodging Minigame Manager could not be found in scene.");
                return;
            }
        
            DireDodgingMinigameManager.Instance.DebugKillPlayer(playerIndex);
            DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Killed Player {playerIndex + 1}");
        }
    }
}