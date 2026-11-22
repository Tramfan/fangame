using UnityEngine;

public sealed partial class Bullet
{
    private int spawnTick;
    private Vector2 spawnPosition;
    private Vector2 spawnDirection;
    private string spawnSourceName;

    private bool hasGrantedGraze;
    private bool hasHitPlayer;
    private bool wasInsideGrazeArea;
    private bool ignoreGrazeUntilExit;
    private bool grazedBeforePatternRelease;

    private void UpdateGrazeState(
        Vector2 bulletCenter,
        PlayerArea grazeArea,
        bool shieldActive
    )
    {
        if (shieldActive || grazeArea == null || !grazeArea.isActiveAndEnabled)
        {
            if (wasInsideGrazeArea)
            {
                OnGrazeCaptureExited();
            }

            wasInsideGrazeArea = false;
            ignoreGrazeUntilExit = false;
            return;
        }

        bool isInsideGrazeArea = grazeArea.OverlapsCircle(
            bulletCenter,
            cachedHitboxRadius
        );

        if (ignoreGrazeUntilExit)
        {
            if (!isInsideGrazeArea)
            {
                ignoreGrazeUntilExit = false;
                wasInsideGrazeArea = false;
            }

            return;
        }

        if (isInsideGrazeArea && !wasInsideGrazeArea)
        {
            wasInsideGrazeArea = true;
            OnGrazeCaptureEntered(grazeArea.ReflectionEnabled);

            if (!hasGrantedGraze && grazeArea.OwnerState != null)
            {
                hasGrantedGraze = true;
                grazeArea.OwnerState.RegisterGraze();
            }
        }
        else if (!isInsideGrazeArea && wasInsideGrazeArea)
        {
            wasInsideGrazeArea = false;
            OnGrazeCaptureExited();

            if (grazeArea.ReflectionEnabled)
            {
                if (IsPatternControlled)
                {
                    grazedBeforePatternRelease = true;
                }
                else
                {
                    Reflect(grazeArea.Center);
                }
            }
        }
    }

    private void EvaluateCollisions()
    {
        if (!CollisionEnabled)
        {
            return;
        }

        Vector2 bulletCenter = position + cachedHitboxOffset;

        if (owner == BulletOwner.Enemy)
        {
            if (hasHitPlayer)
            {
                return;
            }

            PlayerArea hitboxArea = PlayerArea.HitboxArea;
            PlayerArea grazeArea = PlayerArea.GrazeArea;

            PlayerShield shield = hitboxArea != null
                ? hitboxArea.OwnerShield
                : (grazeArea != null ? grazeArea.OwnerShield : null);

            bool shieldActive = shield != null && shield.IsActive;

            // 1. Проверка поглощения щитом.
            if (shieldActive &&
                !IsPatternControlled &&
                BulletHitboxMath.OverlapsCircle(
                    bulletCenter,
                    cachedHitboxRadius,
                    shield.transform.position,
                    shield.ShieldWorldRadius
                ))
            {
                if (shield.TryAbsorb(shieldPowerCost))
                {
                    ReturnToPool();
                    return;
                }
            }

            // 2. Проверка попадания в хитбокс игрока.
            if (!shieldActive &&
                hitboxArea != null &&
                BulletHitboxMath.OverlapsCircle(
                    bulletCenter,
                    cachedHitboxRadius,
                    hitboxArea.Center,
                    hitboxArea.WorldRadius
                ))
            {
                hasHitPlayer = true;

                if (hitboxArea.OwnerState != null)
                {
                    hitboxArea.OwnerState.TakeHit();
                }

                ReturnToPool();
                return;
            }

            UpdateGrazeState(bulletCenter, grazeArea, shieldActive);
            return;
        }

        // Проверяем путь отражённой пули между предыдущей и текущей позициями.
        Vector2 prevCenter = previousPosition + cachedHitboxOffset;

        EnemyHealth hitEnemy = EnemyTargetRegistry.FindFirstHit(
            prevCenter,
            bulletCenter,
            cachedHitboxRadius
        );

        if (hitEnemy != null)
        {
            hitEnemy.TakeDamage(reflectedDamage);
            ReturnToPool();
        }
    }

    private void LogGrazeDiagnostic(
        PlayerArea grazeArea,
        Vector2 bulletPosition
    )
    {
        PlayerInputSource inputSource =
            grazeArea.GetComponentInParent<PlayerInputSource>();

        int grazeTick = inputSource != null
            ? inputSource.SimulationTick - 1
            : -1;

        string runKind =
            inputSource != null && inputSource.IsReplayInput
                ? "REPLAY"
                : "LIVE";

        Vector2 bulletCenter = GetBulletCenter(bulletPosition);

        float distance = Vector2.Distance(
            bulletCenter,
            grazeArea.Center
        );

        float overlapLimit =
            grazeArea.WorldRadius + GetBulletWorldRadius();

        Debug.Log(
            $"GrazeTrace {runKind}; " +
            $"tick {grazeTick}; " +
            $"bullet {transform.GetSiblingIndex()}; " +
            $"distance {distance:F6}; " +
            $"limit {overlapLimit:F6}; " +
            $"bullet {bulletCenter}; " +
            $"player {grazeArea.Center}; " +
            $"spawn tick {spawnTick}; " +
            $"spawn position {spawnPosition}; " +
            $"spawn direction {spawnDirection}; " +
            $"source {spawnSourceName}.",
            this
        );
    }

    private void HandlePlayerAreaEnter(
        PlayerArea playerArea,
        Collider2D other
    )
    {
        if (owner != BulletOwner.Enemy)
        {
            return;
        }

        PlayerShield playerShield =
            other.GetComponentInParent<PlayerShield>();

        if (playerShield != null && playerShield.IsActive)
        {
            return;
        }

        if (playerArea.AreaType != PlayerAreaType.Hitbox)
        {
            return;
        }

        hasHitPlayer = true;

        PlayerState playerState =
            other.GetComponentInParent<PlayerState>();

        if (playerState != null)
        {
            playerState.TakeHit();
        }
        else
        {
            Debug.LogError("Player has no PlayerState.", other);
        }

        ReturnToPool();
    }

    private Vector2 GetBulletCenter(Vector2 position)
    {
        return position +
            (Vector2)transform.TransformVector(cachedHitboxOffset);
    }

    private float GetBulletWorldRadius()
    {
        return cachedHitboxRadius;
    }
}