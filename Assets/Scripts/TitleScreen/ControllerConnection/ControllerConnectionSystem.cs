using System.Collections.Generic;
using System.Linq;
using Player;
using Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Input.ControllerConnection {
    public class ControllerConnectionSystem : MonoBehaviour {
        private List<Color> randomColorList = new() {
            Color.red,
            Color.orange,
            Color.darkBlue,
            Color.darkMagenta,
            Color.seaGreen,
            Color.paleVioletRed,
            Color.powderBlue
        };
        
        private List<PlayerSelector> playerSelectors;
        private IPlayerService playerService;
        [SerializeField] private GameObject playerSelectorPrefab;

        private void Awake() {
            playerSelectors = new List<PlayerSelector>();
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            playerService.OnPlayerJoined += OnPlayerJoined;
            ShuffleColors();
        }

        private void OnPlayerJoined(int slotIndex, PlayerProfile playerProfile) {
            Color pointerColor = GetRandomUnusedColor();
            GameObject playerSelectorGameObject = Instantiate(playerSelectorPrefab);
            PlayerSelector playerSelector = playerSelectorGameObject.GetComponent<PlayerSelector>();
            var slot = playerService.PlayerSlots[slotIndex];
            IDirectionalTwoButtonInputHandler inputHandler = slot.PlayerInput.GetComponent<IDirectionalTwoButtonInputHandler>();
            playerSelectors.Add(playerSelector);
            playerSelector.Initialize(pointerColor, inputHandler);
        }

        private void ShuffleColors() {
            randomColorList = randomColorList.OrderBy(a => Random.value).ToList();
        }

        private Color GetRandomUnusedColor() {
            if (randomColorList.Count <= 0) {
                Debug.LogError("Ran out of random colors for the controller connection system :(");
            }
            
            Color selectedColor = randomColorList[0];
            randomColorList.RemoveAt(0);
            
            return selectedColor;
        }

        private void OnDestroy() {
            playerService.OnPlayerJoined -= OnPlayerJoined;
        }
    }
}
