using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PickupMovement))]
[RequireComponent(typeof(Collider2D))]
public sealed class ScorePickup : MonoBehaviour
{
    [Header("Value")]
    [SerializeField, Min(0)]
    private int minimumValue = 100;

    [SerializeField, Min(1)]
    private int maximumValue = 1000;

    [SerializeField, Range(0.01f, 1f)]
    private float maximumValueViewportY = 0.8f;

    [SerializeField, Min(1)]
    private int valueStep = 10;

    private Collider2D pickupCollider;
    private Camera gameplayCamera;
    private bool collected;

    private void Awake()
    {
        pickupCollider =
            GetComponent<Collider2D>();

        pickupCollider.isTrigger = true;

        gameplayCamera = Camera.main;

        if (gameplayCamera == null)
        {
            Debug.LogError(
                "Score Pickup cannot find the Main Camera.",
                this
            );
        }
    }

    private void OnEnable()
    {
        collected = false;
        pickupCollider.enabled = true;
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
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

        collected = true;
        pickupCollider.enabled = false;

        GameRunContext.AddScore(
            CalculateValue()
        );

        Destroy(gameObject);
    }

    private int CalculateValue()
    {
        if (gameplayCamera == null)
        {
            return minimumValue;
        }

        float viewportY =
            gameplayCamera.WorldToViewportPoint(
                transform.position
            ).y;

        float heightFactor =
            Mathf.InverseLerp(
                0f,
                maximumValueViewportY,
                viewportY
            );

        float unroundedValue =
            Mathf.Lerp(
                minimumValue,
                maximumValue,
                heightFactor
            );

        int roundedValue =
            Mathf.RoundToInt(
                unroundedValue / valueStep
            ) * valueStep;

        return Mathf.Clamp(
            roundedValue,
            minimumValue,
            maximumValue
        );
    }

    private void OnValidate()
    {
        minimumValue =
            Mathf.Max(
                0,
                minimumValue
            );

        maximumValue =
            Mathf.Max(
                minimumValue,
                maximumValue
            );

        maximumValueViewportY =
            Mathf.Clamp(
                maximumValueViewportY,
                0.01f,
                1f
            );

        valueStep =
            Mathf.Max(
                1,
                valueStep
            );
    }
}