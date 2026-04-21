using System;
using System.Collections.Generic;
using System.Linq;
using Services;
using UnityEngine;

public class ShopPlayerManager {
    private ShopNavigationService navigationService;
    private ShopItemUI[] shopItemUIElements;
    private PlayerCornerDisplay[] playerCornerDisplays;
    private List<ShopSlotSelector> activeSelectors = new();
    private IPlayerService playerService;
    private IPowerUpService powerUpService;
    
    // locked count, locked AI count, total human count
    public event Action<int, int, int> OnLockCountChanged;
    public event Action OnLockRejected;
    public event Action OnLockAccepted;

    public ShopPlayerManager(ShopNavigationService navigationService, ShopItemUI[] shopItemUIElements, PlayerCornerDisplay[] playerCornerDisplays) {
        this.navigationService = navigationService;
        this.shopItemUIElements = shopItemUIElements;
        this.playerCornerDisplays = playerCornerDisplays;
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        powerUpService = ServiceLocatorAccessor.GetService<IPowerUpService>();
    }

    public void InitializePlayers() {
        InitializeCornerDisplays();
        CreatePlayerSelectors();
        SubscribeToSelectorEvents();
    }

    private void InitializeCornerDisplays() {
        for (int i = 0; i < playerCornerDisplays.Length; i++) {
            var profile = playerService.PlayerSlots[i].Profile;
            playerCornerDisplays[i].Initialize(profile);
        }
    }

    private void CreatePlayerSelectors() {
        activeSelectors.Clear();
        for (int i = 0; i < playerService.GetPlayerCount(); i++) {
            PlayerSlot slot = playerService.PlayerSlots[i];
            if (slot.InputHandler == null) continue;
            var selector = CreateSelector(i, slot);
            var modifiers = powerUpService.GetShopModifiers(slot.Profile);
            if (modifiers.ShopDiscountCount > 0) {
                foreach (var shopItemUIElement in shopItemUIElements) {
                    shopItemUIElement.SetPointerBonusStatus(i, true);
                }
            }
            activeSelectors.Add(selector);
        }
    }

    private ShopSlotSelector CreateSelector(int index, PlayerSlot slot) {
        var selectorObject = new GameObject($"Player_{index}_ShopSlotSelector");
        var selector = selectorObject.AddComponent<ShopSlotSelector>();
        selector.Initialize(index, slot.InputHandler, navigationService, slot.Profile, shopItemUIElements);
        return selector;
    }

    private void SubscribeToSelectorEvents() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged += HandleSelectionChanged;
            selector.OnLockChanged += HandleLockChanged;
            selector.OnLockRejected += HandleLockRejected;
            HandleSelectionChanged(selector, selector.CurrentShopItemIndex);
        }
    }
    
    private void HandleLockRejected(SelectionController controller) {
        ShopSlotSelector selector = (ShopSlotSelector)controller;
        shopItemUIElements[selector.CurrentShopItemIndex].OnCannotAfford(selector.PlayerIndex);
        OnLockRejected?.Invoke();
    }

    private void HandleLockChanged(SelectionController controller, bool locked) {
        ShopSlotSelector selector = (ShopSlotSelector)controller;
        shopItemUIElements[selector.CurrentShopItemIndex].OnPointedTo(selector.PlayerIndex, true, locked);
        OnLockCountChanged?.Invoke(GetLockedCount(), GetLockedAICount(), GetHumanCount());
        if (locked) {
            OnLockAccepted?.Invoke();
        }
    }

    private void HandleSelectionChanged(SelectionController selector, int newIndex) {
        for (int i = 0; i < shopItemUIElements.Length; i++) {
            bool isSelected = (i == newIndex);
            shopItemUIElements[i].OnPointedTo(selector.PlayerIndex, isSelected, selector.IsLocked);

            var discountInfo = GetDiscountInfoForPlayer(selector.PlayerIndex, shopItemUIElements[i].GetItemCost());
            shopItemUIElements[i].SetPlayerHoverDiscount(selector.PlayerIndex, isSelected, discountInfo);
        }
    }

    private PlayerDiscountInfo GetDiscountInfoForPlayer(int playerIndex, int baseItemCost) {
        var slot = playerService.PlayerSlots[playerIndex];
        var modifiers = powerUpService.GetShopModifiers(slot.Profile);

        if (modifiers.ShopDiscountCount <= 0 || baseItemCost == 0) {
            return new PlayerDiscountInfo(-1, slot.PlayerColor);
        }
        
        return new PlayerDiscountInfo(modifiers.ApplyDiscount(baseItemCost), slot.PlayerColor);
    }

    public void EnableAllSelectors() {
        UnsubscribeFromSelectorEvents();
        foreach (var selector in activeSelectors) {
            selector.CanAct = true;
        }
		SubscribeToSelectorEvents();
    }

    public void DisableAllSelectors() {
        foreach (var selector in activeSelectors) {
            selector.CanAct = false;
        }
    }
    
    private void UnsubscribeFromSelectorEvents() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged -= HandleSelectionChanged;
            selector.OnLockChanged -= HandleLockChanged;
            selector.OnLockRejected -= HandleLockRejected;
        }
    }

    public void UnlockAISelectors() {
        for (int i = 0; i < activeSelectors.Count; i++) {
            if (!playerService.PlayerIsHuman(i)) {
                activeSelectors[i].Unlock();
            }
        }
    }

    public List<ShopSlotSelector> GetSelectors() => activeSelectors;

    public void Cleanup() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged -= HandleSelectionChanged;
            selector.OnLockChanged -= HandleLockChanged;
        }
        activeSelectors.Clear();
    }

    private int GetLockedCount() {
        return activeSelectors.Count(shopSlotSelector => shopSlotSelector.IsLocked);
    }

    private int GetLockedAICount() {
        int count = 0;
        for (int i = 0; i < activeSelectors.Count; i++) {
            if (!playerService.PlayerIsHuman(i)) {
                if (activeSelectors[i].IsLocked) count++;
            }
        }
        return count;
    }

    private int GetHumanCount() {
        int count = 0;
        for (int i = 0; i < activeSelectors.Count; i++) {
            if (playerService.PlayerIsHuman(i)) count++;
        }
        return count;
    }
}