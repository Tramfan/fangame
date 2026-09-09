using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class PlayerBullet :
    MonoBehaviour,
    IPlayerProjectile
{
    [SerializeField, Min(0f)]
    private float speed = 12f;

    [SerializeField, Min(1)]
    private int damage = 1;

    [Header("Removal Bounds")]
    [SerializeField, Min(0f)]
    private float removalX = 6f;

    [SerializeField]
    private float removalY = 6f;

    [SerializeField]
    private float lowerRemovalY = -6f;

    private Rigidbody2D body;
    private Vector2 direction = Vector2.up;
    private bool hasHit;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        hasHit = false;
    }

    public void Initialize(
        Vector2 movementDirection,
        float movementSpeed,
        int hitDamage
    )
    {
        direction =
            movementDirection.sqrMagnitude > 0.0001f
                ? movementDirection.normalized
                : Vector2.up;

        speed = Mathf.Max(0f, movementSpeed);
        damage = Mathf.Max(1, hitDamage);

        float rotationDegrees =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg -
            90f;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            rotationDegrees
        );
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition =
            body.position +
            direction *
            speed *
            Time.fixedDeltaTime;

        body.MovePosition(nextPosition);

        if (Mathf.Abs(nextPosition.x) >= removalX ||
            nextPosition.y >= removalY ||
            nextPosition.y <= lowerRemovalY)
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