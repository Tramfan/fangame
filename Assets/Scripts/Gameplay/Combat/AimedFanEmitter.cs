using UnityEngine;

public sealed class AimedFanEmitter : MonoBehaviour
{
    [SerializeField]
    private BulletPool bulletPool;

    [SerializeField]
    private Transform target;

    [Header("Pattern")]
    [SerializeField, Min(1)]
    private int bulletCount = 5;

    [SerializeField, Range(0f, 360f)]
    private float spreadAngle = 60f;

    [SerializeField, Min(0f)]
    private float bulletSpeed = 3f;

    [SerializeField, Min(1)]
    private int fireIntervalTicks = 90;

    private int ticksUntilFire;
    private bool initialBurstFired;

    private void OnEnable()
    {
        ticksUntilFire =
            Mathf.Max(1, fireIntervalTicks);

        initialBurstFired = false;

        TryFireInitialBurst();
    }

    private void FixedUpdate()
    {
        if (bulletPool == null || target == null)
        {
            return;
        }

        if (!initialBurstFired)
        {
            TryFireInitialBurst();

            if (!initialBurstFired)
            {
                return;
            }
        }

        ticksUntilFire--;

        if (ticksUntilFire > 0)
        {
            return;
        }

        FireFan();

        ticksUntilFire =
            Mathf.Max(1, fireIntervalTicks);
    }

    public void Configure(
        Transform newTarget,
        BulletPool newBulletPool
    )
    {
        target = newTarget;
        bulletPool = newBulletPool;

        TryFireInitialBurst();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        TryFireInitialBurst();
    }

    public void SetBulletPool(
        BulletPool newBulletPool
    )
    {
        bulletPool = newBulletPool;

        TryFireInitialBurst();
    }

    private void TryFireInitialBurst()
    {
        if (!isActiveAndEnabled ||
            initialBurstFired ||
            bulletPool == null ||
            target == null)
        {
            return;
        }

        FireFan();
        initialBurstFired = true;

        ticksUntilFire =
            Mathf.Max(1, fireIntervalTicks);
    }

    private void FireFan()
    {
        Vector2 directionToTarget =
            target.position - transform.position;

        if (directionToTarget.sqrMagnitude <
            0.0001f)
        {
            return;
        }

        float centerAngle = Mathf.Atan2(
            directionToTarget.y,
            directionToTarget.x
        ) * Mathf.Rad2Deg;

        int count = Mathf.Max(1, bulletCount);

        float firstAngle = count == 1
            ? centerAngle
            : centerAngle - spreadAngle * 0.5f;

        float angleStep = count == 1
            ? 0f
            : spreadAngle / (count - 1);

        for (int index = 0; index < count; index++)
        {
            float angle =
                (firstAngle + angleStep * index) *
                Mathf.Deg2Rad;

            Vector2 direction = new(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            );

            bulletPool.Spawn(
                transform.position,
                direction,
                bulletSpeed,
                transform.root
            );
        }
    }
}