using System;
using System.Linq;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Game {
    public class GameSessionManager : MonoBehaviour {
        private IPlayerService playerService;
        private UnityEngine.InputSystem.PlayerInputManager unityInputManager;
        private bool isQuitting = false;
    
        private void Awake() {
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            unityInputManager = GetComponent<UnityEngine.InputSystem.PlayerInputManager>();

            if (playerService == null) {
                DebugLogger.Log(LogChannel.Systems, "No PlayerService found", LogLevel.Error);
                enabled = false;
                return;
            }
            
            if (unityInputManager == null) {
                DebugLogger.Log(LogChannel.Systems, "No PlayerInputManager found", LogLevel.Error);
                enabled = false;
                return;
            }

            ConfigureInputManager();
        }

        private void ConfigureInputManager() {
            unityInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
            unityInputManager.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        }

        private void OnEnable() {
            unityInputManager.onPlayerJoined += HandlePlayerJoined;
            unityInputManager.onPlayerLeft += HandlePlayerLeft;
            SceneManager.sceneLoaded += SetUIModuleOnSceneLoaded;
        }
        
        private void OnDisable() {
            unityInputManager.onPlayerJoined -= HandlePlayerJoined;
            unityInputManager.onPlayerLeft -= HandlePlayerLeft;
            SceneManager.sceneLoaded -= SetUIModuleOnSceneLoaded;
        }
        
        public void HandlePlayerJoined(PlayerInput playerInput) {
            Debug.Log($"[GameSessionManager] Unity PlayerInput detected: {playerInput.playerIndex}");
            bool playerJoined = playerService.TryJoinPlayer(playerInput);
            if (!playerJoined) {
                Debug.LogWarning("[GameSessionManager] Failed to join player");
            }

            if (playerJoined && KeyboardOrMouseIsConnected(playerInput)) {
                var uiModule = GetInputSystemUIInputModule();
                if (uiModule != null) {
                    playerInput.uiInputModule = uiModule;
                }
            }
        }

        private static bool KeyboardOrMouseIsConnected(PlayerInput playerInput) {
            return playerInput.devices.Any(device => device is Keyboard || device is Mouse);
        }

        public void HandlePlayerLeft(PlayerInput playerInput) {
            if (isQuitting) return;
            Debug.Log($"[GameSessionManager] Unity PlayerInput left: {playerInput.playerIndex}");
            
            var uiModule = GetInputSystemUIInputModule();
            if (uiModule != null && playerInput.uiInputModule == uiModule && KeyboardOrMouseIsConnected(playerInput)) {
                playerInput.uiInputModule = null;
            }
            
            RemovePlayerFromSlot(playerInput);
        }

        public void SetUIModuleOnSceneLoaded(Scene scene, LoadSceneMode mode) {
            var uiModule = GetInputSystemUIInputModule();
            if (uiModule == null) return;

            foreach (var playerSlot in playerService.PlayerSlots) {
                if (playerSlot.IsOccupied && playerSlot.PlayerInput != null && KeyboardOrMouseIsConnected(playerSlot.PlayerInput)) {
                    playerSlot.PlayerInput.uiInputModule = uiModule;
                }
            }
        }

        private static InputSystemUIInputModule GetInputSystemUIInputModule() {
            var uiModule = FindObjectsByType<InputSystemUIInputModule>(FindObjectsSortMode.None).FirstOrDefault();
            return uiModule;
        }

        private void RemovePlayerFromSlot(PlayerInput playerInput) {
            for (int i = 0; i < playerService.PlayerSlots.Count; i++) {
                var slot = playerService.PlayerSlots[i];
                if (slot.PlayerInput == playerInput) {
                    playerService.RemovePlayer(i);
                    break;
                }
            }
        }

        private void OnApplicationQuit() {
            isQuitting = true;
        }
    }
}