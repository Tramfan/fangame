using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerShield : MonoBehaviour
{
    [SerializeField]
    private PlayerInputSource inputSource;

    [SerializeField]
    private PlayerPower playerPower;

    [SerializeField]
    private Collider2D shieldCollider;

    [SerializeField]
    private SpriteRenderer shieldRenderer;

    public bool IsActive
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (inputSource == null)
        {
            inputSource =
                GetComponentInParent<PlayerInputSource>();
        }

        if (playerPower == null)
        {
            playerPower =
                GetComponentInParent<PlayerPower>();
        }

        if (inputSource == null)
        {
            Debug.LogError(
                "Player Shield has no Input Source.",
                this
            );

            enabled = false;
            return;
        }

        if (playerPower == null)
        {
            Debug.LogError(
                "Player Shield has no Player Power.",
                this
            );

            enabled = false;
            return;
        }

        if (shieldCollider == null)
        {
            Debug.LogError(
                "Player Shield Collider is not assigned.",
                this
            );

            enabled = false;
            return;
        }

        shieldCollider.isTrigger = true;

        SetShieldActive(false);
    }

    private void Update()
    {
        bool shouldBeActive =
            inputSource.ShieldHeld &&
            playerPower.HasPower;

        SetShieldActive(shouldBeActive);
    }

    private void OnDisable()
    {
        SetShieldActive(false);
    }

    public bool OwnsCollider(Collider2D other)
    {
        return other == shieldCollider;
    }

    public bool TryAbsorb(int powerCost)
    {
        if (!IsActive)
        {
            return false;
        }

        bool absorbed =
            playerPower.TryAbsorbShieldHit(
                powerCost
            );

        if (!playerPower.HasPower)
        {
            SetShieldActive(false);
        }

        return absorbed;
    }

    private void SetShieldActive(bool active)
    {
        if (shieldCollider == null)
        {
            IsActive = false;
            return;
        }

        if (IsActive == active &&
            shieldCollider.enabled == active)
        {
            return;
        }

        IsActive = active;
        shieldCollider.enabled = active;

        if (shieldRenderer != null)
        {
            shieldRenderer.enabled = active;
        }
    }
}