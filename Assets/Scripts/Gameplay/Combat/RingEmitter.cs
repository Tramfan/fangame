using UnityEngine;

public sealed class RingEmitter : MonoBehaviour
{
    [SerializeField]
    private BulletPool bulletPool;

    [Header("Pattern")]
    [SerializeField, Min(1)]
    private int bulletCount = 16;

    [SerializeField, Min(0f)]
    private float bulletSpeed = 2.5f;

    [SerializeField]
    private float rotationPerRing = 8f;

    [SerializeField, Min(1)]
    private int fireIntervalTicks = 90;

    private int ticksUntilFire;
    private float currentAngle;
    private bool initialRingFired;

    private void OnEnable()
    {
        ticksUntilFire =
            Mathf.Max(1, fireIntervalTicks);

        currentAngle = 0f;
        initialRingFired = false;

        TryFireInitialRing();
    }

    private void FixedUpdate()
    {
        if (bulletPool == null)
        {
            return;
        }

        if (!initialRingFired)
        {
            TryFireInitialRing();

            if (!initialRingFired)
            {
                return;
            }
        }

        ticksUntilFire--;

        if (ticksUntilFire > 0)
        {
            return;
        }

        FireRing();

        ticksUntilFire =
            Mathf.Max(1, fireIntervalTicks);
    }

    public void SetBulletPool(
        BulletPool newBulletPool
    )
    {
        bulletPool = newBulletPool;

        TryFireInitialRing();
    }

    private void TryFireInitialRing()
    {
        if (!isActiveAndEnabled ||
            initialRingFired ||
            bulletPool == null)
        {
            return;
        }

        FireRing();
        initialRingFired = true;

        ticksUntilFire =
            Mathf.Max(1, fireIntervalTicks);
    }

    private void FireRing()
    {
        int count = Mathf.Max(1, bulletCount);
        float angleStep = 360f / count;

        for (int index = 0; index < count; index++)
        {
            float angle =
                (currentAngle + angleStep * index) *
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

        currentAngle += rotationPerRing;
    }
}