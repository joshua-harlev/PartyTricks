using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {
    public class MinigameDebugPanel {
        private int maximizedPlayerIndex = -1;
        private bool cornerDisplaysHidden;
        private bool timerHidden;
        private bool timerPaused;
        private Camera[] cachedPlayerCameras;
        private PlayerCornerDisplay[] cachedCornerDisplays;
        private MinigameTimer cachedTimer;
        private Rect[] originalViewpointRects;

        private static readonly HashSet<string> SplitScreenScenes = new()
        {
            "VineSwinging",
            "CoinTiltGame"
        };

        private static readonly HashSet<string> MinigameScenes = new()
        {
            "VineSwinging",
            "CoinTiltGame",
            "DireDodging",
            "Blackjack"
        };

        public void OnSceneLoaded() {
            Reset();
        }

        private void Reset() {
            if (cachedPlayerCameras != null && originalViewpointRects != null) {
                RestoreCameras();
            }
            maximizedPlayerIndex = -1;
            cornerDisplaysHidden = false;
            timerHidden = false;
            timerPaused = false;
            cachedPlayerCameras = null;
            originalViewpointRects = null;
            cachedCornerDisplays = null;
            cachedTimer = null;
        }

        public void Draw() {
            string sceneName = SceneManager.GetActiveScene().name;
            if (!MinigameScenes.Contains(sceneName)) return;
            bool hasSplitScreen = SplitScreenScenes.Contains(sceneName);

            GUILayout.Space(20);
            GUILayout.Label("Minigame Debug", GUI.skin.box);
            GUILayout.Space(10);

            if (hasSplitScreen) {
                DrawCameraSection();
            }

            DrawUIToggles();
        }
        
        private void DrawCameraSection() {
            GUILayout.Label("Camera");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 4; i++) {
                string label;
                if (maximizedPlayerIndex == i) {
                    label = $"[P{i + 1}]";
                } else {
                    label = $"P{i + 1}";
                }

                if (GUILayout.Button(label, GUILayout.Height(30))) {
                    MaximizeCamera(i);
                }
            }
            
            GUILayout.EndHorizontal();
            if (maximizedPlayerIndex >= 0 && GUILayout.Button("Reset Camera", GUILayout.Height(30))) {
                RestoreCameras();
                maximizedPlayerIndex = -1;
            }
            
            GUILayout.Space(10);
        }

        private void MaximizeCamera(int playerIndex) {
            EnsureCamerasCached();
            if (cachedPlayerCameras == null) return;

            maximizedPlayerIndex = playerIndex;
            for (int i = 0; i < cachedPlayerCameras.Length; i++) {
                if (cachedPlayerCameras[i] == null) continue;
                if (i == playerIndex) {
                    cachedPlayerCameras[i].rect = new Rect(0, 0, 1, 1);
                    cachedPlayerCameras[i].enabled = true;
                } else {
                    cachedPlayerCameras[i].enabled = false;
                }
            }
        }
        
        private void RestoreCameras() {
            if (cachedPlayerCameras == null || originalViewpointRects == null) return;
            for (int i = 0; i < cachedPlayerCameras.Length; i++) {
                if (cachedPlayerCameras[i] == null) continue;
                cachedPlayerCameras[i].rect = originalViewpointRects[i];
                cachedPlayerCameras[i].enabled = true;
            }
        }

        private void EnsureCamerasCached() {
            if (cachedPlayerCameras != null) return;

            var cameraObjects = GameObject.FindGameObjectsWithTag("PlayerCamera");
            if (cameraObjects.Length == 0) return;

            var cameras = GetCameraComponents(cameraObjects);
            SortCamerasByPosition(cameras);
            
            cachedPlayerCameras = cameras.ToArray();
            originalViewpointRects = new Rect[cameras.Count];
            for (int i = 0; i < cameraObjects.Length; i++) {
                originalViewpointRects[i] = cameras[i].rect;
            }
        }

        private static List<Camera> GetCameraComponents(GameObject[] cameraObjects) {
            var cameras = new List<Camera>();
            foreach (var cameraObject in cameraObjects) {
                var camera = cameraObject.GetComponent<Camera>();
                if(camera != null) cameras.Add(camera);
            }

            return cameras;
        }

        private static void SortCamerasByPosition(List<Camera> cameras) {
            cameras.Sort((camera1, camera2) =>
            {
                int rowCompare = camera2.rect.y.CompareTo(camera1.rect.y);
                if (rowCompare != 0) return rowCompare;
                return camera1.rect.x.CompareTo(camera2.rect.x);
            });
        }

        private void DrawUIToggles() {
            DrawCornerDisplayToggle();
            DrawTimerVisibilityToggle();
            DrawTimerPauseStateToggle();
        }

        private void DrawTimerPauseStateToggle() {
            bool newTimerPausedState = GUILayout.Toggle(timerPaused, "Timer Paused");
            if (newTimerPausedState != timerPaused) {
                timerPaused = newTimerPausedState;
                cachedTimer ??= Object.FindAnyObjectByType<MinigameTimer>();
                if (cachedTimer == null) return;
                if (timerPaused) cachedTimer.Pause();
                else cachedTimer.Resume();
            }
        }

        private void DrawTimerVisibilityToggle() {
            bool newTimerHiddenState = GUILayout.Toggle(timerHidden, "Timer Hidden");
            if (newTimerHiddenState != timerHidden) {
                timerHidden = newTimerHiddenState;
                cachedTimer ??= Object.FindAnyObjectByType<MinigameTimer>();
                if (cachedTimer != null) {
                    cachedTimer.SetVisible(!timerHidden);
                }
            }
        }

        private void DrawCornerDisplayToggle() {
            bool newCornerHiddenState = GUILayout.Toggle(cornerDisplaysHidden, "Corners Hidden");
            if (newCornerHiddenState != cornerDisplaysHidden) {
                cornerDisplaysHidden = newCornerHiddenState;
                cachedCornerDisplays ??= Object.FindObjectsByType<PlayerCornerDisplay>(FindObjectsSortMode.None);
                foreach (var display in cachedCornerDisplays) {
                    display.gameObject.SetActive(!cornerDisplaysHidden);
                }
            }
        }
    }
}