using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Shop {
    public struct PlayerDiscountInfo {
        public int DiscountedCost; // -1 if no discount
        public Color PlayerColor;

        public PlayerDiscountInfo(int discountedCost, Color playerColor) {
            DiscountedCost = discountedCost;
            PlayerColor = playerColor;
        }
    }

    public class ShopItemUI : MonoBehaviour {
        [SerializeField] private Image icon;
        [FormerlySerializedAs("name")] [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text cost;
        [SerializeField] private TMP_Text category;
        [SerializeField] private TMP_Text description;
        [SerializeField] private ShopPointers pointers;
        private ShopItem item;
        private readonly List<(int playerIndex, PlayerDiscountInfo discountInfo)> activeHoverDiscounts = new();
    
        public void SetItem(ShopItem item) {
            this.item = item;
            icon.sprite = item.Icon;
            itemName.text = item.DisplayName;
            cost.text = "Cost: " + item.Cost;
            category.text = item.Category.ToString();
            description.text = item.Description;
            if (item.Id == "emptyItem") {
                SetEmpty();
            }
        }

        private void SetEmpty() {
            cost.gameObject.SetActive(false);
            category.gameObject.SetActive(false);
            description.gameObject.SetActive(false);
        }

        public void OnPointedTo(int playerIndex, bool shouldShow, bool shouldBeLocked) {
            pointers.OnPointedTo(playerIndex, shouldShow, shouldBeLocked);
        }

        public override string ToString() {
            return $"{itemName.text} ({cost.text}): {description.text}";
        }

        public int GetItemCost() {
            return item.Cost;
        }

        public ShopItem GetItem() {
            return item;
        }

        public void OnCannotAfford(int selectorPlayerIndex) {
            pointers.OnCannotAfford(selectorPlayerIndex);
        }

        public void OnCannotAffordPermanent(int playerIndex) {
            pointers.OnCannotAffordPermanent(playerIndex);
        }

        public void SetPointerBonusStatus(int playerIndex, bool showBonus) {
            pointers.SetPointerBonusStatus(playerIndex, showBonus);
        }

        public void SetPlayerHoverDiscount(int playerIndex, bool isHovering, PlayerDiscountInfo discountInfo) {
            activeHoverDiscounts.RemoveAll(x => x.playerIndex == playerIndex);
            if (isHovering && discountInfo.DiscountedCost >= 0) {
                activeHoverDiscounts.Add((playerIndex, discountInfo));
            }

            UpdateCostText();
        }

        private void UpdateCostText() {
            if (item == null) return;
            var text = "Cost: " + item.Cost;
            foreach (var (_, discountInfo) in activeHoverDiscounts) {
                string hexColor = ColorUtility.ToHtmlStringRGB(discountInfo.PlayerColor);
                text += $" <color=#{hexColor}>({discountInfo.DiscountedCost})</color>";
            }

            cost.text = text;
        }
    }
}