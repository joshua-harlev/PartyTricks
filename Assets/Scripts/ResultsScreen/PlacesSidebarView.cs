using ResultsScreen.Core;
using TMPro;
using UnityEngine;

namespace ResultsScreen {
    public class PlacesSidebarView : MonoBehaviour {
        [SerializeField] private TMP_Text[] PlayerNumberLabels;
        [SerializeField] private TMP_Text[] PlaceLabels;

        [SerializeField] private string[] PlaceNames = { "1st", "2nd", "3rd", "4th" };

    public void UpdateLabels(PlacesEntry[] entries) {
            for (int i = 0; i < entries.Length; i++) {
                PlayerNumberLabels[i].text = $"P{entries[i].PlayerIndex+1}";
                PlaceLabels[i].text = PlaceNames[entries[i].Rank];
            }
        }
    }
}