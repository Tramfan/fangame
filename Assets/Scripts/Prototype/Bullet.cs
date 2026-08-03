using UnityEngine;

public enum BulletOwner
{
    Enemy,
    Player
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class Bullet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float speed = 2.5f;

    [SerializeField]
    private Vector2 removalBounds = new(6f, 6f);

    [Header("Reflection")]
    [SerializeField, Min(0.05f)]
    private float reflectionTurnRadius = 0.45f;

    [SerializeField, Min(1f)]
    private float reflectedSpeedMultiplier = 6f;

    [SerializeField, Min(1)]
    private int reflectedDamage = 1;

    [SerializeField]
    private Color reflectedColor = new(0.25f, 1f, 1f, 1f);

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private BulletPool pool;

    private Color originalColor = Color.white;
    private Vector2 direction = Vector2.down;
    private Transform source;

    private BulletOwner owner = BulletOwner.Enemy;

    private bool hasGrantedGraze;
    private bool hasHitPlayer;

    private bool isTurning;
    private float turnSign;
    private float turnDegreesRemaining;
    private Vector2 turnCenter;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    internal void AssignPool(BulletPool bulletPool)
    {
        pool = bulletPool;
    }

    public void Initialize(
        Vector2 movementDirection,
        float movementSpeed,
        Transform bulletSource = null
    )
    {
        direction = movementDirection.normalized;
        speed = movementSpeed;
        source = bulletSource;

        owner = BulletOwner.Enemy;

        hasGrantedGraze = false;
        hasHitPlayer = false;

        isTurning = false;
        turnSign = 0f;
        turnDegreesRemaining = 0f;
        turnCenter = Vector2.zero;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition;

        if (isTurning)
        {
            nextPosition = CalculateTurnPosition();
        }
        else
        {
            nextPosition =
                body.position +
                direction * speed * Time.fixedDeltaTime;
        }

        body.MovePosition(nextPosition);

        if (Mathf.Abs(nextPosition.x) > removalBounds.x ||
            Mathf.Abs(nextPosition.y) > removalBounds.y)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PrototypePlayerArea playerArea =
            other.GetComponent<PrototypePlayerArea>();

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

    private void OnTriggerExit2D(Collider2D other)
    {
        if (owner != BulletOwner.Enemy ||
            hasHitPlayer ||
            !hasGrantedGraze)
        {
            return;
        }

        PrototypePlayerArea playerArea =
            other.GetComponent<PrototypePlayerArea>();

        if (playerArea == null ||
            playerArea.AreaType != PrototypePlayerAreaType.Graze)
        {
            return;
        }

        Reflect(playerArea.transform.position);
    }

    private void HandlePlayerAreaEnter(
        PrototypePlayerArea playerArea,
        Collider2D other
    )
    {
        // Отражённая пуля больше не опасна для игрока.
        if (owner != BulletOwner.Enemy)
        {
            return;
        }

        if (playerArea.AreaType ==
            PrototypePlayerAreaType.Hitbox)
        {
            hasHitPlayer = true;

            PrototypePlayerState playerState =
                other.GetComponentInParent<PrototypePlayerState>();

            if (playerState != null)
            {
                playerState.TakeHit();
            }
            else
            {
                Debug.LogError(
                    "Player has no PrototypePlayerState.",
                    other
                );
            }

            ReturnToPool();
            return;
        }

        // Пуля, созданная уже внутри зоны грейза,
        // не считается честно пойманной.
        if (source != null &&
            other.OverlapPoint((Vector2)source.position))
        {
            return;
        }

        if (hasGrantedGraze)
        {
            return;
        }

        PrototypePlayerController player =
            other.GetComponentInParent<PrototypePlayerController>();

        if (player == null)
        {
            return;
        }

        hasGrantedGraze = true;
        player.RegisterGraze();
    }

    private void Reflect(Vector2 playerCenter)
    {
        owner = BulletOwner.Player;

        Vector2 directionToPlayer =
            playerCenter - body.position;

        float cross =
            direction.x * directionToPlayer.y -
            direction.y * directionToPlayer.x;

        if (Mathf.Abs(cross) > 0.0001f)
        {
            turnSign = Mathf.Sign(cross);
        }
        else
        {
            // Почти центральная траектория:
            // выбираем стабильную сторону поворота.
            turnSign =
                body.position.x >= playerCenter.x
                    ? 1f
                    : -1f;
        }

        Vector2 leftNormal =
            new(-direction.y, direction.x);

        turnCenter =
            body.position +
            leftNormal *
            turnSign *
            reflectionTurnRadius;

        turnDegreesRemaining = 180f;
        isTurning = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = reflectedColor;
        }
    }

    private Vector2 CalculateTurnPosition()
    {
        float angularSpeed =
            speed /
            reflectionTurnRadius *
            Mathf.Rad2Deg;

        float angleStep = Mathf.Min(
            angularSpeed * Time.fixedDeltaTime,
            turnDegreesRemaining
        );

        float signedAngle = angleStep * turnSign;

        Vector2 relativePosition =
            body.position - turnCenter;

        relativePosition =
            RotateVector(relativePosition, signedAngle);

        direction =
            RotateVector(direction, signedAngle).normalized;

        turnDegreesRemaining -= angleStep;

        Vector2 nextPosition =
            turnCenter + relativePosition;

        if (turnDegreesRemaining <= 0.001f)
        {
            FinishReflectionTurn(nextPosition);
        }

        return nextPosition;
    }

    private void FinishReflectionTurn(Vector2 currentPosition)
    {
        isTurning = false;
        speed *= reflectedSpeedMultiplier;

        Transform target = source;

        if (target == null)
        {
            target = FindNearestEnemy(currentPosition);
        }

        if (target != null)
        {
            Vector2 directionToTarget =
                (Vector2)target.position - currentPosition;

            if (directionToTarget.sqrMagnitude > 0.0001f)
            {
                direction = directionToTarget.normalized;
                return;
            }
        }

        direction.Normalize();
    }

    private static Transform FindNearestEnemy(
        Vector2 currentPosition
    )
    {
        EnemyHealth[] enemies =
            FindObjectsByType<EnemyHealth>(
                FindObjectsSortMode.None
            );

        EnemyHealth nearestEnemy = null;
        float nearestDistanceSquared =
            float.PositiveInfinity;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            Vector2 offset =
                (Vector2)enemy.transform.position -
                currentPosition;

            float distanceSquared = offset.sqrMagnitude;

            if (distanceSquared >= nearestDistanceSquared)
            {
                continue;
            }

            nearestDistanceSquared = distanceSquared;
            nearestEnemy = enemy;
        }

        return nearestEnemy != null
            ? nearestEnemy.transform
            : null;
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
            vector.x * cosine - vector.y * sine,
            vector.x * sine + vector.y * cosine
        );
    }

    private void ReturnToPool()
{
    if (!gameObject.activeSelf)
    {
        return;
    }

    if (pool == null)
    {
        Debug.LogError(
            "Bullet has no pool assigned.",
            this
        );

        gameObject.SetActive(false);
        return;
    }

    pool.Return(this);
}
}