using UnityEngine;

namespace CoreData {
    public struct ShopModifiers {
        public readonly int ShopDiscountCount;
        private const int DISCOUNT_PER_ITEM = 75;

        public ShopModifiers(int shopDiscountCount) {
            ShopDiscountCount = shopDiscountCount;
        }

        public int ApplyDiscount(int baseCost) {
            return Mathf.Max(0, baseCost - (ShopDiscountCount*DISCOUNT_PER_ITEM));
        }
    }
}