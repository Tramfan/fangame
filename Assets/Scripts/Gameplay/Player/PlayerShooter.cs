
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerShooter : MonoBehaviour
{
    [SerializeField]
    private PlayerInputSource inputSource;

    [SerializeField]
    private Transform firePoint;

    [Header("Legacy Fallback")]
    [SerializeField]
    private PlayerBullet bulletPrefab;

    [SerializeField, Min(1)]
    private int fireIntervalTicks = 6;

    [SerializeField]
    private bool shootingEnabled = true;

    private PlayerShotPatternDefinition unfocusedPattern;
    private PlayerShotPatternDefinition focusedPattern;
    private PlayerShotPatternDefinition activePattern;

private int patternFireTick;

    private bool usesPattern;
    private bool shootHeld;
    private bool focusHeld;
    private int legacyCooldownTicks;

    private void Awake()
    {
        if (inputSource == null)
        {
            inputSource =
                GetComponentInParent<PlayerInputSource>();
        }

        if (inputSource == null)
        {
            Debug.LogError(
                "Player Shooter has no Input Source.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (inputSource == null)
        {
            shootHeld = false;
            focusHeld = false;
            return;
        }

        shootHeld =
            shootingEnabled &&
            inputSource.ShootHeld;

        focusHeld = inputSource.FocusHeld;
    }

    private void FixedUpdate()
    {
        if (!shootHeld)
        {
            ResetFiringState();
            return;
        }

        if (usesPattern)
        {
            UpdateActivePattern();
            FirePatternTick();
            return;
        }

        FireLegacyTick();
    }

    private void UpdateActivePattern()
    {
        PlayerShotPatternDefinition wantedPattern =
            focusHeld && focusedPattern != null
                ? focusedPattern
                : unfocusedPattern;

        if (wantedPattern == activePattern)
        {
            return;
        }

        SetActivePattern(wantedPattern);
    }

    private void SetActivePattern(
    PlayerShotPatternDefinition pattern
)
{
    activePattern = pattern;
}

 private void FirePatternTick()
{
    if (activePattern == null) return;

    PlayerShotPatternDefinition.Emitter[] emitters =
        activePattern.Emitters;

    if (emitters == null) return;

    for (int index = 0; index < emitters.Length; index++)
    {
        PlayerShotPatternDefinition.Emitter emitter =
            emitters[index];

        if (emitter == null) continue;

        int firstShotTick = emitter.FirstShotDelayTicks;
        int interval = Mathf.Max(
            1,
            emitter.FireIntervalTicks
        );

        if (patternFireTick < firstShotTick) continue;

        int emitterTick =
            patternFireTick - firstShotTick;

        if (emitterTick % interval != 0) continue;

        if (!FireEmitter(emitter)) return;
    }

    patternFireTick++;
}
    private bool FireEmitter(
        PlayerShotPatternDefinition.Emitter emitter
    )
    {
        if (emitter.ProjectilePrefab == null)
        {
            Debug.LogError(
                "Player shot emitter has no " +
                "projectile prefab.",
                activePattern
            );

            SetShootingEnabled(false);
            return false;
        }

        Transform origin =
            firePoint != null
                ? firePoint
                : transform;

        Vector3 spawnPosition =
            origin.TransformPoint(
                emitter.LocalOffset
            );

        int projectileCount =
            Mathf.Max(1, emitter.ProjectileCount);

        for (int index = 0;
             index < projectileCount;
             index++)
        {
            float spreadPosition =
                projectileCount == 1
                    ? 0.5f
                    : index /
                      (float)(projectileCount - 1);

            float angleDegrees =
                emitter.AngleOffsetDegrees +
                Mathf.Lerp(
                    -emitter.SpreadDegrees * 0.5f,
                    emitter.SpreadDegrees * 0.5f,
                    spreadPosition
                );

            Vector2 localDirection =
                RotateVector(
                    Vector2.up,
                    angleDegrees
                );

            Vector2 worldDirection =
                origin.TransformDirection(
                    localDirection
                );

            GameObject projectileObject =
                Instantiate(
                    emitter.ProjectilePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            IPlayerProjectile projectile =
                projectileObject
                    .GetComponent<IPlayerProjectile>();

            if (projectile == null)
            {
                Debug.LogError(
                    $"Projectile prefab " +
                    $"{emitter.ProjectilePrefab.name} " +
                    $"does not implement " +
                    $"{nameof(IPlayerProjectile)}.",
                    emitter.ProjectilePrefab
                );

                Destroy(projectileObject);
                SetShootingEnabled(false);
                return false;
            }

            projectile.Initialize(
                worldDirection,
                emitter.ProjectileSpeed,
                emitter.ProjectileDamage
            );
        }

        return true;
    }

    private void FireLegacyTick()
    {
        if (legacyCooldownTicks > 0)
        {
            legacyCooldownTicks--;
        }

        if (legacyCooldownTicks > 0)
        {
            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogError(
                "Player bullet prefab is not assigned.",
                this
            );

            SetShootingEnabled(false);
            return;
        }

        Vector3 spawnPosition =
            firePoint != null
                ? firePoint.position
                : transform.position;

        Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.identity
        );

        legacyCooldownTicks =
            Mathf.Max(1, fireIntervalTicks);
    }

 private void ResetFiringState()
{
    legacyCooldownTicks = 0;
    patternFireTick = 0;
    activePattern = null;
}

    private void OnDisable()
    {
        shootHeld = false;
        focusHeld = false;

        ResetFiringState();
    }

    public void Configure(
        PlayerBullet newBulletPrefab,
        int newFireIntervalTicks,
        bool enableShooting
    )
    {
        usesPattern = false;

        bulletPrefab = newBulletPrefab;

        fireIntervalTicks =
            Mathf.Max(1, newFireIntervalTicks);

        unfocusedPattern = null;
        focusedPattern = null;

        ResetFiringState();
        SetShootingEnabled(enableShooting);
    }

    public void Configure(
        PlayerShotPatternDefinition newUnfocusedPattern,
        PlayerShotPatternDefinition newFocusedPattern,
        bool enableShooting
    )
    {
        usesPattern = true;

        unfocusedPattern = newUnfocusedPattern;

        focusedPattern =
            newFocusedPattern != null
                ? newFocusedPattern
                : newUnfocusedPattern;

        ResetFiringState();

        SetShootingEnabled(
            enableShooting &&
            unfocusedPattern != null
        );
    }

    public void SetShootingEnabled(bool value)
    {
        shootingEnabled = value;

        if (!value)
        {
            shootHeld = false;
            ResetFiringState();
        }
    }

    private static Vector2 RotateVector(
        Vector2 vector,
        float degrees
    )
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sine = Mathf.Sin(radians);
        float cosine = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cosine -
            vector.y * sine,
            vector.x * sine +
            vector.y * cosine
        );
    }
}