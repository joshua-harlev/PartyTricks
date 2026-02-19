using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Pool;

public class DireDodgingPlayer : MonoBehaviour
{
    private static bool isDeathZoomActive = false;
    private static float trueOriginalCameraSize;
    private static Vector3 trueOriginalCameraPosition;
    
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
    private float deathAnimationTimeInSeconds;
    private const float cameraFreezeDuration = 1f;
    private const float cameraZoomAmount = 0.7f;

    [SerializeField] private DireDodgingPlayerStatsSO PlayerStatsSO;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private Collider2D Collider2D;
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private Color PlayerColor;
    [SerializeField] private GameObject ProjectilePrefab;
    [SerializeField] private Transform PoolParent;
    [SerializeField] private DireDodgingHealthBar HealthBar;
    [SerializeField] private SpriteRenderer chargeIndicator;
    [SerializeField] private ParticleSystem chargeParticles;
    
    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private Coroutine shootingCoroutineInstance = null;
    private Vector2 lastMoveDirection = Vector2.right;
    
    private int playerIndex;
    private Sequence colorChangeSequence;
    private Color baseColor;
    private IDirectionalTwoButtonInputHandler navigator;
    private bool isAI;
    private bool inputEnabled;
    private bool isAlive = true;
    private ObjectPool<DireDodgingProjectile>[] projectilePools;
    private List<DireDodgingProjectile> activeProjectiles = new();
    
    private bool isGhostMode = false;
    private float respawnDelay = 3f;
    private bool isInvincible = false;
    private float invincibilityDuration = 2f;
    
    private Tween cameraZoomTween;


    private Coroutine damageCoroutineInstance = null;
    private Coroutine intensityCoroutineInstance = null;
    private Camera mainCamera;
    private readonly Quaternion leftRotation = Quaternion.Euler(0, 0, 90);
    private readonly Quaternion rightRotation = Quaternion.Euler(0, 0, 270);
    private readonly Quaternion upRotation = Quaternion.Euler(0, 0, 0);
    private readonly Quaternion downRotation = Quaternion.Euler(0, 0, 180);
    private EventReference hitEvent;
    private EventReference deathEvent;
    private EventReference chargeLoopEvent; 
    private EventReference chargeReleaseEvent;
    private EventReference chargeShootEvent;
    private EventInstance chargeLoopInstance;
    
    private float chargeTimeRequired;
    private float chargedProjectileScale;
    private float chargedProjectileSpeed;
    private float ghostChargeTime;
    private float ghostProjectileSpeed;
    private float ghostMoveSpeedMultiplier;
    private float stunDuration;
    
    
    private void Awake() {
        baseColor = SpriteRenderer.color;
    }

    public void Initialize(int index, IDirectionalTwoButtonInputHandler inputHandler, bool initializeAsAI, int numberOfIncreasedHealthPowerups, int numberOfIncreasedAttackSpeedPowerups, bool isDoubleRound) {
        mainCamera = Camera.main;
        if (!isDeathZoomActive) {
            trueOriginalCameraSize = mainCamera.orthographicSize;
            trueOriginalCameraPosition = mainCamera.transform.position;
        }
        ApplyBaseStats();
        if (isDoubleRound) {
            this.maxHealth *= 2;
            this.currentHealth *= 2;
        }
        ApplyStatBuffs(numberOfIncreasedHealthPowerups, numberOfIncreasedAttackSpeedPowerups);
        this.playerIndex = index;
        this.navigator = inputHandler;
        this.isAI = initializeAsAI;
        this.inputEnabled = false;
        spriteHalfWidth = SpriteRenderer.bounds.size.x / 2f;
        
        // TODO calculate this more effectively
        spriteHalfHeight = SpriteRenderer.bounds.extents.y + 0.4f; // offset added for health bar
        
        InitializePools();
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} initialized. IsAI: {isAI}");
    }

    private void ApplyStatBuffs(int numberOfIncreasedHpPowerups, int numberOfIncreasedAttackSpeedPowerups) {
        this.maxHealth += numberOfIncreasedHpPowerups;
        this.currentHealth = this.maxHealth;
        for (int i = 0; i < numberOfIncreasedAttackSpeedPowerups; i++) {
            this.projectileShootRate *= 0.75f;
        }
    }

    private void InitializePools() {
        projectilePools = new ObjectPool<DireDodgingProjectile>[2];
        projectilePools[0] = new ObjectPool<DireDodgingProjectile>(
            () => CreateProjectile(projectilePools[0]),
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile
        );
        projectilePools[1] = new ObjectPool<DireDodgingProjectile>(
            () => CreateProjectile(projectilePools[1]),
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile
        );
    }
    private DireDodgingProjectile CreateProjectile(IObjectPool<DireDodgingProjectile> projectilePool) {
        GameObject projectileObject = Instantiate(ProjectilePrefab, PoolParent);
        projectileObject.SetActive(false);
        DireDodgingProjectile projectile = projectileObject.GetComponent<DireDodgingProjectile>();
        projectile.SetPool(projectilePool);
        projectile.SetColor(PlayerColor);
        return projectile;
    }

    private void OnGetProjectile(DireDodgingProjectile projectile) {
        projectile.gameObject.SetActive(true);
        activeProjectiles.Add(projectile);
    }

    private void OnReleaseProjectile(DireDodgingProjectile projectile) {
        projectile.gameObject.SetActive(false);
        activeProjectiles.Remove(projectile);
    }

    private void OnDestroyProjectile(DireDodgingProjectile projectile) {
        Destroy(projectile.gameObject);
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
        HandleCharging(); 
        UpdateChargeIndicator();
        UpdateChargeParticleDirection();
    }
    
    private void UpdateChargeParticleDirection() {
        if (!isCharging || chargeParticles == null) return;
    
        Vector2 shootDirection = GetShootDirection();
        Vector2 particleOffset = shootDirection * 2f;
        chargeParticles.transform.localPosition = particleOffset;
    
        float angle = Mathf.Atan2(-shootDirection.y, -shootDirection.x) * Mathf.Rad2Deg;
        chargeParticles.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void FixedUpdate() {
        HandleInput();
    }
    
    private void UpdateChargeIndicator() {
        if (chargeIndicator == null) return;
    
        if (isCharging) {
            float chargeTime = Time.time - chargeStartTime;
            float timeRequiredToCharge;
            
            if (isGhostMode) {
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
    
    private void HandleCharging() {
        if (!inputEnabled) return;
        if (navigator == null) return;

        bool chargeHeld = navigator.ChargeIsHeld();

        if (chargeHeld && !isCharging) {
            StartCharging();
        }

        if (isCharging && !chargeHeld) {
            ReleaseCharge();
        }
    }
    
    
    private void StartCharging() {
        isCharging = true;
        chargeStartTime = Time.time;
    
        if (chargeParticles != null) {
            Vector2 shootDirection = GetShootDirection();
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
        float requiredTime = isGhostMode ? ghostChargeTime : chargeTimeRequired;
        
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
    
    private void ShootChargedProjectile() {
        Vector2 shootDirection = GetShootDirection();
    
        float damage;
        float speed;
    
        if (isGhostMode) {
            damage = 0f;
            speed = projectileSpeed * ghostProjectileSpeed;
        } else {
            damage = maxHealth * 10f;
            speed = projectileSpeed * chargedProjectileSpeed;
        }
        var projectile = projectilePools[1].Get();
    
        Vector2 spawnOffset = shootDirection * (spriteHalfWidth * 1.5f);
        projectile.transform.position = (Vector2)transform.position + spawnOffset;
    
        projectile.transform.rotation = GetRotationForDirection(shootDirection);
        projectile.transform.localScale = Vector3.one * projectileScale * chargedProjectileScale;
    
        projectile.Initialize(playerIndex, damage, speed, shootDirection, isGhostMode);
    }

    private void HandleInput() {
        if (!inputEnabled) return;
        if (navigator == null) return;
    
        Vector2 input = navigator.GetNavigate();
    
        if (input.magnitude > 0.1f) {   
            lastMoveDirection = input.normalized;
        }

        ApplyMovement(input);
    }

    private void ApplyMovement(Vector2 input) {
        float speedMultiplier = isGhostMode ? ghostMoveSpeedMultiplier : 1f;

        if (isCharging) {
            speedMultiplier *= 0.7f;
        }
        
        Vector2 movement = input.normalized * (maxMoveSpeed * speedMultiplier * Time.fixedDeltaTime);
        Vector2 newPosition = Rigidbody2D.position + movement;

        newPosition.x = ClampXPosition(newPosition.x);
        newPosition.y = ClampYPosition(newPosition.y);

        Rigidbody2D.MovePosition(newPosition);
    }
    
    private void ApplyBaseStats() {
        this.maxMoveSpeed = PlayerStatsSO.MoveSpeed;
        this.projectileScale = PlayerStatsSO.ProjectileScale * 0.36f;
        this.projectileSpeed = PlayerStatsSO.ProjectileSpeed;
        this.baseDamage = PlayerStatsSO.BaseDamage;
        this.maxHealth = PlayerStatsSO.BaseHealth;
        this.projectileShootRate = PlayerStatsSO.ProjectileShootRate;
        this.damageAnimationTimeInSeconds = PlayerStatsSO.DamageAnimationTimeInSeconds;
        this.deathAnimationTimeInSeconds = PlayerStatsSO.DeathAnimationTimeInSeconds;
    
        this.chargeTimeRequired = PlayerStatsSO.ChargeTimeRequired;
        this.chargedProjectileScale = PlayerStatsSO.ChargedProjectileScale;
        this.chargedProjectileSpeed = PlayerStatsSO.ChargedProjectileSpeed;
    
        this.ghostChargeTime = PlayerStatsSO.GhostChargeTime;
        this.ghostProjectileSpeed = PlayerStatsSO.GhostProjectileSpeed;
        this.ghostMoveSpeedMultiplier = PlayerStatsSO.GhostMoveSpeedMultiplier;
    
        this.stunDuration = PlayerStatsSO.StunDuration;
    
        this.hitEvent = PlayerStatsSO.GetHitEvent;
        this.deathEvent = PlayerStatsSO.DeathEvent;
        this.chargeLoopEvent = PlayerStatsSO.ChargeLoopEvent;
        this.chargeReleaseEvent = PlayerStatsSO.ChargeReleaseEvent;
        this.chargeShootEvent = PlayerStatsSO.ChargeShootEvent;
    
        currentHealth = maxHealth;
    }

    private IEnumerator ShootingCoroutine() {
        float nextShootTime = 0f;
        while (inputEnabled && isAlive) {
            if (!isCharging && Time.time >= nextShootTime)
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
    
        Vector2 shootDirection = GetShootDirection();
    
        var projectile = projectilePools[0].Get();
    
        Vector2 spawnOffset = shootDirection * (spriteHalfWidth * 1.5f);
        projectile.transform.position = (Vector2)transform.position + spawnOffset;
    
        projectile.transform.rotation = GetRotationForDirection(shootDirection);
        projectile.transform.localScale = Vector3.one * projectileScale;
        projectile.Initialize(playerIndex, baseDamage, projectileSpeed, shootDirection, false);
    }
    
    private Vector2 GetShootDirection() {
        if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y)) {
            return lastMoveDirection.x > 0 ? Vector2.right : Vector2.left;
        } else {
            return lastMoveDirection.y > 0 ? Vector2.up : Vector2.down;
        }
    }

    private Quaternion GetRotationForDirection(Vector2 direction) {
        if (direction == Vector2.right) return rightRotation;
        if (direction == Vector2.left) return leftRotation;
        if (direction == Vector2.up) return upRotation;
        if (direction == Vector2.down) return downRotation;
        return rightRotation; // Default
    }
    
    private float ClampYPosition(float yPosition) {
        float screenBottom = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;
        float screenTop = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
        return Mathf.Clamp(yPosition, screenBottom + spriteHalfHeight, screenTop - spriteHalfHeight);
    }

    private float ClampXPosition(float xPosition) {
        float screenLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        float screenRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
        return Mathf.Clamp(xPosition, screenLeft + spriteHalfWidth, screenRight - spriteHalfWidth);
    }

    public void Freeze() {
        inputEnabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject other = collision.gameObject;
        if (PlayerIsDead) return;
        DireDodgingProjectile projectile = other.GetComponent<DireDodgingProjectile>();
        if (projectile != null) {
            if (projectile.OwnerIndex == playerIndex) return;
            HandleProjectileCollision(projectile);
            projectile.ReturnToPool();
        }
    }

    private void HandleProjectileCollision(DireDodgingProjectile projectile) {
        if (!isAlive || isGhostMode || isInvincible) return;
    
        if (projectile.IsGhostProjectile) {
            StartCoroutine(StunCoroutine());
            RuntimeManager.PlayOneShot(hitEvent);
        } else {
            TakeDamage(projectile);

            if (PlayerIsDead) {
                DireDodgingMinigameManager.Instance.RegisterDeath(projectile.OwnerIndex, playerIndex);
                Die();
                return;
            } else {
                RuntimeManager.PlayOneShot(hitEvent);
                mainCamera.DOShakePosition(duration: 0.05f, strength: 0.2f, vibrato: 1, randomness: 90f, fadeOut: false).SetUpdate(true);
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

    private IEnumerator StunCoroutine() {
        if (isStunned) yield break;
    
        isStunned = true;
        float originalSpeed = maxMoveSpeed;
        maxMoveSpeed = 0f;
        
        Color originalColor = SpriteRenderer.color;
        SpriteRenderer.color = new Color(0.5f, 0f, 0.5f, 1f);
    
        yield return new WaitForSeconds(stunDuration);
    
        maxMoveSpeed = originalSpeed;
        SpriteRenderer.color = originalColor;
        isStunned = false;
    }

    private IEnumerator DamageCoroutine() {
        Debug.Log($"P{playerIndex+1} took damage!");
        var fadeInTween = SpriteRenderer.DOColor(Color.white, damageAnimationTimeInSeconds / 2f);
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

    private void Die() {
        isAlive = false;
        isGhostMode = true;
        
        StopRigidbodyMotion();
        Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
    
        // TODO fix timescale not working apparently
        Time.timeScale = 0f; 
        
        ZoomCameraOnDeath();
        StopChargeEffects();
        
        // TODO re-examine intensity system
        // StopIntensityCoroutine();
        StopColorChangeSequence();
        TransitionSpriteOpacityOnDeath();
        
        RuntimeManager.PlayOneShot(deathEvent);
        StartCoroutine(DeathCoroutine());
    }

    private void TransitionSpriteOpacityOnDeath() {
        var color = baseColor;
        color.a = 0.1f;
        SpriteRenderer.DOColor(color, deathAnimationTimeInSeconds).SetUpdate(true);
    }

    private void StopColorChangeSequence() {
        if(colorChangeSequence != null) colorChangeSequence.Kill();
    }

    private void StopRigidbodyMotion() {
        Rigidbody2D.linearVelocity = Vector2.zero;
        Rigidbody2D.angularVelocity = 0f;
    }

    private void StopIntensityCoroutine() {
        if (intensityCoroutineInstance != null) {
            StopCoroutine(intensityCoroutineInstance);
            intensityCoroutineInstance = null;
        }
    }

    private void StopChargeEffects() {
        if (isCharging) {
            if (chargeParticles != null) {
                chargeParticles.Stop();
            }
            if (chargeLoopInstance.isValid()) {
                chargeLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                chargeLoopInstance.release();
            }
        }
    }

    private void ZoomCameraOnDeath() {
        if (mainCamera == null) {
            throw new MissingComponentException("Main Camera is missing.");
            return;
        }
        KillCameraTweens();
        DoCameraZoomSequence();
    }

    private void DoCameraZoomSequence() {
        isDeathZoomActive = true;
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, trueOriginalCameraPosition.z);
        mainCamera.DOOrthoSize(trueOriginalCameraSize * cameraZoomAmount, cameraFreezeDuration * 0.5f).SetUpdate(true);
        mainCamera.transform.DOMove(targetPosition, cameraFreezeDuration * 0.5f).SetUpdate(true);

        cameraZoomTween = DOVirtual.DelayedCall(cameraFreezeDuration, () => {
            ReturnCameraToOriginalState();
            Time.timeScale = 1f;
            DireDodgingMinigameManager.Instance.EnableAllPlayerInput();
        }, false).SetUpdate(true);
    }

    private void ReturnCameraToOriginalState() {
        mainCamera.DOOrthoSize(trueOriginalCameraSize, 0.3f).SetUpdate(true);
        mainCamera.transform.DOMove(trueOriginalCameraPosition, 0.3f).SetUpdate(true).OnComplete(() => {
            isDeathZoomActive = false;
        });
    }

    private void KillCameraTweens() {
        mainCamera.DOKill();
        mainCamera.transform.DOKill();
    }

    private IEnumerator DeathCoroutine() {
        yield return new WaitForSeconds(deathAnimationTimeInSeconds);
        
        inputEnabled = true;
        
        UpdateSpriteToTransparent();
        ReturnProjectilesToPool();
        StopShootingCoroutine();
        HideHealthBar();
        
        yield return new WaitForSeconds(respawnDelay);

        Respawn();
    }

    private void HideHealthBar() {
        if (HealthBar != null) {
            HealthBar.gameObject.SetActive(false);
        }
    }

    private void StopShootingCoroutine() {
        if (shootingCoroutineInstance != null) {
            StopCoroutine(shootingCoroutineInstance);
            shootingCoroutineInstance = null;
        }
    }

    private void ReturnProjectilesToPool() {
        foreach (var projectile in activeProjectiles.ToArray()) {
            projectile.ReturnToPool();
        }
    }

    private void UpdateSpriteToTransparent() {
        Color ghostColor = baseColor;
        ghostColor.a = 0.3f;
        SpriteRenderer.color = ghostColor;
    }

    private void Respawn() {
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
        
        isAlive = true;
        isGhostMode = false;
        currentHealth = maxHealth;
        
        Rigidbody2D.linearVelocity = Vector2.zero;
        Rigidbody2D.angularVelocity = 0f;
        
        Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
    
        Collider2D.enabled = true;
    
        Color aliveColor = baseColor;
        aliveColor.a = 1f;
        SpriteRenderer.color = aliveColor;
    
        // Show health bar
        if (HealthBar != null) {
            HealthBar.gameObject.SetActive(true);
            HealthBar.UpdateDisplay(currentHealth, maxHealth);
        }
    
        StartShooting();
    
        StartCoroutine(RespawnInvincibilityCoroutine());
    }
    
    private IEnumerator RespawnInvincibilityCoroutine() {
        isInvincible = true;
    
        float flashInterval = 0.1f;
        float elapsed = 0f;
    
        while (elapsed < invincibilityDuration) {
            SpriteRenderer.enabled = !SpriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
    
        SpriteRenderer.enabled = true;
        isInvincible = false;
    }

    private void DisableColliderComponent() {
        Collider2D.enabled = false;
    }

    private bool PlayerIsDead => currentHealth <= 0;

    public void DestroyVisibleProjectiles() {
        var projectilesToDestroy = new List<DireDodgingProjectile>(activeProjectiles);
        foreach (var projectile in projectilesToDestroy) {
            Destroy(projectile.gameObject);
        }
    }

    public void StartIncreasingIntensity(int remainingTimeInSeconds) {
        intensityCoroutineInstance = StartCoroutine(IntensityCoroutine(remainingTimeInSeconds));
    }

    private IEnumerator IntensityCoroutine(int remainingTimeInSeconds) {
        float startTime = Time.time;
        float timeAtFullyRampedUpSpeed = 5f;
        float duration = remainingTimeInSeconds - timeAtFullyRampedUpSpeed;
        float initialProjectileSpeed = projectileSpeed;
        float initialShootRate = projectileShootRate;
        float initialProjectileScale = projectileScale;

        float targetProjectileSpeed = initialProjectileSpeed * 2.5f;
        float targetShootRate = initialShootRate * 0.3f;
        float targetProjectileScale = projectileScale * 2f;

        while (Time.time - startTime < duration) {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration;
            float easedT = t * t;
            projectileSpeed = Mathf.Lerp(initialProjectileSpeed, targetProjectileSpeed, easedT);
            projectileShootRate = Mathf.Lerp(initialShootRate, targetShootRate, easedT);
            projectileScale = Mathf.Lerp(initialProjectileScale, targetProjectileScale, easedT);
            yield return null;
        }

        projectileSpeed = targetProjectileSpeed;
        projectileShootRate = targetShootRate;
        projectileScale = targetProjectileScale;
        intensityCoroutineInstance = null;
    }
    
    private void OnDestroy() {
        if (chargeLoopInstance.isValid()) {
            chargeLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            chargeLoopInstance.release();
        }
        isDeathZoomActive = false;
    }
}
