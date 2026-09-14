using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PickupMovement : MonoBehaviour
{
    private static readonly List<PickupMovement>
        activePickups = new();

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

    [Header("Attraction")]
    [SerializeField, Min(0.1f)]
    private float attractionSpeed = 12f;

    [Header("Removal")]
    [SerializeField]
    private float removalY = -6f;

    private Rigidbody2D body;
    private Vector2 velocity;
    private Transform attractionTarget;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetRegistry()
    {
        activePickups.Clear();
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        body.bodyType =
            RigidbodyType2D.Kinematic;

        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void OnEnable()
    {
        velocity = defaultInitialVelocity;
        attractionTarget = null;

        if (!activePickups.Contains(this))
        {
            activePickups.Add(this);
        }
    }

    private void OnDisable()
    {
        activePickups.Remove(this);
    }

    public void Launch(
        Vector2 initialVelocity
    )
    {
        velocity = initialVelocity;
    }

    public void AttractTo(
        Transform target
    )
    {
        if (target != null)
        {
            attractionTarget = target;
        }
    }

    public static void AttractAllTo(
        Transform target
    )
    {
        if (target == null)
        {
            return;
        }

        for (int index =
                 activePickups.Count - 1;
             index >= 0;
             index--)
        {
            PickupMovement pickup =
                activePickups[index];

            if (pickup == null)
            {
                activePickups.RemoveAt(index);
                continue;
            }

            pickup.AttractTo(target);
        }
    }

    private void FixedUpdate()
    {
        float deltaTime =
            Time.fixedDeltaTime;

        if (attractionTarget != null)
        {
            Vector2 nextPosition =
                Vector2.MoveTowards(
                    body.position,
                    attractionTarget.position,
                    attractionSpeed * deltaTime
                );

            body.MovePosition(nextPosition);
            return;
        }

        velocity.x =
            Mathf.MoveTowards(
                velocity.x,
                0f,
                horizontalDeceleration *
                deltaTime
            );

        velocity.y =
            Mathf.Max(
                velocity.y -
                fallAcceleration * deltaTime,
                -maximumFallSpeed
            );

        Vector2 fallingPosition =
            body.position +
            velocity * deltaTime;

        body.MovePosition(fallingPosition);

        if (fallingPosition.y <= removalY)
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        fallAcceleration =
            Mathf.Max(
                0f,
                fallAcceleration
            );

        maximumFallSpeed =
            Mathf.Max(
                0.1f,
                maximumFallSpeed
            );

        horizontalDeceleration =
            Mathf.Max(
                0f,
                horizontalDeceleration
            );

        attractionSpeed =
            Mathf.Max(
                0.1f,
                attractionSpeed
            );
    }
}