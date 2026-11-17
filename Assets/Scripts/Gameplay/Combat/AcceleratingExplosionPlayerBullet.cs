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

    [Header("Removal Bounds")]
    [SerializeField, Min(0f)]
    private float removalX = 6f;

    [SerializeField]
    private float removalY = 6f;

    [SerializeField]
    private float lowerRemovalY = -6f;

       private const int ExplosionBufferCapacity = 64;

    private readonly List<Collider2D> hitColliders =
        new(ExplosionBufferCapacity);

    private readonly HashSet<EnemyHealth> hitEnemies =
        new(ExplosionBufferCapacity);

    private Rigidbody2D body;
    private Vector2 direction = Vector2.up;
    private float speed;
    private int damage = 1;
    private bool hasExploded;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        hasExploded = false;
        direction = Vector2.up;
        speed = 0f;
        damage = 1;
        hitEnemies.Clear();

        if (body != null)
        {
            body.position = transform.position;
            body.linearVelocity = Vector2.zero;
        }
    }

    public void Initialize(
        Vector2 newDirection,
        float initialSpeed,
        int hitDamage
    )
    {
        if (body != null)
        {
            body.position = transform.position;
        }

        direction = newDirection.sqrMagnitude > 0.0001f
            ? newDirection.normalized
            : Vector2.up;

        speed = Mathf.Max(
            0f,
            initialSpeed
        );

        damage = Mathf.Max(
            1,
            hitDamage
        );

        if (body != null)
        {
            body.linearVelocity = direction * speed;
        }
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

        Vector2 currentPos = body.position;

        if (Mathf.Abs(currentPos.x) >= removalX ||
            currentPos.y >= removalY ||
            currentPos.y <= lowerRemovalY)
        {
            body.linearVelocity = Vector2.zero;
            PlayerProjectilePool.Despawn(gameObject);
        }
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

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }

        hitColliders.Clear();
        hitEnemies.Clear();

        // Зберігаємо попередні налаштування шарів і тригерів.
        ContactFilter2D filter = new()
        {
            useTriggers = Physics2D.queriesHitTriggers,
            useLayerMask = true,
            layerMask = Physics2D.DefaultRaycastLayers
        };

        Physics2D.OverlapCircle(
            transform.position,
            explosionRadius,
            filter,
            hitColliders
        );

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider == null)
            {
                continue;
            }

            EnemyHealth enemy =
                hitCollider.GetComponentInParent<EnemyHealth>();

            if (enemy == null ||
                enemy.IsDead ||
                enemy.IsInvulnerable ||
                !hitEnemies.Add(enemy))
            {
                continue;
            }

            // Кілька колайдерів одного ворога дають лише одне влучання.
            enemy.TakeDamage(damage);
        }

        // Очищення прибирає посилання, зберігаючи місткість буферів.
        hitColliders.Clear();
        hitEnemies.Clear();

        PlayerProjectilePool.Despawn(gameObject);
    }
}