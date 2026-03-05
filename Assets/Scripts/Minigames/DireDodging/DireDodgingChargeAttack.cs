using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Minigames.DireDodging {
    public class DireDodgingChargeAttack : MonoBehaviour {
        [SerializeField] private SpriteRenderer chargeIndicator;
        [SerializeField] private ParticleSystem chargeParticles;

        private DireDodgingPlayer player;
        private DireDodgingProjectilePool projectilePool;

        private bool isCharging;
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

        public bool IsCharging => isCharging;

        public void Initialize(DireDodgingPlayer player, DireDodgingProjectilePool pool,
            DireDodgingPlayerStatsSO stats, int numberOfIncreasedAttackSpeedPowerups) {
            this.player = player;
            this.projectilePool = pool;
            this.numberOfIncreasedAttackSpeedPowerups = numberOfIncreasedAttackSpeedPowerups;
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
        }

        public void UpdateChargeTimeRequired(float coefficient = 0f) {
            speedCoefficient = (1 + (numberOfIncreasedAttackSpeedPowerups * chargedProjectileSpeedReductionPerPowerup) + coefficient);
            chargeTimeRequired = chargeTimeRequiredOriginal / speedCoefficient;
        }

        public void Tick() {
            HandleCharging();
            UpdateChargeIndicator();
            UpdateChargeParticleDirection();
        }

        public void ForceStop() {
            if (isCharging) {
                if(chargeParticles != null) {
                    chargeParticles.Stop();
                }

                if (chargeLoopInstance.isValid()) {
                    chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                    chargeLoopInstance.release();
                }

                isCharging = false;
            }
        }

        public void Cleanup() {
            if (chargeLoopInstance.isValid()) {
                chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                chargeLoopInstance.release();
            }
        }

        private void HandleCharging() {
            if (!player.InputEnabled) return;
            if (player.Navigator == null) return;

            bool chargeIsHeld = player.Navigator.ChargeIsHeld();

            if (chargeIsHeld && !isCharging) {
                StartCharging();
            }

            if (isCharging && !chargeIsHeld) {
                ReleaseCharge();
            }
        }

        private void StartCharging() {
            isCharging = true;
            chargeStartTime = Time.time;
    
            if (chargeParticles != null) {
                Vector2 shootDirection = player.GetShootDirection();
                Vector2 particleOffset = shootDirection * 2f;
                chargeParticles.transform.localPosition = particleOffset;
        
                float angle = Mathf.Atan2(-shootDirection.y, -shootDirection.x) * Mathf.Rad2Deg;
                chargeParticles.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        
                chargeParticles.Play();
            }

            if (chargeLoopInstance.isValid()) {
                chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                chargeLoopInstance.release();
            }

            chargeLoopInstance = RuntimeManager.CreateInstance(chargeLoopEvent);
            Debug.Log($"Created charge instance, valid: {chargeLoopInstance.isValid()}");
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
                chargeParticles.Stop();
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
            if (chargeIndicator == null) return;
    
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
        
                chargeIndicator.transform.localScale = new Vector3(3.6f, chargePercent * 3.6f, 1f);
        
                if (chargePercent >= 1f) {
                    chargeIndicator.color = Color.green;

                    if (chargeLoopInstance.isValid()) {
                        chargeLoopInstance.stop(STOP_MODE.IMMEDIATE);
                        chargeLoopInstance.release();
                        RuntimeManager.PlayOneShot(chargeCompleteEvent);
                    }
                    
                } else {
                    chargeIndicator.color = Color.yellow;
                }
        
                chargeIndicator.enabled = true;
            } else {
                chargeIndicator.enabled = false;
            }
        }
        
        private void UpdateChargeParticleDirection() {
            if (!isCharging || chargeParticles == null) return;
    
            Vector2 shootDirection = player.GetShootDirection();
            Vector2 particleOffset = shootDirection * 2f;
            chargeParticles.transform.localPosition = particleOffset;
    
            float angle = Mathf.Atan2(-shootDirection.y, -shootDirection.x) * Mathf.Rad2Deg;
            chargeParticles.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
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
    
            projectile.transform.rotation = player.GetRotationForDirection(shootDirection);
            projectile.transform.localScale = Vector3.one * player.ProjectileScale * chargedProjectileScale;
    
            projectile.Initialize(player.PlayerIndex, damage, speed, shootDirection, player.IsGhostMode);
        }
    }
}