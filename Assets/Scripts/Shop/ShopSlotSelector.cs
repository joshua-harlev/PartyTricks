using System;
using CoreData;
using Services;
using UnityEngine;

public class ShopSlotSelector : SelectionController {
    public event Action<ShopSlotSelector, int> OnSelectionChanged;
    public int CurrentShopItemIndex { get; private set; }
    private ShopNavigationService shopNavigationService;
    private ShopModifiers shopModifiers;
    private ShopItemUI[] shopItems;

    public void Initialize(int index, IDirectionalTwoButtonInputHandler navigator, ShopNavigationService shopNavigationService, PlayerProfile profile, ShopItemUI[] ShopItemUIElements, int currentShopIndex = 0) {
        PlayerIndex = index;
        Navigator = navigator;
        CurrentShopItemIndex = currentShopIndex;
        Profile = profile;
        var powerUpService = ServiceLocatorAccessor.GetService<IPowerUpService>();
        shopModifiers = powerUpService.GetShopModifiers(Profile);
        this.shopNavigationService = shopNavigationService;
        shopItems = ShopItemUIElements;
    }
    
    protected override void HandleNavigation() {
        var direction = Navigator.GetNavigate();
        var discreteDirection = CalculateDiscreteDirection(direction);
        
        bool moveDirectionIsNew = (discreteDirection != lastNavigateDirection);
        if (discreteDirection != Vector2.zero && moveDirectionIsNew) {
            CurrentShopItemIndex = shopNavigationService.Move(CurrentShopItemIndex, discreteDirection);
            OnSelectionChanged?.Invoke(this, CurrentShopItemIndex);
        }
        
        lastNavigateDirection = discreteDirection;
    }

    protected override bool CanLock() {
        ShopItemUI item = shopItems[CurrentShopItemIndex];
        int baseCost = item.GetItemCost();

        if (baseCost == 0) return true;
        
        int finalCost = shopModifiers.ApplyDiscount(baseCost);

        return Profile.Wallet.CanPurchase(finalCost);
    }

}
