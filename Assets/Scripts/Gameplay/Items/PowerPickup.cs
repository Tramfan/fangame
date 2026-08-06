using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class PowerPickup : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int powerAmount = 10;

    [SerializeField, Min(0f)]
    private float fallSpeed = 1.5f;

    [SerializeField]
    private float removalY = -6f;

    private Rigidbody2D body;
    private Collider2D pickupCollider;

    private bool collected;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        pickupCollider = GetComponent<Collider2D>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;

        pickupCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        collected = false;
        pickupCollider.enabled = true;
    }

    private void FixedUpdate()
    {
        if (collected)
        {
            return;
        }

        Vector2 nextPosition =
            body.position +
            Vector2.down *
            fallSpeed *
            Time.fixedDeltaTime;

        body.MovePosition(nextPosition);

        if (nextPosition.y <= removalY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        PlayerArea playerArea =
            other.GetComponent<PlayerArea>();

        if (playerArea == null ||
            playerArea.AreaType != PlayerAreaType.Hitbox)
        {
            return;
        }

        PlayerPower playerPower =
            other.GetComponentInParent<PlayerPower>();

        if (playerPower == null)
        {
            Debug.LogError(
                "Player hitbox has no PlayerPower.",
                other
            );

            return;
        }

        collected = true;
        pickupCollider.enabled = false;

        playerPower.AddPower(powerAmount);

        Destroy(gameObject);
    }
}