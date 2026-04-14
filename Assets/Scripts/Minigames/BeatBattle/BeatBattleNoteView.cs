using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames.BeatBattle {
    public class BeatBattleNoteView : MonoBehaviour {
        [SerializeField] private Image noteImage;
        
        private RectTransform rectTransform;

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetColor(Color color) {
            noteImage.color = color;
        }
        
        public void SetAnchoredY(float y) {
            var position = rectTransform.anchoredPosition;
            position.y = y;
            rectTransform.anchoredPosition = position;
        }

        public void PlayHitFeedback() {
            rectTransform.DOScale(1.3f, 0.1f).SetLoops(2, LoopType.Yoyo);
            DOTween.ToAlpha(() => noteImage.color, x => noteImage.color = x, 0f, 0.2f);
        }

        public void PlayMissFeedback() {
            DOTween.ToAlpha(() => noteImage.color, x => noteImage.color = x, 0f, 0.2f);
        }
    }
}