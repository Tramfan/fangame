using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerInputSource inputSource;

    [Header("Movement")]
    [SerializeField]
    private float normalSpeed = 5f;

    [SerializeField]
    private float focusSpeed = 2f;

    [Header("Movement Bounds")]
    [SerializeField]
    private Vector2 minimumPosition =
        new(-3.5f, -4.5f);

    [SerializeField]
    private Vector2 maximumPosition =
        new(3.5f, 4.5f);

    [Header("Focus Mode")]
    [SerializeField]
    private SpriteRenderer hitboxRenderer;

    public bool IsFocused
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

        if (inputSource == null)
        {
            Debug.LogError(
                "Player Controller has no Input Source.",
                this
            );

            enabled = false;
            return;
        }

        if (hitboxRenderer != null)
        {
            hitboxRenderer.enabled = false;
        }
        else
        {
            Debug.LogError(
                "Hitbox Renderer is not assigned.",
                this
            );
        }
    }

    private void Update()
    {
        UpdateFocusState();
        UpdateHitboxVisibility();
    }

    private void FixedUpdate()
    {
        UpdateFocusState();
        Move();
    }

    private void UpdateFocusState()
    {
        IsFocused = inputSource.FocusHeld;
    }

    private void UpdateHitboxVisibility()
    {
        if (hitboxRenderer != null)
        {
            hitboxRenderer.enabled = IsFocused;
        }
    }

    private void Move()
    {
        float currentSpeed = IsFocused
            ? focusSpeed
            : normalSpeed;

        Vector2 nextPosition =
            (Vector2)transform.position +
            inputSource.Movement *
            currentSpeed *
            Time.fixedDeltaTime;

        nextPosition.x = Mathf.Clamp(
            nextPosition.x,
            minimumPosition.x,
            maximumPosition.x
        );

        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            minimumPosition.y,
            maximumPosition.y
        );

        transform.position = nextPosition;
    }
    public void ConfigureMovement(
    float newNormalSpeed,
    float newFocusSpeed
)
{
    normalSpeed = Mathf.Max(
        0f,
        newNormalSpeed
    );

    focusSpeed = Mathf.Max(
        0f,
        newFocusSpeed
    );
}
}