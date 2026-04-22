using System.Collections;
using DG.Tweening;
using FMODUnity;
using UnityEngine;

namespace Minigames.DireDodging {
    public class DireDodgingDeathHandler : MonoBehaviour {
        private const float cameraFreezeDuration = 0.75f;
        private const float cameraZoomAmount = 0.5f;

        private DireDodgingPlayer player; 
        private DireDodgingChargeAttack chargeAttack;
        private DireDodgingProjectilePool projectilePool;
        
        private Tween cameraZoomTween;
        private float respawnDelay;
        private float invincibilityDuration;
        private float deathAnimationTimeInSeconds;
        private bool isInvincible;
        private EventReference deathEvent;
        private ParticleSystem deathParticles;
        
        public static bool IsDeathZoomActive => DireDodgingCameraZoomService.DeathZoomActive;
        public bool IsInvincible => isInvincible;
        
        public void Initialize(DireDodgingPlayer player, DireDodgingChargeAttack chargeAttack,
            DireDodgingProjectilePool pool, DireDodgingPlayerStatsSO stats, ParticleSystem deathParticles) {
            this.player = player;
            this.chargeAttack = chargeAttack;
            this.projectilePool = pool;
            this.deathAnimationTimeInSeconds = stats.DeathAnimationTimeInSeconds;
            this.respawnDelay = 3f;
            this.invincibilityDuration = 2f;
            this.deathEvent = stats.DeathEvent;
            this.deathParticles = deathParticles;
            var main = deathParticles.main;
            main.startColor = player.PlayerEffectColor;
        }

        public void TriggerDeath() {
            player.SetAliveState(false, true);
            player.ClearStun();

            Rigidbody2D rigidbody = player.PlayerRigidbody2D;
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.angularVelocity = 0f;
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            
            Time.timeScale = 0f;
            deathParticles.Play();

            ZoomCameraOnDeath();
            chargeAttack.ForceStop();

            player.StopColorChangeSequence();
            TransitionSpriteOpacityOnDeath();
            RuntimeManager.PlayOneShot(deathEvent);
            StartCoroutine(DeathCoroutine());
        }

        private void TransitionSpriteOpacityOnDeath() {
            var color = player.BaseColor;
            color.a = 0.1f;
            player.PlayerSpriteRenderer.DOColor(color, deathAnimationTimeInSeconds).SetUpdate(true);
        }
        
        private void ZoomCameraOnDeath() {
            var mainCamera = player.MainCamera;
            if (mainCamera == null) {
                throw new MissingComponentException("Main Camera is missing.");
            }

            DireDodgingCameraZoomService.StartDeathZoom(transform.position, cameraZoomAmount, cameraFreezeDuration * 0.5f);
            cameraZoomTween = DOVirtual.DelayedCall(cameraFreezeDuration, () =>
            {
                DireDodgingCameraZoomService.ReturnFromDeathZoom(0.3f);
                Time.timeScale = 1f;
                DireDodgingMinigameManager.Instance.EnableAllPlayerInput();
            }, false).SetUpdate(true);
        }
        
        private IEnumerator DeathCoroutine() {
            yield return new WaitForSeconds(deathAnimationTimeInSeconds);
            if (DireDodgingMinigameManager.Instance.GameHasEnded) {
                yield break;
            }
            player.EnableInput();

            player.PlayerRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            
            Color ghostColor = player.BaseColor;
            ghostColor.a = 0.3f;
            player.PlayerSpriteRenderer.color = ghostColor;
            
            projectilePool.ReturnAllToPool();
            player.StopShooting();

            if (player.PlayerHealthBar != null) {
                player.PlayerHealthBar.SetVisible(false);
            }
            
            yield return new WaitForSeconds(respawnDelay);

            Respawn();
        }

        private IEnumerator RespawnInvincibilityCoroutine() {
            isInvincible = true;
    
            float flashInterval = 0.1f;
            float elapsed = 0f;
            var SpriteRenderer = player.PlayerSpriteRenderer;
    
            while (elapsed < invincibilityDuration) {
                SpriteRenderer.enabled = !SpriteRenderer.enabled;
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }
    
            SpriteRenderer.enabled = true;
            isInvincible = false;
        }

        private void Respawn() {
            deathParticles.Stop();
            if (cameraZoomTween != null && cameraZoomTween.IsActive()) {
                cameraZoomTween.Kill();
                DireDodgingCameraZoomService.CancelDeathZoom();
                Time.timeScale = 1f;
                DireDodgingMinigameManager.Instance.EnableAllPlayerInput();
            }
        
            player.SetAliveState(true, false);
            player.ResetHealth();
        
            player.PlayerRigidbody2D.linearVelocity = Vector2.zero;
            player.PlayerRigidbody2D.angularVelocity = 0f;
        
            player.PlayerRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
    
            player.PlayerCollider2D.enabled = true;
    
            Color aliveColor = player.BaseColor;
            aliveColor.a = 1f;
            player.PlayerSpriteRenderer.color = aliveColor;
            
            if (player.PlayerHealthBar != null) {
                player.PlayerHealthBar.SetVisible(true);
                player.PlayerHealthBar.UpdateDisplay(player.CurrentHealth, player.MaxHealth);
            }
    
            player.StartShooting();
    
            StartCoroutine(RespawnInvincibilityCoroutine());
        }
    }
}