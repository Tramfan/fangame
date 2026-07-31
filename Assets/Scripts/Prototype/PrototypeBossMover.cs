using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PrototypeBossMover :
    MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    private float speed = 3f;

    [SerializeField, Min(0.0001f)]
    private float arrivalDistance = 0.01f;

    private Vector3 destination;

    public bool IsMoving { get; private set; }

    public event Action DestinationReached;

    public void MoveTo(Vector2 worldPosition)
    {
        destination = new Vector3(
            worldPosition.x,
            worldPosition.y,
            transform.position.z
        );

        if (HasReachedDestination())
        {
            transform.position = destination;
            CompleteMovement();
            return;
        }

        IsMoving = true;
    }

    private void FixedUpdate()
    {
        if (!IsMoving)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * Time.fixedDeltaTime
        );

        if (!HasReachedDestination())
        {
            return;
        }

        transform.position = destination;
        CompleteMovement();
    }

    private bool HasReachedDestination()
    {
        float maximumDistanceSquared =
            arrivalDistance * arrivalDistance;

        return (
            transform.position - destination
        ).sqrMagnitude <= maximumDistanceSquared;
    }

    private void CompleteMovement()
    {
        IsMoving = false;
        DestinationReached?.Invoke();
    }
}