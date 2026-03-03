using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using ResultsScreen;
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
        [SerializeField] private ShopStockConfig ShopStockConfig;
        [SerializeField] private PlacesScreenPanel PlacesScreenPanel;
        [SerializeField] private CanvasGroup ShopCanvasGroup;
        [SerializeField] private float FadeInTimeInSeconds = 0.5f;
        private ShopPlayerManager playerManager;
        private ShopNavigationService shopNavigationService;
        private ShopPurchaseService shopPurchaseService;
        private IGameFlowService gameFlowService;
        private IPauseService pauseService;
        private EventInstance musicInstance;
        private Sequence currentSequence;
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
            bool shouldShowPlacesScreen = gameFlowService.ShouldShowPlacesScreen();
            if (shouldShowPlacesScreen && PlacesScreenPanel != null) {
                int[] previousFunds = gameFlowService.GetPreviousRoundFunds();
                HideShop();
                PlacesScreenPanel.OnDismissed += StartShop;
                PlacesScreenPanel.ShowPlaces(previousFunds);
            } else {
                StartShop();
            }
        }

        private void HideShop() {
            ShopCanvasGroup.alpha = 0;
            ShopCanvasGroup.interactable = false;
            ShopCanvasGroup.blocksRaycasts = false;
        }

        private void ShowShop() {
            currentSequence = DOTween.Sequence();
            currentSequence.Append(ShopCanvasGroup.DOFade(1f, FadeInTimeInSeconds).SetEase(Ease.Linear));
            currentSequence.OnComplete(() =>
            {
                ShopCanvasGroup.alpha = 1;
                ShopCanvasGroup.interactable = true;
                ShopCanvasGroup.blocksRaycasts = true;
            });
        }

        private void InitializeComponents() {
            ShopItemDisplay.SetShopItemUIElements(ShopItemUIElements);

            int roundIndex = gameFlowService.GetCompletedMinigameList().Count;
            ShopStock stock = null;
            if (ShopStockConfig != null && roundIndex < ShopStockConfig.StocksOrderedByRound.Length) {
                stock = ShopStockConfig.StocksOrderedByRound[roundIndex];
            }
            
            ShopItemDisplay.SetUpItems(stock);
            shopPurchaseService = new ShopPurchaseService();
            shopNavigationService = new ShopNavigationService(GridRows, GridColumns);
            playerManager = new ShopPlayerManager(shopNavigationService, ShopItemUIElements, PlayerCornerDisplays);
            playerManager.OnLockCountChanged += AdjustSpeedByLockCount;
            CountdownTimer.OnTimerEnd += OnShopTimerEnd;
        }

        private void StartShop() {
            ShowShop();
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
            musicInstance.stop(STOP_MODE.IMMEDIATE);
            musicInstance.start();
        }

        public void UnlockAISelectors() {
            playerManager.UnlockAISelectors();
        }


        private void OnDestroy() {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
            musicInstance.release();
            CountdownTimer.OnTimerEnd -= OnShopTimerEnd;
            playerManager.OnLockCountChanged -= AdjustSpeedByLockCount;
            playerManager?.Cleanup();
            if (pauseService != null) {
                pauseService.OnPause -= OnPause;
                pauseService.OnUnpause -= OnUnpause;
            }

            if (PlacesScreenPanel != null) {
                PlacesScreenPanel.OnDismissed -= StartShop;
            }
            currentSequence?.Kill();
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
