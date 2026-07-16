using UnityEngine;

public sealed class PrototypeAimedFanEmitter : MonoBehaviour
{
    [SerializeField] private PrototypeEnemyBullet bulletPrefab;
    [SerializeField] private Transform target;

    [Header("Pattern")]
    [SerializeField] private int bulletCount = 5;
    [SerializeField] private float spreadAngle = 60f;
    [SerializeField] private float bulletSpeed = 3f;
    [SerializeField] private float fireInterval = 1.5f;

    private float timer;

    private void Start()
    {
        FireFan();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < fireInterval)
        {
            return;
        }

        timer -= fireInterval;
        FireFan();
    }

    private void FireFan()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not assigned.", this);
            return;
        }

        if (target == null)
        {
            Debug.LogError("Target is not assigned.", this);
            return;
        }

        Vector2 directionToTarget =
            target.position - transform.position;

        if (directionToTarget.sqrMagnitude < 0.0001f)
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

            PrototypeEnemyBullet bullet = Instantiate(
                bulletPrefab,
                transform.position,
                Quaternion.identity
            );

            bullet.Initialize(direction, bulletSpeed);
        }
    }
}