using System;
using System.Collections.Generic;
using Debug;
using Options;
using Player;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Game {
    public class DebugMenu : MonoBehaviour {
        private static DebugMenu instance;
        private readonly MinigameDebugPanel minigamePanel = new();
        private Vector2 scrollPosition;
        private bool shouldShowMenu = false;
        private InputAction toggleDebugMenuAction;
        private Rect windowRect;
        private bool isDoubleRound = false;
        private IPlayerService playerService;
        public static Action<IPlayerService> PowerupPanelDraw;
        private static readonly List<(int order, Action<bool> draw)> sceneLoadButtons = new();
        private static readonly List<(int order, Action draw)> activeSceneControls = new();
        
        public static float DebugTimerSpeedUpMultiplier = 1f;
    
        private void Awake() {
            windowRect = new Rect(20, 20, 400, 600);
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            toggleDebugMenuAction = InputSystem.actions.FindAction("UI/ToggleDebugMenu");
        
            if (toggleDebugMenuAction == null) {
                UnityEngine.Debug.LogWarning("DebugMenu: ToggleDebugMenu action not found. Debug menu will not be toggleable.");
            }
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            minigamePanel.OnSceneLoaded();
        }

        private void Update() {
            if (toggleDebugMenuAction != null && toggleDebugMenuAction.WasPressedThisFrame()) {
                shouldShowMenu = !shouldShowMenu;
            }

            var keyboard = Keyboard.current;
        
            DebugTimerSpeedUpMultiplier = keyboard[Key.Equals].isPressed ? 3f : 1f;
            if (keyboard[Key.T].wasPressedThisFrame) {
                GameSettings.Accessibility.DisableParallax = !GameSettings.Accessibility.DisableParallax;
            }

            if (keyboard[Key.B].wasPressedThisFrame) {
                GameSettings.Accessibility.IncreaseBackgroundVisibility = !GameSettings.Accessibility.IncreaseBackgroundVisibility;
                GameSettings.Apply();
            }
        }

        private void OnGUI() {
            if (!shouldShowMenu) return;
            windowRect = GUI.Window(0, windowRect, DrawDebugWindow, "Debug Menu");
        }

        private void DrawDebugWindow(int windowID) {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();

            GUILayout.Label("Scene Testing", GUI.skin.box);
            GUILayout.Space(10);
            
            isDoubleRound = GUILayout.Toggle(isDoubleRound, "Double Round");
            
            foreach(var (_, draw) in sceneLoadButtons) {
                draw(isDoubleRound);
                GUILayout.Space(10);
            }
        
            if (GUILayout.Button("Load Shop", GUILayout.Height(40))) {
                SceneManager.LoadScene("Shop");
            }

            GUILayout.Space(10);
        
            if (GUILayout.Button("Load Main Menu", GUILayout.Height(40))) {
                SceneManager.LoadScene("MainMenu");
            }
        
            GUILayout.Space(10);
        
            if (GUILayout.Button("Load Results Screen", GUILayout.Height(40))) {
                SceneManager.LoadScene("Results");
            }

            GUILayout.Space(10);
            
            foreach (var (_, draw) in activeSceneControls) {
                draw();
            }
            
            GUILayout.Space(20);

            GUILayout.Label("Game State", GUI.skin.box);
            GUILayout.Space(10);
        
            GUILayout.Label($"Current Scene: {SceneManager.GetActiveScene().name}");
            DisplayPlayerFunds();

            GUILayout.Space(20);

            GUILayout.Label("Utilities", GUI.skin.box);
            GUILayout.Space(10);
        
            if (GUILayout.Button("Add 100 to All Players", GUILayout.Height(30))) {
                AddFundsToAllPlayers(100);
            }

            if (GUILayout.Button("Remove 100 from All Players", GUILayout.Height(30))) {
                RemoveFundsFromAllPlayers(100);
            }
        
            GUILayout.Space(10);
        
            if (GUILayout.Button("Randomize all player funds", GUILayout.Height(30))) {
                RandomizeAllPlayerFunds();
            }

            GUILayout.Space(10);
        
            if (GUILayout.Button("Reset All Player Funds", GUILayout.Height(30))) {
                ResetAllPlayerFunds();
            }

            minigamePanel.Draw();
            PowerupPanelDraw?.Invoke(playerService);
        
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        private void RandomizeAllPlayerFunds() {
            if (playerService == null) {
                UnityEngine.Debug.LogWarning("Debug Menu: PlayerService not found.");
                return;
            }

            foreach (var slot in playerService.PlayerSlots) {
                if (slot?.Profile != null) {
                    Wallet wallet = slot.Profile.Wallet;
                    int currentFunds = wallet.GetCurrentFunds();
                    if(currentFunds > 0) wallet.RemoveFunds(currentFunds);
                    wallet.AddFunds(Random.Range(1, 100)*10);
                }
            }
        }

        private void DisplayPlayerFunds() {
            if (playerService != null) {
                GUILayout.Space(5);
                for (int i = 0; i < playerService.GetPlayerCount(); i++) {
                    var slot = playerService.PlayerSlots[i];
                    if (slot?.Profile != null) {
                        int funds = slot.Profile.Wallet.GetCurrentFunds();
                        string aiLabel = slot.IsAI ? " (AI)" : "";
                        GUILayout.Label($"P{i + 1}: {funds} coins{aiLabel}");
                    }
                }
            }
        }

        private void AddFundsToAllPlayers(int amount) {
            if (playerService == null) {
                UnityEngine.Debug.LogWarning("Debug Menu: PlayerService not found.");
                return;
            }

            foreach (var slot in playerService.PlayerSlots) {
                if (slot?.Profile != null) {
                    slot.Profile.Wallet.AddFunds(amount);
                }
            }
        
            DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Added {amount} funds to all players.");
        }
    
        private void RemoveFundsFromAllPlayers(int amount) {
            if (playerService == null) {
                UnityEngine.Debug.LogWarning("Debug Menu: PlayerService not found.");
                return;
            }

            foreach (var slot in playerService.PlayerSlots) {
                if (slot?.Profile != null) {
                    slot.Profile.Wallet.RemoveFunds(amount);
                }
            }
        
            DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Added {amount} funds to all players.");
        }

        private void ResetAllPlayerFunds() {
            if (playerService == null) {
                UnityEngine.Debug.LogWarning("Debug Menu: PlayerService not found.");
                return;
            }

            foreach (var slot in playerService.PlayerSlots) {
                if (slot?.Profile != null) {
                    slot.Profile.Wallet.Reset();
                }
            }
        
            DebugLogger.Log(LogChannel.Systems, "Debug Menu: Reset all player funds.");
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public static void RegisterSceneLoadButton(int order, Action<bool> draw) {
            sceneLoadButtons.Add((order, draw));
            sceneLoadButtons.Sort((a, b) => a.order.CompareTo(b.order));
        }

        public static void RegisterActiveSceneControl(int order, Action draw) {
            activeSceneControls.Add((order, draw));
            activeSceneControls.Sort((a, b) => a.order.CompareTo(b.order));
        }
    }
}