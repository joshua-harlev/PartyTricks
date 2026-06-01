using System.Collections;
using System.Collections.Generic;
using CoreData;
using Debug;
using DG.Tweening;
using FMODUnity;
using Input;
using Options;
using Services;
using UnityEngine;

namespace Minigames.DireDodging {
    public class DireDodgingPlayer : MonoBehaviour {
        public bool InputEnabled => inputEnabled;
        public IDirectionalTwoButtonInputHandler Navigator => navigator;
        public bool IsGhostMode => isGhostMode;
        public bool IsStunned => isStunned;
        public int PlayerIndex => playerIndex;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileScale => projectileScale;
        public float SpriteHalfWidth => spriteHalfWidth;
        public bool IsAlive => isAlive;
        public SpriteRenderer PlayerSpriteRenderer => SpriteRenderer;
        public Rigidbody2D PlayerRigidbody2D => Rigidbody2D;
        public Collider2D PlayerCollider2D => Collider2D;
        public DireDodgingHealthBar PlayerHealthBar => HealthBar;
        public Color BaseColor => baseColor;
        public Camera MainCamera => mainCamera;
        public Vector2 LastMoveDirection => lastMoveDirection;

        private float maxMoveSpeed;
        private float projectileScale;
        private float projectileSpeed;
        private float baseDamage;
        private float maxHealth;
        private float currentHealth;
        private float projectileShootRate;
        private float spriteHalfWidth;
        private float spriteHalfHeight;
        private float damageAnimationTimeInSeconds;
        private int multishotCount;
        private int originalScreenWidth, originalScreenHeight;

        [SerializeField] private DireDodgingPlayerStatsSO PlayerStatsSO;
        [SerializeField] private SpriteRenderer SpriteRenderer;
        [SerializeField] private Collider2D Collider2D;
        [SerializeField] private Rigidbody2D Rigidbody2D;
        [SerializeField] private DireDodgingHealthBar HealthBar;
        [SerializeField] private DireDodgingProjectilePool ProjectilePool;
        [SerializeField] private DireDodgingChargeAttack ChargeAttack;
        [SerializeField] private DireDodgingDeathHandler DeathHandler;
        [SerializeField] private DireDodgingShockwave Shockwave;
        [SerializeField] private ParticleSystem stunParticles;
        [SerializeField] private ParticleSystem deathParticles;
    
        private Color playerEffectColor = Color.white;
        public Color PlayerEffectColor => playerEffectColor;

        private Coroutine shootingCoroutineInstance = null;
        private Vector2 lastMoveDirection = Vector2.right;
        private DireDodgingIntensityStats intensityStats;
    
        private int playerIndex;
        private Sequence colorChangeSequence;
        private Color baseColor;
        private IDirectionalTwoButtonInputHandler navigator;
        private bool isAI;
        private bool inputEnabled;
        private bool isAlive = true;
        private bool shootEventExists;
        private bool isGhostMode = false;
        private bool isPlayingStunnedAnimation = false;
        private bool showTrail = false;
        private bool shockwaveZoomEnabled;
        private float baseMaxHealth;
        private float screenBottom;
        private float screenTop;
        private float screenLeft;
        private float screenRight;

        private Coroutine damageCoroutineInstance = null;
        private Coroutine stunCoroutineInstance = null;
        private Coroutine intensityCoroutineInstance = null;
        private Tween stunColorTween;
        private Tween stunShakeTween;
        private float originalSpeedBeforeStun;
        private Camera mainCamera;
        private readonly Quaternion leftRotation = Quaternion.Euler(0, 0, 90);
        private readonly Quaternion rightRotation = Quaternion.Euler(0, 0, 270);
        private readonly Quaternion upRotation = Quaternion.Euler(0, 0, 0);
        private readonly Quaternion downRotation = Quaternion.Euler(0, 0, 180);
        private EventReference hitEvent;

        private float ghostMoveSpeedMultiplier;
        private float defaultStunDuration;
        private float stunNudgeMultiplier;
    
        private PlayerColorConfig playerColorConfig;

        private void Awake() {
            playerColorConfig = ServiceLocatorAccessor.GetService<PlayerColorConfig>();
            baseColor = SpriteRenderer.color;
            DireDodgingCameraZoomService.OnShockwaveZoomStatusChange += OnShockwaveZoomStatusChange;
        }

        private void OnShockwaveZoomStatusChange(bool zooming) {
            if (!zooming) {
                // reset screen position
                UpdateScreenBounds();
            }
        }

        private void Start() {
            originalScreenWidth = Screen.width;
            originalScreenHeight = Screen.height;
        }

        public void Initialize(int index, IDirectionalTwoButtonInputHandler inputHandler, bool initializeAsAI, CombatModifiers modifiers, bool isDoubleRound) {
            this.playerIndex = index;
            playerEffectColor = playerColorConfig.GetEffectColor(playerIndex);
            mainCamera = Camera.main;
            UpdateScreenBounds();
            ApplyBaseStats();
            if (isDoubleRound) {
                this.maxHealth *= 2;
                this.currentHealth *= 2;
            }
            this.baseMaxHealth = this.maxHealth;
            ApplyStatBuffs(modifiers.IncreasedHPCount, modifiers.IncreasedAttackSpeedCount);
            if (modifiers.IncreasedHPCount > 0) {
                HealthBar.InitializeWithShield(this.baseMaxHealth, this.maxHealth - this.baseMaxHealth, playerEffectColor);
                HealthBar.UpdateDisplay(currentHealth, maxHealth);
            }
            this.multishotCount = modifiers.MultishotCount;
            this.navigator = inputHandler;
            this.isAI = initializeAsAI;
            this.inputEnabled = false;
            spriteHalfWidth = SpriteRenderer.bounds.size.x / 2f;

            // TODO calculate this more effectively
            spriteHalfHeight = SpriteRenderer.bounds.extents.y + PlayerStatsSO.HealthBarYOffset; // offset added for health bar

            shootEventExists = !PlayerStatsSO.BasicShootEvent.IsNull;
        
            ProjectilePool.Initialize(index);
            ChargeAttack.Initialize(this, ProjectilePool, PlayerStatsSO, modifiers);
            DeathHandler.Initialize(this, ChargeAttack, ProjectilePool, PlayerStatsSO, deathParticles);
            if(modifiers.ShockwaveCount > 0) Shockwave.Initialize(this, modifiers.ShockwaveCount);
            DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} initialized. IsAI: {isAI}");
        }

        public void DestroyVisibleProjectiles() => ProjectilePool.DestroyAllVisible();

        private void ApplyStatBuffs(int numberOfIncreasedHpPowerups, int numberOfIncreasedAttackSpeedPowerups) {
            this.maxHealth += (numberOfIncreasedHpPowerups * this.maxHealth/2f);
            this.currentHealth = this.maxHealth;
            for (int i = 0; i < numberOfIncreasedAttackSpeedPowerups; i++) {
                this.projectileShootRate *= 0.75f;
                this.projectileSpeed *= 1.25f;
            }

            if (numberOfIncreasedAttackSpeedPowerups > 0) showTrail = true;
        }

        public void EnableInput() {
            inputEnabled = true;
        }

        public void StartShooting() {
            if (shootingCoroutineInstance == null) {
                shootingCoroutineInstance = StartCoroutine(ShootingCoroutine());
            }
        }

        private void Update() {
            ChargeAttack.Tick();
            Shockwave.Tick();
        }

        private void FixedUpdate() {
            HandleInput();
        }

        private void HandleInput() {
            if (!inputEnabled && !isStunned) return;
            if (navigator == null) return;

            Vector2 input = navigator.GetNavigate();

            if (input.magnitude > 0.1f) {
                lastMoveDirection = input.normalized;
            }

            if (isStunned) {
                if (!isPlayingStunnedAnimation) {
                    isPlayingStunnedAnimation = true;
                    transform.DOPunchPosition(input.normalized * stunNudgeMultiplier, 0.1f).OnComplete(() =>
                    {
                        stunNudgeMultiplier *= 1.05f;
                        isPlayingStunnedAnimation = false;
                    });
                }
                return;
            }
            ApplyMovement(input);
        }

        private void ApplyMovement(Vector2 input) {
            float speedMultiplier = isGhostMode ? ghostMoveSpeedMultiplier : 1f;

            if (ChargeAttack.IsCharging) {
                speedMultiplier *= 0.7f;
            }

            Vector2 movement = input.normalized * (maxMoveSpeed * speedMultiplier * Time.fixedDeltaTime);
            Vector2 newPosition = Rigidbody2D.position + movement;

            CheckScreenBounds();
            newPosition.x = ClampXPosition(newPosition.x);
            newPosition.y = ClampYPosition(newPosition.y);

            Rigidbody2D.MovePosition(newPosition);
        }

        private void CheckScreenBounds() {
            if (Screen.width != originalScreenWidth || Screen.height != originalScreenHeight) {
                originalScreenWidth = Screen.width;
                originalScreenHeight = Screen.height;
                UpdateScreenBounds();
            } else if (DireDodgingCameraZoomService.ShockwaveZoomActive && !DireDodgingCameraZoomService.DeathZoomActive) {
                UpdateScreenBounds();
            }
        }

        private void UpdateScreenBounds() {
            if(mainCamera == null) mainCamera = Camera.main;
            screenBottom = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;
            screenTop = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
            screenLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            screenRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
        }

        private void ApplyBaseStats() {
            this.maxMoveSpeed = PlayerStatsSO.MoveSpeed;
            this.projectileScale = PlayerStatsSO.ProjectileScale;
            this.projectileSpeed = PlayerStatsSO.ProjectileSpeed;
            this.baseDamage = PlayerStatsSO.BaseDamage;
            this.maxHealth = PlayerStatsSO.BaseHealth;
            this.projectileShootRate = PlayerStatsSO.ProjectileShootRate;
            this.damageAnimationTimeInSeconds = PlayerStatsSO.DamageAnimationTimeInSeconds;
            this.ghostMoveSpeedMultiplier = PlayerStatsSO.GhostMoveSpeedMultiplier;
            this.defaultStunDuration = PlayerStatsSO.StunDuration;
            this.hitEvent = PlayerStatsSO.GetHitEvent;
            this.intensityStats = PlayerStatsSO.GetIntensityStats();
            currentHealth = maxHealth;
        }

        private IEnumerator ShootingCoroutine() {
            nextShootTime = 0f;
            while (inputEnabled && isAlive) {
                if (!ChargeAttack.IsCharging && Time.time >= nextShootTime)
                {
                    Shoot();
                    nextShootTime = Time.time + projectileShootRate;
                }
                yield return null;
            }
            shootingCoroutineInstance = null;
        }

        private void Shoot() {
            if (isGhostMode) return;

            Vector2 baseDirection = GetShootDirection();
            List<(Vector2 directions, float angle)> directions = GetShootDirections(baseDirection);
            Quaternion baseRotation = GetRotationForDirection(baseDirection);
        
            foreach (var (shootDirection, angleOffset) in directions) {

                var projectile = ProjectilePool.GetNormal();

                Vector2 spawnOffset = shootDirection * (spriteHalfWidth * 1.5f);
                projectile.transform.SetParent(null);
                projectile.transform.position = (Vector2)transform.position + spawnOffset;
                projectile.transform.rotation = baseRotation * Quaternion.Euler(0, 0, angleOffset);
                projectile.transform.localScale = Vector3.one * (projectileScale * 0.3f);
                projectile.Initialize(playerIndex, baseDamage, projectileSpeed, shootDirection, false, showTrail);
            }
            if (shootEventExists) {
                RuntimeManager.PlayOneShot(PlayerStatsSO.BasicShootEvent);
            }
        }

        private List<(Vector2 direction, float angle)> GetShootDirections(Vector2 baseDirection) {
            List<(Vector2 directions, float angle)> directions = new();
            directions.Add((baseDirection, 0f));
            for (int i = 0; i < multishotCount; i++) {
                float angle = PlayerStatsSO.MultishotSpreadAngle * (i + 1);
                directions.Add((Quaternion.Euler(0f, 0f, angle) * baseDirection, angle));
                directions.Add((Quaternion.Euler(0f, 0f, -angle) * baseDirection, -angle));
            }

            return directions;
        }

        public Vector2 GetShootDirection() {
            if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y)) {
                return lastMoveDirection.x > 0 ? Vector2.right : Vector2.left;
            } else {
                return lastMoveDirection.y > 0 ? Vector2.up : Vector2.down;
            }
        }

        public void SetAliveState(bool alive, bool ghostMode) {
            isAlive = alive;
            isGhostMode = ghostMode;
        }

        public void ResetHealth() {
            currentHealth = maxHealth;
        }

        public Quaternion GetRotationForDirection(Vector2 direction) {
            if (direction == Vector2.right) return rightRotation;
            if (direction == Vector2.left) return leftRotation;
            if (direction == Vector2.up) return upRotation;
            if (direction == Vector2.down) return downRotation;
            return rightRotation; // Default
        }

        private float ClampYPosition(float yPosition) {
            return Mathf.Clamp(yPosition, screenBottom + spriteHalfHeight, screenTop - spriteHalfHeight);
        }

        private float ClampXPosition(float xPosition) {
            return Mathf.Clamp(xPosition, screenLeft + spriteHalfWidth, screenRight - spriteHalfWidth);
        }

        public void Freeze() {
            inputEnabled = false;
            ClearStun();
            ChargeAttack.ForceStop();
            Shockwave.ForceStop();
        }
    
        public void TakeProjectileDamage(DireDodgingProjectile projectile) => HandleProjectileCollision(projectile);

        private void OnTriggerEnter2D(Collider2D other) {
            if (PlayerIsDead) return;
            DireDodgingProjectile projectile = other.GetComponent<DireDodgingProjectile>();
            if (projectile != null) {
                if (projectile.OwnerIndex == playerIndex) return;
                HandleProjectileCollision(projectile);
                projectile.ReturnToPool();
            }
        }

        private void HandleProjectileCollision(DireDodgingProjectile projectile) {
            if (!isAlive || isGhostMode || DeathHandler.IsInvincible) return;

            if (projectile.IsGhostProjectile) {
                Stun(defaultStunDuration);
                RuntimeManager.PlayOneShot(hitEvent);
            } else {
                TakeDamage(projectile);

                if (PlayerIsDead) {
                    DireDodgingMinigameManager.Instance.RegisterDeath(projectile.OwnerIndex, playerIndex);
                    DeathHandler.TriggerDeath();
                    return;
                } else {
                    RuntimeManager.PlayOneShot(hitEvent);
                    mainCamera.DOShakePosition(duration: 0.05f, strength: GameSettings.Accessibility.ScreenShakeIntensity*0.2f, vibrato: 1, randomness: 90f, fadeOut: false).SetUpdate(true);
                }

                if (damageCoroutineInstance != null) {
                    StopCoroutine(damageCoroutineInstance);
                }
                damageCoroutineInstance = StartCoroutine(DamageCoroutine());
            }
        }

        private void TakeDamage(DireDodgingProjectile projectile) {
            currentHealth -= projectile.Damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            HealthBar.UpdateDisplay(currentHealth, maxHealth);
        }

        private bool isStunned = false;
        private float nextShootTime;

        public void Stun(float stunDuration = -1f) {
            if (isStunned) return;
            StartCoroutine(StunCoroutine(stunDuration));
        }

        private IEnumerator StunCoroutine(float stunDuration = -1f) {
            if (Mathf.Approximately(stunDuration, -1f)) stunDuration = defaultStunDuration;
        
            stunNudgeMultiplier = 0.02f;
            isStunned = true;
            stunParticles.Play();
            StopShooting();
            ChargeAttack.ForceStop();
        
            StopColorChangeSequence();
            if (damageCoroutineInstance != null) {
                StopCoroutine(damageCoroutineInstance);
                damageCoroutineInstance = null;
            }
        
            originalSpeedBeforeStun = maxMoveSpeed;
            maxMoveSpeed = 0f;

            Color stunColor = new Color(1f, 0.7f, 0.2f, 1f) * baseColor;
            stunColor.a = 1f;
            stunColorTween = SpriteRenderer.DOColor(stunColor, 0.15f);

            stunShakeTween = transform.DOShakePosition(
                duration: stunDuration,
                strength: 0.05f,
                vibrato: 15,
                randomness: 90,
                fadeOut: false
            );

            yield return new WaitForSeconds(stunDuration);

            EndStun();
        }

        private void EndStun() {
            if (!isStunned) return;
            isStunned = false;
        
            if(stunColorTween != null && stunColorTween.IsActive()) stunColorTween.Kill();
            if(stunShakeTween != null && stunShakeTween.IsActive()) stunShakeTween.Kill();
            stunColorTween = null;
            stunShakeTween = null;

            transform.DOKill();

            maxMoveSpeed = originalSpeedBeforeStun;
            SpriteRenderer.color = baseColor;
        
            stunParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (isAlive) {
                StartShooting();
            }

            stunCoroutineInstance = null;
        }

        public void ClearStun() {
            if (!isStunned) return;

            if (stunCoroutineInstance != null) {
                StopCoroutine(stunCoroutineInstance);
                stunCoroutineInstance = null;
            }
        
            EndStun();
        }

        private IEnumerator DamageCoroutine() {
            UnityEngine.Debug.Log($"P{playerIndex+1} took damage!");
            var fadeInTween = SpriteRenderer.DOColor(Color.red * baseColor, damageAnimationTimeInSeconds / 2f);
            var fadeOutTween = SpriteRenderer.DOColor(baseColor, damageAnimationTimeInSeconds / 2f);
            colorChangeSequence = DOTween.Sequence();
            colorChangeSequence.Append(fadeInTween);
            colorChangeSequence.Append(fadeOutTween);
            colorChangeSequence.onComplete = ResetColorChangeSequence;
            yield return new DOTweenCYInstruction.WaitForKill(fadeOutTween);
            damageCoroutineInstance = null;
        }

        private void ResetColorChangeSequence() {
            colorChangeSequence = null;
        }
    
        public void StopColorChangeSequence() {
            if(colorChangeSequence != null) colorChangeSequence.Kill();
        }
    
        private void StopIntensityCoroutine() {
            if (intensityCoroutineInstance != null) {
                StopCoroutine(intensityCoroutineInstance);
                intensityCoroutineInstance = null;
            }
        }
    
        public void StopShooting() {
            if (shootingCoroutineInstance != null) {
                StopCoroutine(shootingCoroutineInstance);
                shootingCoroutineInstance = null;
            }
        }
    
        private bool PlayerIsDead => currentHealth <= 0;

        public void StartIncreasingIntensity(float remainingTimeInSeconds) {
            intensityCoroutineInstance = StartCoroutine(IntensityCoroutine(remainingTimeInSeconds));
        }

        private IEnumerator IntensityCoroutine(float remainingTimeInSeconds) {
            float startTime = Time.time;
            float duration = remainingTimeInSeconds - PlayerStatsSO.TimeAtMaxIntensityInSeconds;
            float initialProjectileSpeed = projectileSpeed;
            float initialShootRate = projectileShootRate;
            float initialProjectileScale = projectileScale;

            float targetProjectileSpeed = initialProjectileSpeed * intensityStats.ProjectileSpeedIncrease;
            float targetShootRate = initialShootRate * intensityStats.ShootRateDivisor;
            float targetProjectileScale = projectileScale * intensityStats.ProjectileScaleIncrease;

            while (Time.time - startTime < duration) {
                float elapsed = Time.time - startTime;
                float t = elapsed / duration;
                float easedT = t * t;
                projectileSpeed = Mathf.Lerp(initialProjectileSpeed, targetProjectileSpeed, easedT);
                projectileShootRate = Mathf.Lerp(initialShootRate, targetShootRate, easedT);
                projectileScale = Mathf.Lerp(initialProjectileScale, targetProjectileScale, easedT);
                ChargeAttack.UpdateChargeTimeRequired(t * intensityStats.ChargeTimeDecrease);
                yield return null;
            }

            projectileSpeed = targetProjectileSpeed;
            projectileShootRate = targetShootRate;
            projectileScale = targetProjectileScale;
            intensityCoroutineInstance = null;
        }

        private void OnDestroy() {
            ChargeAttack.Cleanup();
            Shockwave.Cleanup();
            DireDodgingCameraZoomService.OnShockwaveZoomStatusChange -= OnShockwaveZoomStatusChange;
        }

        public void ResetShootCooldown() {
            nextShootTime = Time.time + projectileShootRate;
        }
    }
}
