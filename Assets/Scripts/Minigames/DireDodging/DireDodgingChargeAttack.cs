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

        private EventReference chargeLoopEvent;
        private EventReference chargeReleaseEvent;
        private EventReference chargeShootEvent;

        public bool IsCharging => isCharging;

        public void Initialize(DireDodgingPlayer player, DireDodgingProjectilePool pool,
            DireDodgingPlayerStatsSO stats) {
            this.player = player;
            this.projectilePool = pool;

            chargeTimeRequired = stats.ChargeTimeRequired;
            chargedProjectileScale = stats.ChargedProjectileScale;
            chargedProjectileSpeed = stats.ChargedProjectileSpeed;
            ghostChargeTime = stats.GhostChargeTime;
            ghostProjectileSpeed = stats.GhostProjectileSpeed;
            chargeLoopEvent = stats.ChargeLoopEvent;
            chargeReleaseEvent = stats.ChargeReleaseEvent;
            chargeShootEvent = stats.ChargeShootEvent;
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

            chargeLoopInstance = RuntimeManager.CreateInstance(chargeLoopEvent);
            chargeLoopInstance.start();
        }
        
        private void ReleaseCharge() {
            float chargeTime = Time.time - chargeStartTime;
            float requiredTime = player.IsGhostMode ? ghostChargeTime : chargeTimeRequired;
        
            if (chargeParticles != null) {
                chargeParticles.Stop();
            }
    
            if (chargeLoopInstance.isValid()) {
                chargeLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                chargeLoopInstance.release();
            }
    
            if (chargeTime >= requiredTime) {
                ShootChargedProjectile();
                RuntimeManager.PlayOneShot(chargeShootEvent);
            } else {
                RuntimeManager.PlayOneShot(chargeReleaseEvent);
            }
    
            isCharging = false;
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
        
                chargeIndicator.transform.localScale = new Vector3(2f, chargePercent * 2, 1f);
        
                if (chargePercent >= 1f) {
                    chargeIndicator.color = Color.green;
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
                damage = player.MaxHealth; //changed to the player who's shooting's max health -> benefits to more HP 
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