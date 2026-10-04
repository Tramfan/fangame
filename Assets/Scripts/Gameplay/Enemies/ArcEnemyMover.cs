using UnityEngine;

[DisallowMultipleComponent]
public sealed class ArcEnemyMover : MonoBehaviour
{
    private enum TravelDirection
    {
        SideToSide,
        TopToBottom,
        ByAngle
    }

    [SerializeField]
    private TravelDirection travelDirection =
        TravelDirection.SideToSide;

    [SerializeField, Min(1)]
    private int travelTicks = 180;

    [SerializeField, Min(0f)]
    private float downwardArcDepth = 1.2f;

    [SerializeField]
    private float bottomExitY = -5.5f;

    [SerializeField, Min(0f)]
    [Tooltip("The vertical route bends toward the centre of the screen.")]
    private float inwardArcDepth = 0.9f;

    [Header("Angle Route")]
    [SerializeField]
    [Tooltip("0 right, 90 up, 180 left, -90 down.")]
    private float travelAngleDegrees = -90f;

    [SerializeField, Min(0f)]
    private float travelDistance = 10.9f;

    [SerializeField]
    [Tooltip("Signed bend perpendicular to travel; 0 gives a straight path.")]
    private float arcOffset;

    private Vector3 startPosition;
    private int elapsedTicks;

    private void OnEnable()
    {
        startPosition = transform.position;
        elapsedTicks = 0;
    }

    public void ConfigureAngle(
        float angleDegrees,
        float distance,
        float bend,
        int durationTicks
    )
    {
        travelDirection = TravelDirection.ByAngle;
        travelAngleDegrees = angleDegrees;
        travelDistance = Mathf.Max(0f, distance);
        arcOffset = bend;
        travelTicks = Mathf.Max(1, durationTicks);
    }

    private void FixedUpdate()
    {
        elapsedTicks++;

        float progress = Mathf.Clamp01(
            elapsedTicks / (float)Mathf.Max(1, travelTicks)
        );

        Vector3 position = startPosition;

        float arc = Mathf.Sin(progress * Mathf.PI);

        if (travelDirection == TravelDirection.ByAngle)
        {
            float radians =
                travelAngleDegrees * Mathf.Deg2Rad;

            Vector2 forward = new(
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            );

            Vector2 sideways = new(
                -forward.y,
                forward.x
            );

            Vector2 offset =
                forward * travelDistance * progress +
                sideways * arcOffset * arc;

            position.x += offset.x;
            position.y += offset.y;
        }
        else if (travelDirection ==
            TravelDirection.TopToBottom)
        {
            position.y = Mathf.Lerp(
                startPosition.y,
                bottomExitY,
                progress
            );

            position.x -=
                Mathf.Sign(startPosition.x) *
                inwardArcDepth * arc;
        }
        else
        {
            position.x = Mathf.Lerp(
                startPosition.x,
                -startPosition.x,
                progress
            );

            position.y -=
                downwardArcDepth * arc;
        }

        transform.position = position;

        if (elapsedTicks >= travelTicks)
        {
            Destroy(gameObject);
        }
    }
}
