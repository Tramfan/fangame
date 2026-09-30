using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BulletPool : MonoBehaviour
{
    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField, Min(0)]
    private int initialSize = 256;

    private readonly SortedSet<Bullet> available =
        new(Comparer<Bullet>.Create(CompareBullets));

    private readonly HashSet<Bullet> activeBullets = new();
    private readonly Dictionary<Bullet, Rigidbody2D> bodies = new();

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
            available.Add(CreateBullet());
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

        Bullet bullet;

        if (available.Count > 0)
        {
            bullet = available.Min;
            available.Remove(bullet);
        }
        else
        {
            bullet = CreateBullet();
        }

        bullet.transform.SetPositionAndRotation(
            position,
            Quaternion.identity
        );

        Rigidbody2D body = bodies[bullet];

        body.position = position;
        body.rotation = 0f;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        bullet.Initialize(direction, speed, source);
        bullet.gameObject.SetActive(true);

        activeBullets.Add(bullet);

        return bullet;
    }

    public void ClearActiveBullets()
    {
        if (activeBullets.Count == 0)
        {
            return;
        }

        Bullet[] bulletsToReturn =
            new Bullet[activeBullets.Count];

        activeBullets.CopyTo(bulletsToReturn);

        System.Array.Sort(
            bulletsToReturn,
            CompareBullets
        );

        foreach (Bullet bullet in bulletsToReturn)
        {
            Return(bullet);
        }
    }

    internal void Return(Bullet bullet)
    {
        if (bullet == null ||
            !bullet.gameObject.activeSelf)
        {
            return;
        }

        activeBullets.Remove(bullet);

        Rigidbody2D body = bodies[bullet];

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        bullet.gameObject.SetActive(false);
        available.Add(bullet);
    }

    private Bullet CreateBullet()
    {
        Bullet bullet =
            Instantiate(bulletPrefab, transform);

        bullet.AssignPool(this);

        Rigidbody2D body =
            bullet.GetComponent<Rigidbody2D>();

        bodies.Add(bullet, body);

        bullet.gameObject.SetActive(false);

        return bullet;
    }

    private static int CompareBullets(
        Bullet left,
        Bullet right
    )
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return -1;
        }

        if (right == null)
        {
            return 1;
        }

        int siblingComparison =
            left.transform
                .GetSiblingIndex()
                .CompareTo(
                    right.transform
                        .GetSiblingIndex()
                );

        return siblingComparison;
    }
}
