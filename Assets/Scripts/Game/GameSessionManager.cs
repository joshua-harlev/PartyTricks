using System.Linq;
using Debug;
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
        [SerializeField] private InputActionAsset PointerActionAsset;
        private InputActionReference pointReference, clickReference, rightClickReference, middleClickReference, scrollWheelReference;
    
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
            var joinAction = new InputAction(binding: "<Gamepad>/*", type: InputActionType.Button);
            joinAction.AddBinding("<Keyboard>/anyKey");
            joinAction.Enable();
            unityInputManager.joinAction = new InputActionProperty(joinAction);
        }

        private void OnEnable() {
            unityInputManager.onPlayerJoined += HandlePlayerJoined;
            unityInputManager.onPlayerLeft += HandlePlayerLeft;
            SceneManager.sceneLoaded += SetUIModuleOnSceneLoaded;
            SceneManager.sceneLoaded += SceneObserver.OnSceneLoaded;
        }
        
        private void OnDisable() {
            unityInputManager.onPlayerJoined -= HandlePlayerJoined;
            unityInputManager.onPlayerLeft -= HandlePlayerLeft;
            SceneManager.sceneLoaded -= SetUIModuleOnSceneLoaded;
            SceneManager.sceneLoaded -= SceneObserver.OnSceneLoaded;
            PointerActionAsset?.Disable();
            PointerActionAsset = null;
        }
        
        public void HandlePlayerJoined(PlayerInput playerInput) {
            UnityEngine.Debug.Log($"[GameSessionManager] Unity PlayerInput detected: {playerInput.playerIndex}");
            bool playerJoined = playerService.TryJoinPlayer(playerInput);
            if (!playerJoined) {
                UnityEngine.Debug.LogWarning("[GameSessionManager] Failed to join player");
            }

            ConfigureUIModuleAsPointerOnly();
        }
        
        private void ConfigureUIModuleAsPointerOnly() {
            var uiModule = GetInputSystemUIInputModule();
            if (uiModule == null) return;

            uiModule.move = null;
            uiModule.submit = null;
            uiModule.cancel = null;

            if (pointReference == null) {
                PointerActionAsset.Enable();
                var map = PointerActionAsset.FindActionMap("Pointer");
                pointReference = InputActionReference.Create(map.FindAction("Point"));
                clickReference = InputActionReference.Create(map.FindAction("Click"));
                rightClickReference = InputActionReference.Create(map.FindAction("RightClick"));
                middleClickReference = InputActionReference.Create(map.FindAction("MiddleClick"));
                scrollWheelReference = InputActionReference.Create(map.FindAction("ScrollWheel"));
            }

            uiModule.point = pointReference;
            uiModule.leftClick = clickReference;
            uiModule.rightClick = rightClickReference;
            uiModule.middleClick = middleClickReference;
            uiModule.scrollWheel = scrollWheelReference;
        }

        public void HandlePlayerLeft(PlayerInput playerInput) {
            if (isQuitting) return;
            UnityEngine.Debug.Log($"[GameSessionManager] Unity PlayerInput left: {playerInput.playerIndex}");
            RemovePlayerFromSlot(playerInput);
        }

        public void SetUIModuleOnSceneLoaded(Scene scene, LoadSceneMode mode) {
            ConfigureUIModuleAsPointerOnly();
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