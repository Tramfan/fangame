using UnityEngine;

[DisallowMultipleComponent]
public sealed class LoopingBackgroundLayer : MonoBehaviour
{
    [SerializeField]
    private Transform[] tiles;

    [SerializeField, Min(0f)]
    [Tooltip("World units per second.")]
    private float scrollSpeed = 0.2f;

    [SerializeField]
    [Tooltip("The direction in which the image moves.")]
    private Vector2 scrollDirection = Vector2.down;

    [SerializeField, Min(0.01f)]
    [Tooltip("Height of one repeated image along Scroll Direction.")]
    private float tileLength = 10.24f;

    private Vector2 normalizedDirection;

    private void Awake()
    {
        normalizedDirection =
            scrollDirection.sqrMagnitude > 0.0001f
                ? scrollDirection.normalized
                : Vector2.down;
    }

    private void Update()
    {
        if (tiles == null || tiles.Length < 2 ||
            scrollSpeed <= 0f)
        {
            return;
        }

        Vector3 movement =
            (Vector3)(normalizedDirection *
                scrollSpeed * Time.deltaTime);

        foreach (Transform tile in tiles)
        {
            if (tile == null)
            {
                continue;
            }

            tile.localPosition += movement;

            float distanceAlongScroll =
                Vector2.Dot(
                    tile.localPosition,
                    normalizedDirection
                );

            while (distanceAlongScroll >= tileLength)
            {
                tile.localPosition -=
                    (Vector3)(normalizedDirection *
                        tileLength * tiles.Length);

                distanceAlongScroll -=
                    tileLength * tiles.Length;
            }
        }
    }
}