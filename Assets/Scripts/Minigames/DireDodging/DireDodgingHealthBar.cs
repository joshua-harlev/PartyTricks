using DG.Tweening;
using UnityEngine;

namespace Minigames.DireDodging {
    public class DireDodgingHealthBar : MonoBehaviour {
        [SerializeField] private Transform ShieldBar;
        [SerializeField] private SpriteRenderer ShieldBarSprite;
        private Vector3 baseScale;
        private Vector3 shieldBaseScale;
        private float baseMaxHealth;
        private float shieldHP;

        private void Awake() {
            this.baseScale = transform.localScale;
            if (ShieldBar) {
                shieldBaseScale = ShieldBar.localScale;
                ShieldBar.gameObject.SetActive(false);
            }
        }

        public void UpdateDisplay(float currentHealth, float maxHealth) {
            if (maxHealth <= 0) return;
            if (currentHealth < 0) currentHealth = 0;

            float effectiveBaseMax = maxHealth;
            if (baseMaxHealth > 0) effectiveBaseMax = baseMaxHealth;

            float baseFraction = Mathf.Min(currentHealth, effectiveBaseMax) / effectiveBaseMax;
            transform.DOScale(new Vector3(baseScale.x*baseFraction, baseScale.y, baseScale.z), 0.25f).SetUpdate(true);

            if (ShieldBar && shieldHP > 0f) {
                float shieldFraction = Mathf.Max(0f, currentHealth - effectiveBaseMax) / shieldHP;
                bool hasShield = shieldFraction > 0f;
                ShieldBar.gameObject.SetActive(hasShield);
                if (hasShield) {
                    ShieldBar.DOScale(new Vector3(shieldBaseScale.x*shieldFraction, shieldBaseScale.y, shieldBaseScale.z), 0.25f).SetUpdate(true);
                }
            }
        }

        public void SetVisible(bool visible) {
            gameObject.SetActive(visible);
            if (ShieldBar != null && !visible) {
                ShieldBar.gameObject.SetActive(false);
            }
        }

        public void InitializeWithShield(float baseMaxHealth, float shieldHP, Color shieldColor) {
            this.baseMaxHealth = baseMaxHealth;
            this.shieldHP = shieldHP;
            ShieldBarSprite.color = shieldColor;
        }
    }
}
