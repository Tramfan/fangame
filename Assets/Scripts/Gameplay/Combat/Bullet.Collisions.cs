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

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerShield playerShield =
            other.GetComponentInParent<PlayerShield>();

        if (playerShield != null && playerShield.OwnsCollider(other))
        {
            if (owner == BulletOwner.Enemy &&
                !IsPatternControlled &&
                playerShield.TryAbsorb(shieldPowerCost))
            {
                ReturnToPool();
            }

            return;
        }

        PlayerArea playerArea = other.GetComponent<PlayerArea>();

        if (playerArea != null)
        {
            HandlePlayerAreaEnter(playerArea, other);
            return;
        }

        if (owner != BulletOwner.Player)
        {
            return;
        }

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(reflectedDamage);
        ReturnToPool();
    }

    private void UpdateGrazeState(Vector2 bulletPosition)
    {
        if (owner != BulletOwner.Enemy || hasHitPlayer)
        {
            return;
        }

        PlayerArea grazeArea = PlayerArea.GrazeArea;

        if (grazeArea == null)
        {
            wasInsideGrazeArea = false;
            ignoreGrazeUntilExit = false;
            return;
        }

        bool isInside = grazeArea.OverlapsCircle(
            GetBulletCenter(bulletPosition),
            GetBulletWorldRadius()
        );

        if (isInside == wasInsideGrazeArea)
        {
            return;
        }

        PlayerShield playerShield =
            grazeArea.GetComponentInParent<PlayerShield>();

        bool shieldActive =
            playerShield != null && playerShield.IsActive;

        if (isInside && !wasInsideGrazeArea)
        {
            if (!ignoreGrazeUntilExit &&
                !hasGrantedGraze &&
                !shieldActive)
            {
                PlayerState playerState =
                    grazeArea.GetComponentInParent<PlayerState>();

                if (playerState != null)
                {
                    if (logGrazeDiagnostics)
                    {
                        LogGrazeDiagnostic(grazeArea, bulletPosition);
                    }

                    hasGrantedGraze = true;
                    playerState.RegisterGraze();
                }
                else
                {
                    Debug.LogError(
                        "Player has no PlayerState.",
                        grazeArea
                    );
                }
            }
        }
        else if (!isInside && wasInsideGrazeArea)
        {
            if (ignoreGrazeUntilExit)
            {
                ignoreGrazeUntilExit = false;
            }
            else if (hasGrantedGraze &&
                     !shieldActive &&
                     !IsPatternControlled &&
                     !grazedBeforePatternRelease)
            {
                if (grazeArea.ReflectionEnabled)
                {
                    Reflect(grazeArea.Center);
                }
            }
        }

        wasInsideGrazeArea = isInside;
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
            (Vector2)transform.TransformVector(bulletCollider.offset);
    }

    private float GetBulletWorldRadius()
    {
        Vector3 scale = transform.lossyScale;

        float largestScale = Mathf.Max(
            Mathf.Abs(scale.x),
            Mathf.Abs(scale.y)
        );

        return bulletCollider.radius * largestScale;
    }
}