using UnityEngine;

public sealed class RingEmitter : MonoBehaviour
{
    [SerializeField]
    private BulletPool bulletPool;

    [SerializeField, Min(1)]
    private int bulletCount = 16;

    [SerializeField]
    private float bulletSpeed = 2.5f;

    [SerializeField, Min(1)]
    private int fireIntervalTicks = 120;

    [SerializeField]
    private float rotationPerRing = 7.5f;

    private int ticksUntilFire;
    private float currentAngle;

    private void OnEnable()
    {
        ticksUntilFire = fireIntervalTicks;
        currentAngle = 0f;
        FireRing();
    }

    private void FixedUpdate()
    {
        ticksUntilFire--;

        if (ticksUntilFire > 0)
        {
            return;
        }

        FireRing();
        ticksUntilFire = Mathf.Max(1, fireIntervalTicks);
    }

    private void FireRing()
    {
        if (bulletPool == null)
        {
            Debug.LogError(
                "Bullet Pool is not assigned.",
                this
            );

            return;
        }

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