using UnityEngine;

public sealed class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float normalSpeed = 5f;

    [SerializeField]
    private float focusSpeed = 2f;

    [Header("Movement Bounds")]
    [SerializeField]
    private Vector2 minimumPosition = new(-3.5f, -4.5f);

    [SerializeField]
    private Vector2 maximumPosition = new(3.5f, 4.5f);

    [Header("Focus Mode")]
    [SerializeField]
    private SpriteRenderer hitboxRenderer;

    public bool IsFocused { get; private set; }

    private Vector2 movementInput;

    private void Awake()
    {
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
        ReadMovementInput();
        ReadFocusInput();
        UpdateHitboxVisibility();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ReadMovementInput()
    {
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        movementInput =
            Vector2.ClampMagnitude(movementInput, 1f);
    }

    private void ReadFocusInput()
    {
        IsFocused =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);
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
            movementInput *
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
}