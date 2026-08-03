using UnityEngine;

public sealed class PrototypeRingEmitter : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int bulletCount = 16;
    [SerializeField] private float bulletSpeed = 2.5f;
    [SerializeField, Min(1)]
private int fireIntervalTicks = 120;

private int ticksUntilFire;
    [SerializeField] private float rotationPerRing = 7.5f;


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
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not assigned.", this);
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

            Bullet bullet = Instantiate(
                bulletPrefab,
                transform.position,
                Quaternion.identity
            );

           bullet.Initialize(
    direction,
    bulletSpeed,
    transform.root
);
        }

        currentAngle += rotationPerRing;
    }
}