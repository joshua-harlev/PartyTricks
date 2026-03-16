using System.Collections;
using DG.Tweening;
using FMODUnity;
using UnityEngine;

namespace Minigames.DireDodging {
    public class DireDodgingDeathHandler : MonoBehaviour {
        private static bool isDeathZoomActive = false;
        private static float trueOriginalCameraSize;
        private static Vector3 trueOriginalCameraPosition;
        
        private const float cameraFreezeDuration = 1f;
        private const float cameraZoomAmount = 0.7f;

        private DireDodgingPlayer player;
        private DireDodgingChargeAttack chargeAttack;
        private DireDodgingProjectilePool projectilePool;
        
        private Tween cameraZoomTween;
        private float respawnDelay;
        private float invincibilityDuration;
        private float deathAnimationTimeInSeconds;
        private bool isInvincible;
        private EventReference deathEvent;
        
        public static bool IsDeathZoomActive => isDeathZoomActive;
        public bool IsInvincible => isInvincible;

        public static void CaptureOriginalCamera(Camera camera) {
            if (!isDeathZoomActive) {
                trueOriginalCameraSize = camera.orthographicSize;
                trueOriginalCameraPosition = camera.transform.position;
            }
        }

        public void Initialize(DireDodgingPlayer player, DireDodgingChargeAttack chargeAttack,
            DireDodgingProjectilePool pool, DireDodgingPlayerStatsSO stats) {
            this.player = player;
            this.chargeAttack = chargeAttack;
            this.projectilePool = pool;
            this.deathAnimationTimeInSeconds = stats.DeathAnimationTimeInSeconds;
            this.respawnDelay = 3f;
            this.invincibilityDuration = 2f;
            this.deathEvent = stats.DeathEvent;
        }

        public void TriggerDeath() {
            player.SetAliveState(false, true);

            Rigidbody2D rigidbody = player.PlayerRigidbody2D;
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.angularVelocity = 0f;
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            
            Time.timeScale = 0f;

            ZoomCameraOnDeath();
            chargeAttack.ForceStop();

            player.StopColorChangeSequence();
            TransitionSpriteOpacityOnDeath();
            RuntimeManager.PlayOneShot(deathEvent);
            StartCoroutine(DeathCoroutine());
        }

        public void Cleanup() {
            isDeathZoomActive = false;
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

            mainCamera.DOKill();
            mainCamera.transform.DOKill();
            DoCameraZoomSequence(mainCamera);
        }
        
        private void DoCameraZoomSequence(Camera camera) {
            isDeathZoomActive = true;
            Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, trueOriginalCameraPosition.z);
            camera.DOOrthoSize(trueOriginalCameraSize * cameraZoomAmount, cameraFreezeDuration * 0.5f).SetUpdate(true);
            camera.transform.DOMove(targetPosition, cameraFreezeDuration * 0.5f).SetUpdate(true);

            cameraZoomTween = DOVirtual.DelayedCall(cameraFreezeDuration, () => {
                ReturnCameraToOriginalState(camera);
                Time.timeScale = 1f;
                DireDodgingMinigameManager.Instance.EnableAllPlayerInput();
            }, false).SetUpdate(true);
        }
        
        private void ReturnCameraToOriginalState(Camera camera) {
            camera.DOOrthoSize(trueOriginalCameraSize, 0.3f).SetUpdate(true);
            camera.transform.DOMove(trueOriginalCameraPosition, 0.3f).SetUpdate(true).OnComplete(() => {
                isDeathZoomActive = false;
            });
        }
        
        private IEnumerator DeathCoroutine() {
            yield return new WaitForSeconds(deathAnimationTimeInSeconds);
            if (DireDodgingMinigameManager.Instance.GameHasEnded) {
                yield break;
            }
            player.EnableInput();

            Color ghostColor = player.BaseColor;
            ghostColor.a = 0.3f;
            player.PlayerSpriteRenderer.color = ghostColor;
            
            projectilePool.ReturnAllToPool();
            player.StopShooting();

            if (player.PlayerHealthBar != null) {
                player.PlayerHealthBar.gameObject.SetActive(false);
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
            Camera mainCamera = player.MainCamera;
            if (cameraZoomTween != null && cameraZoomTween.IsActive()) {
                cameraZoomTween.Kill();
                mainCamera.DOKill();
                mainCamera.transform.DOKill();

                mainCamera.DOOrthoSize(trueOriginalCameraSize, 0.3f).SetUpdate(true);
                mainCamera.transform.DOMove(trueOriginalCameraPosition, 0.3f).SetUpdate(true).OnComplete(() => {
                    isDeathZoomActive = false;
                });
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
    
            // Show health bar
            if (player.PlayerHealthBar != null) {
                player.PlayerHealthBar.gameObject.SetActive(true);
                player.PlayerHealthBar.UpdateDisplay(player.CurrentHealth, player.MaxHealth);
            }
    
            player.StartShooting();
    
            StartCoroutine(RespawnInvincibilityCoroutine());
        }
    }
}