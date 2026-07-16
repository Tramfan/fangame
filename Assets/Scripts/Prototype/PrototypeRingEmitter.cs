using UnityEngine;

public sealed class PrototypeRingEmitter : MonoBehaviour
{
    [SerializeField] private PrototypeEnemyBullet bulletPrefab;
    [SerializeField] private int bulletCount = 16;
    [SerializeField] private float bulletSpeed = 2.5f;
    [SerializeField] private float fireInterval = 2f;
    [SerializeField] private float rotationPerRing = 7.5f;

    private float timer;
    private float currentAngle;

    private void Start()
    {
        FireRing();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < fireInterval)
        {
            return;
        }

        timer -= fireInterval;
        FireRing();
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

            PrototypeEnemyBullet bullet = Instantiate(
                bulletPrefab,
                transform.position,
                Quaternion.identity
            );

            bullet.Initialize(direction, bulletSpeed);
        }

        currentAngle += rotationPerRing;
    }
}