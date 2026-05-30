using UnityEngine;

namespace Shop {
    public struct PlayerDiscountInfo {
        public int DiscountedCost; // -1 if no discount
        public Color PlayerColor;

        public PlayerDiscountInfo(int discountedCost, Color playerColor) {
            DiscountedCost = discountedCost;
            PlayerColor = playerColor;
        }
    }
}