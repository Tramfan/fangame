using UnityEngine;

public sealed class PrototypePlayerController : MonoBehaviour
{
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float focusSpeed = 2f;

    [SerializeField] private Vector2 minimumPosition = new(-3.5f, -4.5f);
    [SerializeField] private Vector2 maximumPosition = new(3.5f, 4.5f);

    public bool IsFocused { get; private set; }

    private void Update()
    {
        Vector2 input = new(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        input = Vector2.ClampMagnitude(input, 1f);

        IsFocused =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        float currentSpeed = IsFocused ? focusSpeed : normalSpeed;

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