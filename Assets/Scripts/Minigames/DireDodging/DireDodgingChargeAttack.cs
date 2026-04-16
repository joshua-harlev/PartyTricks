using CoreData;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;
using Time = UnityEngine.Time;

namespace Minigames.DireDodging {
    public class DireDodgingChargeAttack : MonoBehaviour {
        [SerializeField] private SpriteRenderer chargeIndicator;
        [SerializeField] private ParticleSystem chargeParticles;
        [SerializeField] private Transform chargeAimIndicator;

        private DireDodgingPlayer player;
        private DireDodgingProjectilePool projectilePool;

        private bool isCharging;
        private bool showTrail;
        private float chargeStartTime;
        private EventInstance chargeLoopInstance;
        
        private float chargeTimeRequired;
        private float chargedProjectileScale;
        private float chargedProjectileSpeed;
        private float ghostChargeTime;
        private float ghostProjectileSpeed;
        private float chargeTimeRequiredOriginal;
        private float ghostChargeTimeOriginal;
        private float originalBaseHP;
        private float speedCoefficient;
        private int numberOfIncreasedAttackSpeedPowerups;
        private float chargedProjectileSpeedReductionPerPowerup;

        private EventReference chargeLoopEvent;
        private EventReference chargeReleaseEvent;
        private EventReference chargeShootEvent;
        private EventReference chargeCompleteEvent;

        private Material chargeRingMaterial;
        private static readonly int FillAmountProperty = Shader.PropertyToID("_FillAmount");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int FillStartAngleProperty = Shader.PropertyToID("_FillStartAngle");
        private bool chargeCompleteEventFired;
        private Tween fullChargePulseTween;
        private Vector3 chargeRingBaseScale;

        public bool IsCharging => isCharging;

        public void Initialize(DireDodgingPlayer player, DireDodgingProjectilePool pool,
            DireDodgingPlayerStatsSO stats, CombatModifiers modifiers) {
            this.player = player;
            this.projectilePool = pool;
            this.numberOfIncreasedAttackSpeedPowerups = modifiers.IncreasedAttackSpeedCount;
            this.chargedProjectileSpeedReductionPerPowerup = stats.ChargedProjectileSpeedReductionPerPowerup;
            chargeTimeRequiredOriginal = stats.ChargeTimeRequired;
            UpdateChargeTimeRequired();
            chargedProjectileScale = stats.ChargedProjectileScale;
            chargedProjectileSpeed = stats.ChargedProjectileSpeed * speedCoefficient;
            ghostChargeTime = stats.GhostChargeTime;
            ghostProjectileSpeed = stats.GhostProjectileSpeed;
            chargeLoopEvent = stats.ChargeLoopEvent;
            chargeReleaseEvent = stats.ChargeReleaseEvent;
            chargeShootEvent = stats.ChargeShootEvent;
            chargeCompleteEvent = stats.ChargeCompleteEvent;
            ghostChargeTimeOriginal = stats.GhostChargeTime;
            originalBaseHP = stats.BaseHealth;
            
            if (chargeIndicator != null) {
                chargeRingMaterial = chargeIndicator.material;
                chargeRingMaterial.SetFloat(FillStartAngleProperty, Mathf.PI/2f);
                chargeRingBaseScale = chargeIndicator.transform.localScale;
            }
        }

        public void UpdateChargeTimeRequired(float coefficient = 0f) {
            speedCoefficient = (1 + (numberOfIncreasedAttackSpeedPowerups * chargedProjectileSpeedReductionPerPowerup) + coefficient);
            chargeTimeRequired = chargeTimeRequiredOriginal / speedCoefficient;
            if (numberOfIncreasedAttackSpeedPowerups > 0) showTrail = true;
        }

        public void Tick() {
            HandleCharging();
            UpdateChargeIndicator();
        }

        public void ForceStop() {
            if (isCharging) {
                if(chargeParticles != null) {
                    chargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                if (chargeLoopInstance.isValid()) {
                    chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                    chargeLoopInstance.release();
                }

                isCharging = false;
                KillPulseTween();
                chargeCompleteEventFired = false;
            }
        }

        public void Cleanup() {
            if (chargeLoopInstance.isValid()) {
                chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                chargeLoopInstance.release();
            }
            KillPulseTween();
        }

        private void HandleCharging() {
            if (!player.InputEnabled) return;
            if (player.Navigator == null) return;

            if (GameSettings.Accessibility.ToggleCharge) {
                bool chargePressed = player.Navigator.ChargeIsPressed();

                if (chargePressed && !isCharging) {
                    StartCharging();
                } else if (chargePressed && isCharging) {
                    ReleaseCharge();
                }
            } else {
                bool chargeIsHeld = player.Navigator.ChargeIsHeld();

                if (chargeIsHeld && !isCharging) {
                    StartCharging();
                }

                if (isCharging && !chargeIsHeld) {
                    ReleaseCharge();
                }
            }
        }


        private void StartCharging() {
            isCharging = true;
            chargeStartTime = Time.time;
    
            if (chargeParticles != null) {
                chargeParticles.transform.localPosition = Vector3.zero;
                chargeParticles.transform.localRotation = Quaternion.identity;
                
                var main = chargeParticles.main;
                main.startColor = player.PlayerEffectColor;
                
                chargeParticles.Play();
            }

            if (chargeLoopInstance.isValid()) {
                chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                chargeLoopInstance.release();
            }

            chargeLoopInstance = RuntimeManager.CreateInstance(chargeLoopEvent);
            Debug.Log($"Created charge instance, valid: {chargeLoopInstance.isValid().ToString()}");
            float baseTime = player.IsGhostMode ? ghostChargeTime : chargeTimeRequired;
            float originalTime = player.IsGhostMode ? ghostChargeTimeOriginal : chargeTimeRequiredOriginal;
            float speedRatio = originalTime / baseTime;
            chargeLoopInstance.setPitch(speedRatio);
            chargeLoopInstance.start();
        }
        
        private void ReleaseCharge() {
            float chargeTime = Time.time - chargeStartTime;
            float requiredTime = player.IsGhostMode ? ghostChargeTime : chargeTimeRequired;
        
            if (chargeParticles != null) {
                chargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
    
            if (chargeLoopInstance.isValid()) {
                chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                chargeLoopInstance.release();
            }
    
            if (chargeTime >= requiredTime) {
                ShootChargedProjectile();
                RuntimeManager.PlayOneShot(chargeShootEvent);
            } else {
                // RuntimeManager.PlayOneShot(chargeReleaseEvent);
            }
    
            isCharging = false;
            player.ResetShootCooldown();
        }
        
        private void UpdateChargeIndicator() {
            if (chargeIndicator == null || chargeRingMaterial == null) return;
    
            if (isCharging) {
                float chargeTime = Time.time - chargeStartTime;
                float timeRequiredToCharge;
            
                if (player.IsGhostMode) {
                    timeRequiredToCharge = ghostChargeTime;
                } else {
                    timeRequiredToCharge = chargeTimeRequired;
                }
            
                float chargePercent = Mathf.Clamp01(chargeTime / timeRequiredToCharge);

                if (chargeLoopInstance.isValid()) {
                    chargeLoopInstance.setParameterByName("ChargeProgress", chargePercent);
                }

                chargeRingMaterial.SetFloat(FillAmountProperty, chargePercent);
                
                Color ringColor = Color.Lerp(Color.grey, player.PlayerEffectColor, chargePercent);
                ringColor.a = Mathf.Lerp(0.4f, 1f, chargePercent);
                chargeRingMaterial.SetColor(ColorProperty, ringColor);

                Vector2 shootDirection = player.GetShootDirection();
                float aimAngle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
                if (chargeAimIndicator != null) {
                    chargeAimIndicator.rotation = Quaternion.Euler(0, 0, aimAngle - 90f);
                }

                if (chargePercent >= 1f) {
                    if (!chargeCompleteEventFired) {
                        chargeCompleteEventFired = true;

                        if (chargeLoopInstance.isValid()) {
                            chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                            chargeLoopInstance.release();
                            RuntimeManager.PlayOneShot(chargeCompleteEvent);
                        }

                        fullChargePulseTween = chargeIndicator.transform
                            .DOScale(chargeRingBaseScale * 1.05f, 3f)
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Yoyo);
                    }
                }
                
                chargeIndicator.enabled = true;
                if(chargeAimIndicator != null) chargeAimIndicator.gameObject.SetActive(true);
            } else {
                chargeIndicator.enabled = false;
                chargeRingMaterial.SetFloat(FillAmountProperty, 0f);
                if(chargeAimIndicator != null) chargeAimIndicator.gameObject.SetActive(false);
                KillPulseTween();
                chargeCompleteEventFired = false;
            }
        }

        private void KillPulseTween() {
            if (fullChargePulseTween != null && fullChargePulseTween.IsActive()) {
                fullChargePulseTween.Kill();
                fullChargePulseTween = null;
                chargeIndicator.transform.localScale = chargeRingBaseScale;
            }
        }
        
        private void ShootChargedProjectile() {
            Vector2 shootDirection = player.GetShootDirection();
    
            float damage;
            float speed;
    
            if (player.IsGhostMode) {
                damage = 0f;
                speed = player.ProjectileSpeed * ghostProjectileSpeed;
            } else {
                damage = originalBaseHP; // if you use player.MaxHealth instead, attacks also scale on HP buffs which makes them MUCH stronger
                speed = player.ProjectileSpeed * chargedProjectileSpeed;
            }

            var projectile = projectilePool.GetCharged();
    
            Vector2 spawnOffset = shootDirection * (player.SpriteHalfWidth * 1.5f);
            projectile.transform.position = (Vector2)player.transform.position + spawnOffset;
            projectile.transform.SetParent(null);
            projectile.transform.rotation = player.GetRotationForDirection(shootDirection);
            projectile.transform.localScale = Vector3.one * (player.ProjectileScale * chargedProjectileScale * 0.3f);
    
            projectile.Initialize(player.PlayerIndex, damage, speed, shootDirection, player.IsGhostMode, showTrail);
        }
    }
}