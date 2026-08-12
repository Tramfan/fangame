using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PickupMovement))]
[RequireComponent(typeof(Collider2D))]
public sealed class PowerPickup : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int powerAmount = 10;

    private Collider2D pickupCollider;
    private bool collected;

    private void Awake()
    {
        pickupCollider =
            GetComponent<Collider2D>();

        pickupCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        collected = false;
        pickupCollider.enabled = true;
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
            playerArea.AreaType !=
            PlayerAreaType.Hitbox)
        {
            return;
        }

        PlayerPower playerPower =
            other.GetComponentInParent<PlayerPower>();

        if (playerPower == null)
        {
            Debug.LogError(
                "Player hitbox has no Player Power.",
                other
            );

            return;
        }

        collected = true;
        pickupCollider.enabled = false;

        playerPower.AddPower(powerAmount);

        Destroy(gameObject);
    }

    private void OnValidate()
    {
        powerAmount = Mathf.Max(
            1,
            powerAmount
        );
    }
}