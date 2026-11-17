using UnityEngine;

public static class BulletHitboxMath
{
    public static bool OverlapsCircle(
        Vector2 centerA,
        float radiusA,
        Vector2 centerB,
        float radiusB
    )
    {
        float combinedRadius = radiusA + radiusB;

        if (combinedRadius <= 0f)
        {
            return false;
        }

        float deltaX = centerA.x - centerB.x;
        float deltaY = centerA.y - centerB.y;

        // Дешёвый Broadphase-отсев до умножений.
        if (Mathf.Abs(deltaX) > combinedRadius ||
            Mathf.Abs(deltaY) > combinedRadius)
        {
            return false;
        }

        return deltaX * deltaX + deltaY * deltaY <=
            combinedRadius * combinedRadius;
    }

    public static bool SegmentOverlapsCircle(
        Vector2 segmentStart,
        Vector2 segmentEnd,
        float segmentRadius,
        Vector2 circleCenter,
        float circleRadius
    )
    {
        float combinedRadius = segmentRadius + circleRadius;

        if (combinedRadius <= 0f)
        {
            return false;
        }

        Vector2 segment = segmentEnd - segmentStart;
        float segmentLengthSquared = segment.sqrMagnitude;

        if (segmentLengthSquared <= 0.00000001f)
        {
            return OverlapsCircle(
                segmentStart,
                segmentRadius,
                circleCenter,
                circleRadius
            );
        }

        Vector2 toCenter = circleCenter - segmentStart;
        float t = Mathf.Clamp01(
            Vector2.Dot(toCenter, segment) / segmentLengthSquared
        );

        Vector2 closestPoint = segmentStart + segment * t;

        return OverlapsCircle(
            closestPoint,
            segmentRadius,
            circleCenter,
            circleRadius
        );
    }
}