using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class PrototypeEnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float removalPositionY = -5.5f;

    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition =
            body.position +
            Vector2.down * speed * Time.fixedDeltaTime;

        body.MovePosition(nextPosition);

        if (nextPosition.y < removalPositionY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Player hit.");

        Destroy(gameObject);
    }
}