using System.Collections;
using CoreData;
using DG.Tweening;
using FMODUnity;
using Minigames.DireDodging;
using UnityEngine;

public class DireDodgingPlayer : MonoBehaviour {
    public bool InputEnabled => inputEnabled;
    public IDirectionalTwoButtonInputHandler Navigator => navigator;
    public bool IsGhostMode => isGhostMode;
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

    [SerializeField] private DireDodgingPlayerStatsSO PlayerStatsSO;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private Collider2D Collider2D;
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private DireDodgingHealthBar HealthBar;
    [SerializeField] private DireDodgingProjectilePool ProjectilePool;
    [SerializeField] private DireDodgingChargeAttack ChargeAttack;
    [SerializeField] private DireDodgingDeathHandler DeathHandler;

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


    private Coroutine damageCoroutineInstance = null;
    private Coroutine intensityCoroutineInstance = null;
    private Camera mainCamera;
    private readonly Quaternion leftRotation = Quaternion.Euler(0, 0, 90);
    private readonly Quaternion rightRotation = Quaternion.Euler(0, 0, 270);
    private readonly Quaternion upRotation = Quaternion.Euler(0, 0, 0);
    private readonly Quaternion downRotation = Quaternion.Euler(0, 0, 180);
    private EventReference hitEvent;

    private float ghostMoveSpeedMultiplier;
    private float defaultStunDuration;

    private void Awake() {
        baseColor = SpriteRenderer.color;
    }

    public void Initialize(int index, IDirectionalTwoButtonInputHandler inputHandler, bool initializeAsAI, CombatModifiers modifiers, bool isDoubleRound) {
        mainCamera = Camera.main;
        DireDodgingDeathHandler.CaptureOriginalCamera(mainCamera);
        ApplyBaseStats();
        if (isDoubleRound) {
            this.maxHealth *= 2;
            this.currentHealth *= 2;
        }
        ApplyStatBuffs(modifiers.IncreasedHPCount, modifiers.IncreasedAttackSpeedCount);
        this.playerIndex = index;
        this.navigator = inputHandler;
        this.isAI = initializeAsAI;
        this.inputEnabled = false;
        spriteHalfWidth = SpriteRenderer.bounds.size.x / 2f;

        // TODO calculate this more effectively
        spriteHalfHeight = SpriteRenderer.bounds.extents.y + 0.4f; // offset added for health bar

        shootEventExists = !PlayerStatsSO.BasicShootEvent.IsNull;
        
        ProjectilePool.Initialize();
        ChargeAttack.Initialize(this, ProjectilePool, PlayerStatsSO, modifiers);
        DeathHandler.Initialize(this, ChargeAttack, ProjectilePool, PlayerStatsSO);
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} initialized. IsAI: {isAI}");
    }

    public void DestroyVisibleProjectiles() => ProjectilePool.DestroyAllVisible();

    private void ApplyStatBuffs(int numberOfIncreasedHpPowerups, int numberOfIncreasedAttackSpeedPowerups) {
        this.maxHealth += (numberOfIncreasedHpPowerups * this.maxHealth/2f);
        this.currentHealth = this.maxHealth;
        for (int i = 0; i < numberOfIncreasedAttackSpeedPowerups; i++) {
            this.projectileShootRate *= 0.75f;
        }
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
    }

    private void FixedUpdate() {
        HandleInput();
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

        if (ChargeAttack.IsCharging) {
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

        Vector2 shootDirection = GetShootDirection();

        var projectile = ProjectilePool.GetNormal();

        Vector2 spawnOffset = shootDirection * (spriteHalfWidth * 1.5f);
        projectile.transform.position = (Vector2)transform.position + spawnOffset;

        projectile.transform.rotation = GetRotationForDirection(shootDirection);
        projectile.transform.localScale = Vector3.one * projectileScale;
        projectile.Initialize(playerIndex, baseDamage, projectileSpeed, shootDirection, false);
        if (shootEventExists) {
            RuntimeManager.PlayOneShot(PlayerStatsSO.BasicShootEvent);
        }
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
        ChargeAttack.ForceStop();
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
    private float nextShootTime;

    public void Stun(float stunDuration = -1f) {
        StartCoroutine(StunCoroutine(stunDuration));
    }

    private IEnumerator StunCoroutine(float stunDuration = -1f) {
        if (isStunned) yield break;

        if (Mathf.Approximately(stunDuration, -1f)) stunDuration = defaultStunDuration;
        isStunned = true;
        StopShooting();
        ChargeAttack.ForceStop();
        float originalSpeed = maxMoveSpeed;
        maxMoveSpeed = 0f;

        Color originalColor = SpriteRenderer.color;
        SpriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);

        yield return new WaitForSeconds(stunDuration);
        StartShooting();

        maxMoveSpeed = originalSpeed;
        SpriteRenderer.color = originalColor;
        isStunned = false;
    }

    private IEnumerator DamageCoroutine() {
        Debug.Log($"P{playerIndex+1} took damage!");
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
        DeathHandler.Cleanup();
    }

    public void ResetShootCooldown() {
        nextShootTime = Time.time + projectileShootRate;
    }
}
