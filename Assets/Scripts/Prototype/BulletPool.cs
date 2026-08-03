using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BulletPool : MonoBehaviour
{
    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField, Min(0)]
    private int initialSize = 256;

    private readonly Queue<Bullet> available = new();

    private void Awake()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError(
                "Bullet Pool has no Bullet Prefab assigned.",
                this
            );

            enabled = false;
            return;
        }

        for (int index = 0; index < initialSize; index++)
        {
            available.Enqueue(CreateBullet());
        }
    }

    public Bullet Spawn(
        Vector2 position,
        Vector2 direction,
        float speed,
        Transform source = null
    )
    {
        if (!enabled || bulletPrefab == null)
        {
            return null;
        }

        Bullet bullet = available.Count > 0
            ? available.Dequeue()
            : CreateBullet();

        bullet.transform.SetPositionAndRotation(
            position,
            Quaternion.identity
        );

        bullet.Initialize(direction, speed, source);
        bullet.gameObject.SetActive(true);

        return bullet;
    }

    internal void Return(Bullet bullet)
    {
        if (bullet == null || !bullet.gameObject.activeSelf)
        {
            return;
        }

        bullet.gameObject.SetActive(false);
        available.Enqueue(bullet);
    }

    private Bullet CreateBullet()
    {
        Bullet bullet = Instantiate(bulletPrefab, transform);

        bullet.AssignPool(this);
        bullet.gameObject.SetActive(false);

        return bullet;
    }
}