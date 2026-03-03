using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ResultsScreen {
    public class PlacesBarView : MonoBehaviour {
        [SerializeField] private Image barImage;
        [SerializeField] private Image characterIconImage;
        [SerializeField] private Image crownImage;
        [SerializeField] private TMP_Text fundsLabel;
        
        private RectTransform barRectTransform;

        private void Awake() {
            barRectTransform = barImage.GetComponent<RectTransform>();
        }

        public void SetColor(Color color) {
            barImage.color = color;
            crownImage.color = color;
        }

        public void SetCharacterIcon(Sprite icon) {
            characterIconImage.sprite = icon;
        }

        public void SetFundsText(string text) {
            fundsLabel.text = text;
        }

        public void SetCrownVisibility(bool visible) {
            switch (crownImage.gameObject.activeSelf) {
                // should disappear
                case true when !visible:
                    crownImage.gameObject.SetActive(false);
                    break;
                case false when visible:
                    crownImage.color = new Color(crownImage.color.r, crownImage.color.g, crownImage.color.b, 0f);
                    crownImage.gameObject.SetActive(true);
                    crownImage.DOFade(1f, 0.3f);
                    break;
            }
        }

        public void SetBarHeight(float height) {
            barRectTransform.sizeDelta = new Vector2(barRectTransform.sizeDelta.x, height);
        }

        public float GetBarHeight() {
            return barRectTransform.sizeDelta.y;
        }
        
        public RectTransform GetBarRectTransform() {
            return barRectTransform;
        }
    }
}