using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Input {
    public class PlayerSlot : MonoBehaviour {
        [Header("Slot Configuration")]
        [SerializeField] private int slotIndex;
        [SerializeField] private Color playerColor = Color.white;

        private IDirectionalTwoButtonInputHandler inputHandler;
        private PlayerInput playerInput;
        private PlayerProfile profile;
        private bool isAI;
    
        public PlayerInput PlayerInput => playerInput;
        public int SlotIndex => slotIndex;
        public Color PlayerColor => playerColor;
        public PlayerProfile Profile => profile;
        public bool IsAI => isAI;
        public bool IsOccupied => inputHandlerProxy.IsOccupied;
        public IDirectionalTwoButtonInputHandler InputHandler => inputHandlerProxy;
        private InputHandlerProxy inputHandlerProxy;

        public void Initialize(int index) {
            slotIndex = index;
            SetUpAsAI();
        }

        public void SetUpAsAI() {
            ClearCurrentInput();

            playerInput = null;
        
            var aiGameObject = new GameObject($"AIInput_Player{slotIndex}");
            aiGameObject.transform.SetParent(transform);
            inputHandler = aiGameObject.AddComponent<AIShopInputHandler>();
            if (inputHandlerProxy == null) {
                inputHandlerProxy = new InputHandlerProxy(inputHandler);
            } else {
                inputHandlerProxy.SetTarget(inputHandler);
            }
            isAI = true;

            InitializePlayerProfileIfNull();
        }

        private void InitializePlayerProfileIfNull() {
            // TODO make 300 a const somewhere 
            if (profile == null) {
                profile = new PlayerProfile(300);
            }
        }

        public void SetUpAsHuman(PlayerInput playerInput) {
            ClearCurrentInput();

            this.playerInput = playerInput;
            playerInput.transform.SetParent(transform);
        
            var handler = playerInput.gameObject.GetComponent<PlayerUITwoButtonInputHandler>();
            if (handler == null) {
                handler = playerInput.gameObject.AddComponent<PlayerUITwoButtonInputHandler>();
                handler.Initialize(playerInput);
            } 
        
            inputHandler = handler;
        
            if (inputHandlerProxy == null) {
                inputHandlerProxy = new InputHandlerProxy(inputHandler);
            } else {
                inputHandlerProxy.SetTarget(inputHandler);
            }
        
            isAI = false;

            InitializePlayerProfileIfNull();
        }

        public void SetPlayerColor(Color color) {
            playerColor = color;
        }

        private void ClearCurrentInput() {
            if (inputHandler != null) {
                if (inputHandler is Component component) {
                    Destroy(component.gameObject);
                }
                inputHandler = null;
            }
        }


        public void AssignProfile(PlayerProfile newProfile) {
            profile = newProfile;
        }

        public void Reset() {
            SetUpAsAI();
            profile?.Reset();
        }

        public void SwapAIHandler<T>() where T : AIInputHandlerBase {
            if (!isAI) return;
            ClearCurrentInput();
            var aiGameObject = new GameObject($"AIInput_Player{slotIndex}");
            aiGameObject.transform.SetParent(transform);
            inputHandler = aiGameObject.AddComponent<T>();
            inputHandlerProxy.SetTarget(inputHandler);
        
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public T GetAIHandler<T>() where T : AIInputHandlerBase {
            return inputHandler as T;
        }

        private void OnSceneUnloaded(Scene scene) {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            if (isAI) {
                SetUpAsAI();
            }
        }

        private void OnDestroy() {
            ClearCurrentInput();
        }
    }
}
