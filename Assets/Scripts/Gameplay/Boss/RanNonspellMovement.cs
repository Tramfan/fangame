using UnityEngine;

[DisallowMultipleComponent]
public sealed class RanNonspellMovement : MonoBehaviour
{
    [SerializeField]
    private Transform movementRoot;

    [Header("Cycle")]
    [SerializeField, Min(1)]
    private int cycleTicks = 120;

    [SerializeField, Min(1)]
    private int moveTicks = 60;

    [SerializeField, Min(0f)]
    private float stepDistance = 1.1f;

    [SerializeField, Range(0f, 90f)]
    private float angleSpread = 45f;

    [Header("World Bounds")]
    [SerializeField]
    private Vector2 minimumPosition = new(-3f, 1.9f);

    [SerializeField]
    private Vector2 maximumPosition = new(3f, 3.5f);

    private int tickInCycle;
    private Vector3 startPosition;
    private Vector3 destination;

    private void OnEnable()
    {
        tickInCycle = 0;
    }

    private void FixedUpdate()
    {
        if (movementRoot == null || GameplayRandom.Instance == null)
        {
            Debug.LogError(
                "Ran Nonspell Movement needs a Movement Root and Gameplay Random.",
                this
            );
            enabled = false;
            return;
        }

        int duration = Mathf.Max(1, moveTicks);
        int period = Mathf.Max(duration, cycleTicks);

        if (tickInCycle == 0)
        {
            ChooseDestination();
        }

        if (tickInCycle < duration)
        {
            float progress = (tickInCycle + 1) / (float)duration;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            movementRoot.position = Vector3.Lerp(
                startPosition,
                destination,
                easedProgress
            );
        }

        tickInCycle = (tickInCycle + 1) % period;
    }

    private void ChooseDestination()
    {
        startPosition = movementRoot.position;

        Vector2 min = Vector2.Min(minimumPosition, maximumPosition);
        Vector2 max = Vector2.Max(minimumPosition, maximumPosition);

        PlayerArea grazeArea = PlayerArea.GrazeArea;
        float playerX = grazeArea != null
            ? grazeArea.Center.x
            : startPosition.x;

        float leftLimit = Mathf.Lerp(min.x, max.x, 0.2f);
        float rightLimit = Mathf.Lerp(min.x, max.x, 0.8f);

        bool moveLeft = startPosition.x > rightLimit ||
            (startPosition.x > leftLimit && playerX < startPosition.x);

        float sectorCenter = moveLeft ? 180f : 0f;
        float offset = GameplayRandom.Instance.Range(-angleSpread, angleSpread);
        float angle = (sectorCenter + offset) * Mathf.Deg2Rad;

        destination = new Vector3(
            Mathf.Clamp(startPosition.x + Mathf.Cos(angle) * stepDistance, min.x, max.x),
            Mathf.Clamp(startPosition.y + Mathf.Sin(angle) * stepDistance, min.y, max.y),
            startPosition.z
        );
    }
}