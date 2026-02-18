using System.Collections;
using FMOD.Studio;
using FMODUnity;
using Services;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Shop {
    public class Shop : MonoBehaviour {
        [SerializeField] private ShopItemUI[] ShopItemUIElements;
        [SerializeField] private PlayerCornerDisplay[] PlayerCornerDisplays;
        [SerializeField] public ShopItemsDisplay ShopItemDisplay;
        [SerializeField] public float LockSpeedUpMultiplier = 3.0f;
        [SerializeField] private CountdownTimer CountdownTimer;
        [SerializeField] private ShopFeedback ShopFeedback; 
        [SerializeField] private EventReference MusicEvent;
        private ShopPlayerManager playerManager;
        private ShopNavigationService shopNavigationService;
        private ShopPurchaseService shopPurchaseService;
        private IGameFlowService gameFlowService;
        private IPauseService pauseService;
        private EventInstance musicInstance;
        public int GridRows = 2;
        public int GridColumns = 2;
        public int ShopDurationInSeconds = 10;

        private void Awake() {
            gameFlowService = GlobalServiceLocator.Instance.GetService<IGameFlowService>();
            pauseService = GlobalServiceLocator.Instance.GetService<IPauseService>();
            if (pauseService != null) {
                pauseService.OnPause += OnPause;
                pauseService.OnUnpause += OnUnpause;
            }
            musicInstance = RuntimeManager.CreateInstance(MusicEvent);
        }

        private void OnPause() => playerManager?.DisableAllSelectors();
        private void OnUnpause() => playerManager?.EnableAllSelectors();

        private void Start() {
            InitializeComponents();
            StartShop();
        }

        private void InitializeComponents() {
            ShopItemDisplay.SetShopItemUIElements(ShopItemUIElements);
            ShopItemDisplay.SetUpItems();
            shopPurchaseService = new ShopPurchaseService();
            shopNavigationService = new ShopNavigationService(GridRows, GridColumns);
            playerManager = new ShopPlayerManager(shopNavigationService, ShopItemUIElements, PlayerCornerDisplays);
            playerManager.OnLockCountChanged += AdjustSpeedByLockCount;
            CountdownTimer.OnTimerEnd += OnShopTimerEnd;
        }

        private void StartShop() {
            musicInstance.start();
            playerManager.InitializePlayers();
            CountdownTimer.StartTimer(ShopDurationInSeconds);
        }

        private void OnShopTimerEnd() {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
            ShopFeedback?.OnTimerEnd();
            shopPurchaseService.ResolvePurchases(playerManager.GetSelectors(), ShopItemUIElements);
            if (gameFlowService != null) {
                StartCoroutine(WaitAndThenMoveToNextMinigame());
            }
            else {
                Debug.LogError($"Shop: GameFlowManager is missing!");
            }
        }

        private IEnumerator WaitAndThenMoveToNextMinigame() {
            int numberOfSecondsToWait = 5;
            yield return new WaitForSeconds(numberOfSecondsToWait);
            gameFlowService.OnShopEnd();
        }

        public void Reset() {
            CountdownTimer.Reset();
            ShopFeedback.Reset();
            CountdownTimer.StartTimer(ShopDurationInSeconds);
            playerManager.EnableAllSelectors();
        }

        public void UnlockAISelectors() {
            playerManager.UnlockAISelectors();
        }


        private void OnDestroy() {
            musicInstance.release();
            CountdownTimer.OnTimerEnd -= OnShopTimerEnd;
            playerManager.OnLockCountChanged -= AdjustSpeedByLockCount;
            playerManager?.Cleanup();
            if (pauseService != null) {
                pauseService.OnPause -= OnPause;
                pauseService.OnUnpause -= OnUnpause;
            }
        }

        private void AdjustSpeedByLockCount(int lockedCount, int lockedAICount, int humanCount) {
            int numberOfLockedHumans = lockedCount - lockedAICount;
            bool allHumansAreLocked = (numberOfLockedHumans == humanCount);
            if (allHumansAreLocked) {
                CountdownTimer.SetSpeedMultiplier(LockSpeedUpMultiplier);
            }
            else {
                CountdownTimer.ResetSpeed();
            }
            ShopFeedback?.UpdateFeedback((lockedCount == 4), lockedCount);
        }
    }
}
