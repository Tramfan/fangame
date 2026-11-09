using UnityEngine;

public sealed partial class Bullet
{
    private bool isTurning;
    private float turnSign;
    private float turnDegreesRemaining;
    private Vector2 turnCenter;

    private void Reflect(Vector2 playerCenter)
    {
        owner = BulletOwner.Player;

        Vector2 directionToPlayer = playerCenter - body.position;

        float cross =
            direction.x * directionToPlayer.y -
            direction.y * directionToPlayer.x;

        if (Mathf.Abs(cross) > 0.0001f)
        {
            turnSign = Mathf.Sign(cross);
        }
        else
        {
            turnSign = body.position.x >= playerCenter.x
                ? 1f
                : -1f;
        }

        Vector2 leftNormal = new(-direction.y, direction.x);

        turnCenter =
            body.position +
            leftNormal * turnSign * reflectionTurnRadius;

        turnDegreesRemaining = 180f;
        isTurning = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = reflectedColor;
        }
    }

    private Vector2 CalculateTurnPosition()
    {
        float angularSpeed =
            speed / reflectionTurnRadius * Mathf.Rad2Deg;

        float angleStep = Mathf.Min(
            angularSpeed * Time.fixedDeltaTime,
            turnDegreesRemaining
        );

        float signedAngle = angleStep * turnSign;

        Vector2 relativePosition = body.position - turnCenter;

        relativePosition = RotateVector(
            relativePosition,
            signedAngle
        );

        direction = RotateVector(
            direction,
            signedAngle
        ).normalized;

        turnDegreesRemaining -= angleStep;

        Vector2 nextPosition = turnCenter + relativePosition;

        if (turnDegreesRemaining <= 0.001f)
        {
            FinishReflectionTurn(nextPosition);
        }

        return nextPosition;
    }

    private void FinishReflectionTurn(Vector2 currentPosition)
    {
        isTurning = false;
        speed *= reflectedSpeedMultiplier;

        Transform target = source;

        if (target == null)
        {
            target = FindNearestEnemy(currentPosition);
        }

        if (target != null)
        {
            Vector2 directionToTarget =
                (Vector2)target.position - currentPosition;

            if (directionToTarget.sqrMagnitude > 0.0001f)
            {
                direction = directionToTarget.normalized;
                return;
            }
        }

        direction.Normalize();
    }

    private static Transform FindNearestEnemy(Vector2 currentPosition)
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        EnemyHealth nearestEnemy = null;
        float nearestDistanceSquared = float.PositiveInfinity;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            Vector2 offset =
                (Vector2)enemy.transform.position - currentPosition;

            float distanceSquared = offset.sqrMagnitude;

            if (distanceSquared >= nearestDistanceSquared)
            {
                continue;
            }

            nearestDistanceSquared = distanceSquared;
            nearestEnemy = enemy;
        }

        return nearestEnemy != null
            ? nearestEnemy.transform
            : null;
    }

    private static Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sine = Mathf.Sin(radians);
        float cosine = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cosine - vector.y * sine,
            vector.x * sine + vector.y * cosine
        );
    }
}