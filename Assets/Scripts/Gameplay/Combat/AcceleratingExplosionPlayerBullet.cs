using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class AcceleratingExplosionPlayerBullet :
    MonoBehaviour,
    IPlayerProjectile
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float accelerationPerSecond = 8f;

    [SerializeField, Min(0f)]
    private float maximumSpeed = 12f;

    [Header("Explosion")]
    [SerializeField, Min(0.01f)]
    private float explosionRadius = 0.55f;

    private Rigidbody2D body;
    private Vector2 direction;
    private float speed;
    private int damage;
    private bool hasExploded;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Vector2 newDirection,
        float initialSpeed,
        int hitDamage
    )
    {
        direction = newDirection.normalized;
        speed = Mathf.Max(
            0f,
            initialSpeed
        );

        damage = Mathf.Max(
            1,
            hitDamage
        );

        body.linearVelocity =
            direction * speed;
    }

    private void FixedUpdate()
    {
        if (hasExploded)
        {
            return;
        }

        speed = Mathf.Min(
            maximumSpeed,
            speed +
            accelerationPerSecond *
            Time.fixedDeltaTime
        );

        body.linearVelocity =
            direction * speed;
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (hasExploded)
        {
            return;
        }

        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
        {
            return;
        }

        Explode();
    }

    private void Explode()
    {
        hasExploded = true;

        Collider2D[] hitColliders =
            Physics2D.OverlapCircleAll(
                transform.position,
                explosionRadius
            );

        HashSet<EnemyHealth> hitEnemies = new();

        foreach (Collider2D hitCollider
                 in hitColliders)
        {
            EnemyHealth enemy =
                hitCollider
                    .GetComponentInParent<
                        EnemyHealth
                    >();

            if (enemy == null ||
                enemy.IsDead ||
                enemy.IsInvulnerable ||
                !hitEnemies.Add(enemy))
            {
                continue;
            }

            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        if (!hasExploded)
        {
            Destroy(gameObject);
        }
    }
}