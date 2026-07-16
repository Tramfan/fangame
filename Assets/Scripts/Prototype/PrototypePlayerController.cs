using UnityEngine;

public sealed class PrototypePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float focusSpeed = 2f;

    [Header("Movement Bounds")]
    [SerializeField] private Vector2 minimumPosition = new(-3.5f, -4.5f);
    [SerializeField] private Vector2 maximumPosition = new(3.5f, 4.5f);

    [Header("Focus Mode")]
    [SerializeField] private SpriteRenderer hitboxRenderer;

    public bool IsFocused { get; private set; }
    public int GrazeCount { get; private set; }

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
        ReadFocusInput();
        UpdateHitboxVisibility();
        Move();
    }

    public void RegisterGraze()
    {
        GrazeCount++;

        Debug.Log($"Graze: {GrazeCount}");
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
        Vector2 input = new(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        input = Vector2.ClampMagnitude(input, 1f);

        float currentSpeed = IsFocused
            ? focusSpeed
            : normalSpeed;

        Vector2 nextPosition =
            (Vector2)transform.position +
            input * currentSpeed * Time.deltaTime;

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