using UnityEngine;

public sealed class PrototypeHorizontalEnemy : MonoBehaviour
{
    [Header("Horizontal movement")]
    [SerializeField, Min(0f)]
    private float movementSpeed = 1.5f;

    [SerializeField]
    private float removalX = -5f;

    [Header("Vertical movement")]
    [SerializeField, Min(0f)]
    private float verticalAmplitude = 0f;

    [SerializeField, Min(0f)]
    private float verticalCyclesPerSecond = 0.5f;

    private float startY;
    private float elapsedTime;

    private void Awake()
    {
        startY = transform.position.y;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        Vector3 position = transform.position;

        position.x -= movementSpeed * Time.deltaTime;

        position.y =
            startY +
            verticalAmplitude *
            Mathf.Sin(
                elapsedTime *
                verticalCyclesPerSecond *
                Mathf.PI * 2f
            );

        transform.position = position;

        if (position.x <= removalX)
        {
            Destroy(gameObject);
        }
    }
}