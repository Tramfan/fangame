using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerAutoCollect :
    MonoBehaviour
{
    [SerializeField, Range(0.01f, 0.99f)]
    private float collectionLineViewportY =
        0.8f;

    [SerializeField]
    private Transform attractionTarget;

    private Camera gameplayCamera;

    public float CollectionLineViewportY =>
        collectionLineViewportY;

    private void Awake()
    {
        gameplayCamera = Camera.main;

        if (attractionTarget == null)
        {
            attractionTarget = transform;
        }

        if (gameplayCamera == null)
        {
            Debug.LogError(
                "Player Auto Collect cannot find " +
                "the Main Camera.",
                this
            );

            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        Vector3 viewportPosition =
            gameplayCamera.WorldToViewportPoint(
                transform.position
            );

        if (viewportPosition.y <
            collectionLineViewportY)
        {
            return;
        }

        PickupMovement.AttractAllTo(
            attractionTarget
        );
    }

    private void OnValidate()
    {
        collectionLineViewportY =
            Mathf.Clamp(
                collectionLineViewportY,
                0.01f,
                0.99f
            );
    }
}