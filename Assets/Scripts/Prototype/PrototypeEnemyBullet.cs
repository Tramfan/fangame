using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class PrototypeEnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private Vector2 removalBounds = new(6f, 6f);

    private Rigidbody2D body;
    private Vector2 direction = Vector2.down;
    private bool hasGrantedGraze;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 movementDirection, float movementSpeed)
    {
        direction = movementDirection.normalized;
        speed = movementSpeed;
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition =
            body.position +
            direction * speed * Time.fixedDeltaTime;

        body.MovePosition(nextPosition);

        if (Mathf.Abs(nextPosition.x) > removalBounds.x ||
            Mathf.Abs(nextPosition.y) > removalBounds.y)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PrototypePlayerArea playerArea =
            other.GetComponent<PrototypePlayerArea>();

        if (playerArea == null)
        {
            return;
        }

        if (playerArea.AreaType == PrototypePlayerAreaType.Hitbox)
        {
            Debug.Log("Player hit.");
            Destroy(gameObject);
            return;
        }

        if (hasGrantedGraze)
        {
            return;
        }

        PrototypePlayerController player =
            other.GetComponentInParent<PrototypePlayerController>();

        if (player == null)
        {
            return;
        }

        hasGrantedGraze = true;
        player.RegisterGraze();
    }
}