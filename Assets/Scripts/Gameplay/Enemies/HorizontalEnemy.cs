using UnityEngine;

public sealed class HorizontalEnemyMover : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField, Min(0f)]
    private float movementSpeed = 1.5f;

    [SerializeField]
    private float removalX = -5f;

    [Header("Vertical Movement")]
    [SerializeField, Min(0f)]
    private float verticalAmplitude;

    [SerializeField, Min(0f)]
    private float verticalCyclesPerSecond = 0.5f;

    private float startY;
    private int elapsedTicks;

    private void OnEnable()
    {
        startY = transform.position.y;
        elapsedTicks = 0;
    }

    private void FixedUpdate()
    {
        elapsedTicks++;

        Vector3 position = transform.position;

        position.x -=
            movementSpeed * Time.fixedDeltaTime;

        float elapsedTime =
            elapsedTicks * Time.fixedDeltaTime;

        position.y =
            startY +
            verticalAmplitude *
            Mathf.Sin(
                elapsedTime *
                verticalCyclesPerSecond *
                Mathf.PI *
                2f
            );

        transform.position = position;

        if (position.x <= removalX)
        {
            Destroy(gameObject);
        }
    }
}