using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PickupMovement : MonoBehaviour
{
    [Header("Default Launch")]
    [SerializeField]
    private Vector2 defaultInitialVelocity =
        new(0f, 1.5f);

    [Header("Falling")]
    [SerializeField, Min(0f)]
    private float fallAcceleration = 4f;

    [SerializeField, Min(0.1f)]
    private float maximumFallSpeed = 3f;

    [SerializeField, Min(0f)]
    private float horizontalDeceleration = 2f;

    [SerializeField]
    private float removalY = -6f;

    private Rigidbody2D body;
    private Vector2 velocity;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void OnEnable()
    {
        velocity = defaultInitialVelocity;
    }

    public void Launch(Vector2 initialVelocity)
    {
        velocity = initialVelocity;
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        velocity.x = Mathf.MoveTowards(
            velocity.x,
            0f,
            horizontalDeceleration * deltaTime
        );

        velocity.y = Mathf.Max(
            velocity.y -
            fallAcceleration * deltaTime,
            -maximumFallSpeed
        );

        Vector2 nextPosition =
            body.position +
            velocity * deltaTime;

        body.MovePosition(nextPosition);

        if (nextPosition.y <= removalY)
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        fallAcceleration =
            Mathf.Max(0f, fallAcceleration);

        maximumFallSpeed =
            Mathf.Max(0.1f, maximumFallSpeed);

        horizontalDeceleration =
            Mathf.Max(0f, horizontalDeceleration);
    }
}