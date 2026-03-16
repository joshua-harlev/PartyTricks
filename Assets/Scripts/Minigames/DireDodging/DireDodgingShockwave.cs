using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Minigames.DireDodging {
    public class DireDodgingShockwave : MonoBehaviour {
        public enum ShockwaveState {Disabled, Charging, Warning, Holding}

        [SerializeField] private DireDodgingShockwaveConfigSO config;
        [SerializeField] private ParticleSystem chargeParticles;
        [SerializeField] private ParticleSystem burstParticles;
        [SerializeField] private SpriteRenderer ringSprite;

        private DireDodgingPlayer player;
        private Color playerColor;
        private DireDodgingShockwaveData data;
        private bool isInitialized;
        private EventInstance chargeSoundInstance;
        private static int activeShockwaveCount = 0;
        private bool isZooming;
        private Color playerEffectColor;
        private const bool ZoomEnabled = true;
        private const float zoomPercentage = 1 - 0.10f;

        private const float zoomWarningStartPercent = 0.3f;
        private const float zoomReverseDelay = 0.15f;
        private const float zoomReverseDurationInSeconds = 0.08f;

        public void Initialize(DireDodgingPlayer playerInstance, int stackCount) {
            player = playerInstance;
            data = DireDodgingShockwaveData.Create(config, stackCount);
            ringSprite.enabled = false;
            isInitialized = true;
            playerEffectColor = playerInstance.PlayerEffectColor;
            UpdateChargeParticleColor();
            UpdateBurstParticleColor();
            UpdateRingColor();
        }

        private void UpdateChargeParticleColor() {
            var mainModule = chargeParticles.main;
            mainModule.startColor = playerEffectColor;
        }
        
        private void UpdateBurstParticleColor() {
            var mainModule = burstParticles.main;
            mainModule.startColor = playerEffectColor;
        }

        private void UpdateRingColor() {
            ringSprite.color = playerEffectColor;
        }

        public void Tick() {
            if (!isInitialized) return;
            
            if (data.State == ShockwaveState.Disabled) return;
            if (!player.IsAlive || !player.InputEnabled || player.IsGhostMode) return;
            if (player.IsStunned) return;

            data.Timer -= Time.deltaTime;
            switch (data.State) {
                case ShockwaveState.Charging:
                    if (data.Timer <= 0f) {
                        data.State = ShockwaveState.Warning;
                        data.Timer = data.WarningDurationInSeconds;
                        activeShockwaveCount++;
                        StartWarningVisuals();
                        if(activeShockwaveCount > 1) DireDodgingCameraZoomService.ReverseShockwaveZoom(zoomReverseDurationInSeconds);
                    }
                    break;
                
                case ShockwaveState.Warning:
                    UpdateWarningVisuals(1f - data.Timer / data.WarningDurationInSeconds);
                    if (ZoomEnabled && activeShockwaveCount == 1 && !DireDodgingDeathHandler.IsDeathZoomActive) {
                        float warningProgress = 1f - data.Timer / data.WarningDurationInSeconds;
                        if (!isZooming && warningProgress >= zoomWarningStartPercent) {
                            isZooming = true;
                            float remainingDuration = data.Timer + data.HoldDurationInSeconds;
                            DireDodgingCameraZoomService.StartShockwaveZoom(transform.position, zoomPercentage, 0.15f, remainingDuration);
                        }

                        if (isZooming) {
                            DireDodgingCameraZoomService.UpdateShockwaveZoomPosition(transform.position);
                        }
                    }
                    if (data.Timer <= 0f) {
                        chargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        data.State = ShockwaveState.Holding;
                        data.Timer = data.HoldDurationInSeconds;
                    }
                    break;
                
                case ShockwaveState.Holding:
                    if (ZoomEnabled && isZooming && activeShockwaveCount == 1 && !DireDodgingDeathHandler.IsDeathZoomActive) {
                        DireDodgingCameraZoomService.UpdateShockwaveZoomPosition(transform.position);
                    }
                    if (data.Timer <= 0f) {
                        Fire();
                        data.State = ShockwaveState.Charging;
                        data.Timer = data.CooldownDurationInSeconds;
                    }

                    break;
            }
        }

        private void StartWarningVisuals() {
            var shape = chargeParticles.shape;
            shape.radius = GetScreenRadius();
            chargeParticles.Play();
            if (!config.ChargeSound.IsNull) {
                chargeSoundInstance = RuntimeManager.CreateInstance(config.ChargeSound);
                chargeSoundInstance.start();
            }
        }
        
        private void UpdateWarningVisuals(float progress) {
            var mainModule = chargeParticles.main;
            mainModule.startSpeed = Mathf.Lerp(-4f, -25f, progress);
            mainModule.startSize = Mathf.Lerp(0.1f, 0.03f, progress);
            
            var emission = chargeParticles.emission;
            emission.rateOverTime = Mathf.Lerp(3f, 200f, progress);
        }


        private void Fire() {
            activeShockwaveCount--;
            if (isZooming) {
                isZooming = false;
                DOVirtual.DelayedCall(zoomReverseDelay, () =>
                {
                    DireDodgingCameraZoomService.ReverseShockwaveZoom(zoomReverseDurationInSeconds);
                });
            }
            chargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            StopChargeSound();

            if (!config.FireSound.IsNull) {
                RuntimeManager.PlayOneShot(config.FireSound);
            }

            
            float screenRadius = GetScreenRadius();
            
            
            ShowRingEffect(screenRadius);
            if(burstParticles != null) burstParticles.Play();
            PerformHitDetection();
        }

        private void PerformHitDetection() {
            Vector2 origin = transform.position;
            float hitRadius = GetScreenRadius();
            Collider2D[] hits =  Physics2D.OverlapCircleAll(origin, hitRadius);

            foreach (var hit in hits) {
                DireDodgingPlayer target = hit.GetComponent<DireDodgingPlayer>();
                if (!target) continue;
                if (target == player) continue;
                if (!target.IsAlive) continue;
                if (target.IsGhostMode) continue;

                Vector2 targetPosition = target.transform.position;
                Vector2 direction = targetPosition - origin;
                float distance = direction.magnitude;
                
                RaycastHit2D[] rayHits = Physics2D.RaycastAll(origin, direction.normalized, distance);
                bool hitBlocked = false;
                foreach (var rayHit in rayHits) {
                    if (rayHit.collider.CompareTag("Wall")) {
                        hitBlocked = true;
                        break;
                    }
                }

                if (!hitBlocked) {
                    target.Stun(data.StunDurationInSeconds);
                }
            }
        }

        private void ShowRingEffect(float screenRadius) {
            ringSprite.enabled = true;
            ringSprite.transform.localScale = Vector3.zero;
            Color ringColor = ringSprite.color;
            ringColor.a = 0.7f;
            ringSprite.color = ringColor;
            
            float spriteWorldSize = ringSprite.sprite.bounds.size.x;
            float parentScale = transform.lossyScale.x;
            float targetScale = (screenRadius * 5f) / (spriteWorldSize * parentScale);

            ringSprite.transform.DOScale(Vector3.one * targetScale, config.RingExpansionDurationInSeconds).SetEase(Ease.OutQuad);
            ringSprite.DOFade(0f, config.RingExpansionDurationInSeconds).SetEase(Ease.InQuad).OnComplete(() =>
            {
                ringSprite.enabled = false;
                ringSprite.transform.localScale = Vector3.zero;
            });
        }

        private void StopChargeSound() {
            if (chargeSoundInstance.isValid()) {
                chargeSoundInstance.stop(STOP_MODE.IMMEDIATE);
                chargeSoundInstance.release();
            }
        }

        private float GetScreenRadius() {
            Camera playerMainCamera = player.MainCamera;
            Vector2 bottomLeftCorner = playerMainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
            Vector2 topRightCorner = playerMainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            return (topRightCorner - bottomLeftCorner).magnitude / 2f;
        }

        public void ForceStop() {
            if (!isInitialized) return;
            if (data.State == ShockwaveState.Warning || data.State == ShockwaveState.Holding) {
                activeShockwaveCount--;
                if (isZooming) {
                    isZooming = false;
                    DireDodgingCameraZoomService.ReverseShockwaveZoom(zoomReverseDurationInSeconds);
                }
            }
            chargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if(burstParticles != null) burstParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ringSprite.enabled = false;
            StopChargeSound();
            data.State = ShockwaveState.Disabled;
        }

        public void Cleanup() {
            StopChargeSound();
            DOTween.Kill(ringSprite.transform);
            DOTween.Kill(ringSprite);
            activeShockwaveCount = 0;
        }
    }
}