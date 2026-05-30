using System.Linq;
using CoreData;
using Game;
using Services;
using UnityEngine;

namespace Shop {
    public class PowerupAssignmentDebugPanel {
        private ShopItem[] allItems;
        private int selectedCategoryIndex;
        private int selectedItemIndex;

        [RuntimeInitializeOnLoadMethod]
        private static void Register() {
            DebugMenu.PowerupPanelDraw = new PowerupAssignmentDebugPanel().Draw;
        }

        private readonly PowerUpCategory[] categories =
        {
            PowerUpCategory.Movement,
            PowerUpCategory.Combat,
            PowerUpCategory.Gambling,
            PowerUpCategory.Shop
        };

        private void Draw(IPlayerService playerService) {
            GUILayout.Space(20);
            GUILayout.Label("Powerup Assignment", GUI.skin.box);

            if (allItems == null) {
                allItems = Resources.LoadAll<ShopItem>("Powerups").ToArray();
            }

            ShopItem[] filteredItems =
                allItems.Where(item => item.Category == categories[selectedCategoryIndex]).ToArray();
            
            DrawCategoryButtons();
            DrawItemButtons(filteredItems);
            DrawAddToPlayerButtons(playerService, filteredItems);
            GUILayout.Space(10);
            DrawPlayerInfoAndClearButtons(playerService);
        }

        private static void DrawPlayerInfoAndClearButtons(IPlayerService playerService) {
            for (int i = 0; i < playerService.PlayerSlots.Count; i++) {
                var slot = playerService.PlayerSlots[i];
                if (slot?.Profile == null) continue;
                int itemCount = slot.Profile.Inventory.Items.Count;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"P{i+1}: {itemCount} item(s)");
                if (GUILayout.Button("Clear",GUILayout.Height(24))) {
                    slot.Profile.Inventory.Reset();
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawAddToPlayerButtons(IPlayerService playerService, ShopItem[] filteredItems) {
            if (selectedItemIndex < filteredItems.Length) {
                GUILayout.BeginHorizontal();
                for (int i = 0; i < 4; i++) {
                    if (GUILayout.Button($"+ P{i+1}", GUILayout.Height(30))) {
                        playerService.PlayerSlots[i].Profile.Inventory.AddItem(filteredItems[selectedItemIndex].ToDefinition());
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawItemButtons(ShopItem[] filteredItems) {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < filteredItems.Length; i++) {
                var item = filteredItems[i];
                string itemName = item.DisplayName;
                if (selectedItemIndex == i) itemName = $"[{itemName}]";
                if (GUILayout.Button(itemName, GUILayout.Height(24))) {
                    selectedItemIndex = i;
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCategoryButtons() {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < categories.Length; i++) {
                var category = categories[i];
                string categoryName = category.ToString();
                if (selectedCategoryIndex == i) categoryName = $"[{categoryName}]";
                if (GUILayout.Button(categoryName, GUILayout.Height(24))) {
                    selectedCategoryIndex = i;
                    selectedItemIndex = 0;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}