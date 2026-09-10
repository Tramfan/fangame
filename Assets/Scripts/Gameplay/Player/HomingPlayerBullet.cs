using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class HomingPlayerBullet :
    MonoBehaviour,
    IPlayerProjectile
{
    [SerializeField, Min(0f)]
    private float speed = 12f;

    [SerializeField, Min(1)]
    private int damage = 1;

    [Header("Homing")]
    [SerializeField, Min(0f)]
    private float turnSpeedDegreesPerSecond = 360f;
[SerializeField, Min(0)]
private int homingDurationTicks = 90;
    [Header("Removal Bounds")]
    [SerializeField, Min(0f)]
    private float removalX = 6f;

    [SerializeField]
    private float removalY = 6f;

    [SerializeField]
    private float lowerRemovalY = -6f;

    private Rigidbody2D body;

    private Vector2 direction =
        Vector2.up;
private Transform target;
private EnemyHealth targetHealth;

private bool hasAcquiredTarget;
private bool hasHit;
private int homingTicksRemaining;
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        hasHit = false;
    target = null;
    targetHealth = null;
    hasAcquiredTarget = false;
    homingTicksRemaining = 0;
    }

    public void Initialize(
        Vector2 movementDirection,
        float movementSpeed,
        int hitDamage
    )
    {
        direction =
            movementDirection.sqrMagnitude > 0.0001f
                ? movementDirection.normalized
                : Vector2.up;

        speed = Mathf.Max(
            0f,
            movementSpeed
        );

        damage = Mathf.Max(
            1,
            hitDamage
        );
homingTicksRemaining =
    Mathf.Max(
        0,
        homingDurationTicks
    );
    
        AcquireTarget();
        UpdateRotation();
    }

private void FixedUpdate()
{
    if (homingTicksRemaining > 0)
    {
        if (!hasAcquiredTarget)
        {
            AcquireTarget();
        }

        if (hasAcquiredTarget)
        {
            if (HasValidTarget())
            {
                TurnTowardsTarget();
            }
            else
            {
                StopHoming();
            }
        }

        if (homingTicksRemaining > 0)
        {
            homingTicksRemaining--;

            if (homingTicksRemaining == 0)
            {
                StopHoming();
            }
        }
    }

    Vector2 nextPosition =
        body.position +
        direction *
        speed *
        Time.fixedDeltaTime;

    body.MovePosition(nextPosition);
    UpdateRotation();

    if (Mathf.Abs(nextPosition.x) >= removalX ||
        nextPosition.y >= removalY ||
        nextPosition.y <= lowerRemovalY)
    {
        Destroy(gameObject);
    }
}

    private bool HasValidTarget()
    {
        return
            target != null &&
            targetHealth != null &&
            targetHealth.isActiveAndEnabled &&
            !targetHealth.IsDead &&
            !targetHealth.IsInvulnerable;
    }
private void StopHoming()
{
    homingTicksRemaining = 0;
    target = null;
    targetHealth = null;
}
    private void AcquireTarget()
{
    target =
        EnemyTargetRegistry.FindNearest(
            body.position
        );

    targetHealth =
        target != null
            ? target.GetComponent<EnemyHealth>()
            : null;

    if (targetHealth != null)
    {
        hasAcquiredTarget = true;
    }
}
    private void TurnTowardsTarget()
    {
        if (!HasValidTarget())
        {
            return;
        }

        Vector2 targetDirection =
            (Vector2)target.position -
            body.position;

        if (targetDirection.sqrMagnitude <=
            0.0001f)
        {
            return;
        }

        targetDirection.Normalize();

        float currentAngle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        float targetAngle =
            Mathf.Atan2(
                targetDirection.y,
                targetDirection.x
            ) * Mathf.Rad2Deg;

        float newAngle =
            Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                turnSpeedDegreesPerSecond *
                Time.fixedDeltaTime
            );

        float newAngleRadians =
            newAngle * Mathf.Deg2Rad;

        direction = new Vector2(
            Mathf.Cos(newAngleRadians),
            Mathf.Sin(newAngleRadians)
        );
    }

    private void UpdateRotation()
    {
        float rotationDegrees =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg -
            90f;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                rotationDegrees
            );
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (hasHit)
        {
            return;
        }

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        hasHit = true;

        damageable.TakeDamage(
            damage
        );

        Destroy(gameObject);
    }
}