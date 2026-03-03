using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using ResultsScreen.Core;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ResultsScreen {
    public class PlacesScreenPanel : MonoBehaviour {
        [Header("Views")] [SerializeField] private PlacesBarView[] barViews;
        [SerializeField] private PlacesSidebarView sidebarView;

        [Header("Panel")] [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRoot;

        [Header("Player Visuals")] [SerializeField]
        private Color[] playerColors;

        [SerializeField] private Sprite[] playerIcons;

        [Header("Animation Config")] [SerializeField]
        private float maxBarHeight = 686f;

        [SerializeField] private float minBarHeight = 60f;
        [SerializeField] private float barGrowDurationInSeconds = 0.5f;
        [SerializeField] private float barStaggerDurationInSeconds = 0.15f;
        [SerializeField] private float holdDurationInSeconds = 3.5f;
        [SerializeField] private float exitDurationInSeconds = 0.4f;
        [SerializeField] private float positionSwapDurationInSeconds = 0.4f;

        public event Action OnDismissed;

        private IPlayerService playerService;
        private PlacesAnimationState animationState = new();
        private readonly List<InputAction> subscribedActions = new();
        private bool canDismissEarly;
        private Sequence currentSequence;
        private float[] slotXPositions;
        private int[] barToPlayerMap;

        private void Awake() {
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            slotXPositions = new float[barViews.Length];
            for (int i = 0; i < barViews.Length; i++) {
                slotXPositions[i] = barViews[i].GetComponent<RectTransform>().anchoredPosition.x;
            }
        }

        public void ShowStatic(int[] funds) {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            
            PlacesEntry[] entries = PlacesCalculator.CalculatePlaces(funds);
            float[] barHeights = BarLayoutCalculator.ComputeBarHeights(entries, maxBarHeight, minBarHeight);

            if (barToPlayerMap == null) {
                barToPlayerMap = new int[barViews.Length];
                InitializeBarsFromEntries(entries);
            }

            for (int i = 0; i < entries.Length; i++) {
                barViews[i].SetBarHeight(barHeights[i]);
            }
            
            sidebarView.UpdateLabels(entries);
        }

        public void ShowPlaces(int[] previousFunds) {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canDismissEarly = false;

            int[] currentFunds = new int[4];
            for (int i = 0; i < 4; i++) {
                currentFunds[i] = playerService.GetPlayerProfile(i).Wallet.GetCurrentFunds();
            }

            PlacesEntry[] entries = PlacesCalculator.CalculatePlaces(currentFunds);
            float[] barHeights = BarLayoutCalculator.ComputeBarHeights(entries, maxBarHeight, minBarHeight);

            sidebarView.UpdateLabels(entries);

            if (barToPlayerMap == null) {
                barToPlayerMap = new int[barViews.Length];

                if (previousFunds != null) {
                    PlacesEntry[] prevEntries = PlacesCalculator.CalculatePlaces(previousFunds);
                    InitializeBarsFromEntries(prevEntries);
                } else {
                    InitializeBarsFromEntries(entries);
                }
            } else {
                for (int i = 0; i < barViews.Length; i++) {
                    int playerIndex = barToPlayerMap[i];
                    PlacesEntry? playerEntry = FindEntryForPlayer(entries, playerIndex);
                    barViews[i].SetFundsText(playerEntry.GetValueOrDefault().Funds + " Funds");
                }
            }

            if (previousFunds == null) {
                AnimateFirstTime(barHeights);
            }
            else {
                float[] previousHeights;
                if (animationState.HasPreviousState) {
                    previousHeights = animationState.PreviousHeights;
                }
                else {
                    previousHeights =
                        BarLayoutCalculator.ComputeBarHeights(PlacesCalculator.CalculatePlaces(previousFunds),
                            maxBarHeight, minBarHeight);
                }

                AnimateTransition(previousHeights, barHeights, entries);
            }

            animationState.Record(entries, barHeights);
            SubscribeToPlayerSubmitActions();
        }

        private PlacesEntry? FindEntryForPlayer(PlacesEntry[] entries, int playerIndex) {
            for (int j = 0; j < entries.Length; j++) {
                if (entries[j].PlayerIndex == playerIndex) {
                    return entries[j];
                }
            }

            return null;
        }

        private void InitializeBarsFromEntries(PlacesEntry[] sourceEntries) {
            for (int i = 0; i < sourceEntries.Length; i++) {
                barToPlayerMap[i] = sourceEntries[i].PlayerIndex;
                int playerIndex = sourceEntries[i].PlayerIndex;
                barViews[i].SetColor(playerColors[playerIndex]);
                barViews[i].SetCharacterIcon(playerIcons[playerIndex]);
                barViews[i].SetCrownVisibility(sourceEntries[i].Rank == 0);
                barViews[i].SetFundsText(sourceEntries[i].Funds + " Funds");
            }
        }

        private void SubscribeToPlayerSubmitActions() {
            foreach (var playerSlot in playerService.PlayerSlots) {
                PlayerInput playerInput = playerSlot.PlayerInput;
                if (playerInput == null) continue;
                InputAction action = playerInput.actions.FindAction("UI/Submit");
                if (action == null) continue;
                action.performed += OnSubmitPerformed;
                subscribedActions.Add(action);
            }
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context) {
            if (!canDismissEarly) return;
            canDismissEarly = false;
            currentSequence?.Kill();
            PlayExitAnimation();
        }

        private void PlayExitAnimation() {
            canDismissEarly = false;
            currentSequence = DOTween.Sequence();
            currentSequence.Append(panelRoot.DOAnchorPosX(-Screen.width, exitDurationInSeconds).SetEase(Ease.InCubic));
            currentSequence.Join(canvasGroup.DOFade(0f, exitDurationInSeconds));
            currentSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                OnDismissed?.Invoke();
            });
        }

        private void AnimateTransition(float[] previousHeights, float[] newHeights, PlacesEntry[] entries) {
            foreach (var barView in barViews) {
                barView.SetCrownVisibility(false);
            }
            for (int i = 0; i < barViews.Length; i++) {
                barViews[i].SetBarHeight(previousHeights[i]);
            }

            currentSequence = DOTween.Sequence();

            for (int barIndex = 0; barIndex < barViews.Length; barIndex++) {
                int playerIndex = barToPlayerMap[barIndex];
                float targetHeight = 0;
                for (int j = 0; j < entries.Length; j++) {
                    if(entries[j].PlayerIndex == playerIndex) {
                        targetHeight = newHeights[j];
                        break;
                    }
                }
                int capturedBarIndex = barIndex;
                float capturedTargetHeight = targetHeight;
                currentSequence.Join(
                    CreateBarHeightTween(capturedBarIndex, capturedTargetHeight)
                );
            }

            bool firstSwap = true;
            for (int targetSlot = 0; targetSlot < entries.Length; targetSlot++) {
                int targetPlayer = entries[targetSlot].PlayerIndex;
                int barIndex = FindBarForPlayer(targetPlayer);
                if (barIndex >= 0 && barIndex != targetSlot) {
                    int index = barIndex;
                    var tween = barViews[index].GetComponent<RectTransform>()
                        .DOAnchorPosX(slotXPositions[targetSlot],
                            positionSwapDurationInSeconds)
                        .SetEase(Ease.InOutCubic);
                    if (firstSwap) { 
                        currentSequence.Append(tween); 
                        firstSwap = false; 
                    } else {
                        currentSequence.Join(tween);
                    }
                }
            }

            currentSequence.AppendCallback(() =>
            {
                for (int j = 0; j < entries.Length; j++) {
                    int playerIndex = entries[j].PlayerIndex;
                    int bar = FindBarForPlayer(playerIndex);
                    if (bar >= 0) {
                        barViews[bar].SetFundsText(entries[j].Funds + " Funds");
                        barViews[bar].SetCrownVisibility(entries[j].Rank == 0);
                    }
                }
                
                for (int i = 0; i < entries.Length; i++) {
                    barToPlayerMap[i] = entries[i].PlayerIndex;
                }
            });
            
            FinalizeAnimationSequence();
        }

        private TweenerCore<float, float, FloatOptions> CreateBarHeightTween(int barIndex, float targetHeight) {
            return DOTween.To(
                () => barViews[barIndex].GetBarHeight(),
                height => barViews[barIndex].SetBarHeight(height),
                targetHeight,
                barGrowDurationInSeconds
            ).SetEase(Ease.OutCubic);
        }

        private int FindBarForPlayer(int playerIndex) {
            for (int i = 0; i < barToPlayerMap.Length; i++) {
                if (barToPlayerMap[i] == playerIndex) return i;
            }

            return -1;
        }

        private void AnimateFirstTime(float[] barHeights) {
            foreach (var barView in barViews) {
                barView.SetBarHeight(0f);
            }

            currentSequence = DOTween.Sequence();
            for (int i = barViews.Length - 1; i >= 0; i--) {
                float targetHeight = barHeights[i];
                int capturedBarIndex = i;
                currentSequence.Append(
                    CreateBarHeightTween(capturedBarIndex, targetHeight)
                );
                if (i > 0) currentSequence.AppendInterval(barStaggerDurationInSeconds);
            }

            FinalizeAnimationSequence();
        }

        private void FinalizeAnimationSequence() {
            currentSequence.AppendCallback(() => canDismissEarly = true);
            currentSequence.AppendInterval(holdDurationInSeconds);
            currentSequence.AppendCallback(PlayExitAnimation);
        }

        private void OnDestroy() {
            currentSequence?.Kill();
            foreach (var action in subscribedActions) {
                action.performed -= OnSubmitPerformed;
            }
        }
    }
}