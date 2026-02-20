using System;
using System.Linq;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

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
        }
        
        private void OnDisable() {
            unityInputManager.onPlayerJoined -= HandlePlayerJoined;
            unityInputManager.onPlayerLeft -= HandlePlayerLeft;
        }
        
        public void HandlePlayerJoined(PlayerInput playerInput) {
            Debug.Log($"[GameSessionManager] Unity PlayerInput detected: {playerInput.playerIndex}");
            bool playerJoined = playerService.TryJoinPlayer(playerInput);
            if (!playerJoined) {
                Debug.LogWarning("[GameSessionManager] Failed to join player");
            }

            if (playerJoined && KeyboardOrMouseIsConnected(playerInput)) {
                var uiModule = FindObjectsByType<InputSystemUIInputModule>(FindObjectsSortMode.None).FirstOrDefault();
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

            if (KeyboardOrMouseIsConnected(playerInput)) {
                var uiModule = FindObjectsByType<InputSystemUIInputModule>(FindObjectsSortMode.None).FirstOrDefault();
                if (uiModule != null && playerInput.uiInputModule == uiModule) {
                    playerInput.uiInputModule = null;
                }
            }
            
            RemovePlayerFromSlot(playerInput);
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