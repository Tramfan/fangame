using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class PlayerBullet : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float speed = 12f;

    [SerializeField, Min(1)]
    private int damage = 1;

    [SerializeField]
    private float removalY = 6f;

    private Rigidbody2D body;
    private bool hasHit;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        hasHit = false;
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition =
            body.position +
            Vector2.up *
            speed *
            Time.fixedDeltaTime;

        body.MovePosition(nextPosition);

        if (nextPosition.y >= removalY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
        {
            return;
        }

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        hasHit = true;
        damageable.TakeDamage(damage);

        Destroy(gameObject);
    }
}