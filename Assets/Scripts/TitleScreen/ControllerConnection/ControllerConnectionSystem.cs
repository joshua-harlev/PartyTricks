using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Input.ControllerConnection {
    // basically pseudocode for now; not linked up to OnPlayerJoined or tested
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
        [SerializeField] private GameObject playerSelectorPrefab;

        private void Awake() {
            ShuffleColors();
        }

        private void OnPlayerJoined() {
            Color pointerColor = GetRandomUnusedColor();
            GameObject playerSelectorGameObject = Instantiate(playerSelectorPrefab);
            PlayerSelector playerSelector = playerSelectorGameObject.GetComponent<PlayerSelector>();
            playerSelectors.Add(playerSelector);
            playerSelector.Initialize(pointerColor);
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
    }
}
