using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public sealed class HoverReturnEnemyMover : MonoBehaviour
{
    [Header("Route")]
    [SerializeField]
    [Tooltip("0 right, 90 up, 180 left, -90 down.")]
    private float approachAngleDegrees = -90f;

    [SerializeField, Min(0f)]
    private float approachDistance = 1.5f;

    [Header("Timing (fixed ticks)")]
    [SerializeField, Min(1)]
    private int approachTicks = 60;

    [SerializeField, Min(1)]
    private int hoverTicks = 120;

    [SerializeField, Min(1)]
    private int returnTicks = 60;

    private Vector3 startPosition;
    private Vector3 hoverPosition;
    private int elapsedTicks;

    public bool IsHovering =>
        elapsedTicks >= Mathf.Max(1, approachTicks) &&
        elapsedTicks <
        Mathf.Max(1, approachTicks) +
        Mathf.Max(1, hoverTicks);

    private void OnEnable()
    {
        startPosition = transform.position;

        float radians =
            approachAngleDegrees * Mathf.Deg2Rad;

        hoverPosition = startPosition +
            new Vector3(
                Mathf.Cos(radians),
                Mathf.Sin(radians),
                0f
            ) * Mathf.Max(0f, approachDistance);

        elapsedTicks = 0;
    }

    private void FixedUpdate()
    {
        elapsedTicks++;

        int approach = Mathf.Max(1, approachTicks);
        int hover = Mathf.Max(1, hoverTicks);
        int retreat = Mathf.Max(1, returnTicks);

        if (elapsedTicks <= approach)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                hoverPosition,
                elapsedTicks / (float)approach
            );

            return;
        }

        if (elapsedTicks <= approach + hover)
        {
            transform.position = hoverPosition;
            return;
        }

        float returnProgress =
            (elapsedTicks - approach - hover) /
            (float)retreat;

        transform.position = Vector3.Lerp(
            hoverPosition,
            startPosition,
            returnProgress
        );

        if (elapsedTicks >=
            approach + hover + retreat)
        {
            Destroy(gameObject);
        }
    }
}