using System.Collections.Generic;
using UnityEngine;

public static class EnemyTargetRegistry
{
    private static readonly List<EnemyHealth>
        activeEnemies = new();

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetRegistry()
    {
        activeEnemies.Clear();
    }

    public static void Register(
        EnemyHealth enemy
    )
    {
        if (enemy == null ||
            activeEnemies.Contains(enemy))
        {
            return;
        }

        activeEnemies.Add(enemy);
    }

    public static void Unregister(
        EnemyHealth enemy
    )
    {
        if (enemy == null)
        {
            return;
        }

        activeEnemies.Remove(enemy);
    }

    public static Transform FindNearest(
        Vector2 position
    )
    {
        EnemyHealth nearestEnemy = null;

        float nearestDistanceSquared =
            float.PositiveInfinity;

        for (int index = activeEnemies.Count - 1;
             index >= 0;
             index--)
        {
            EnemyHealth enemy =
                activeEnemies[index];

            if (enemy == null)
            {
                activeEnemies.RemoveAt(index);
                continue;
            }

            if (!enemy.isActiveAndEnabled ||
                enemy.IsDead ||
                enemy.IsInvulnerable)
            {
                continue;
            }

            Vector2 offset =
                (Vector2)enemy.transform.position -
                position;

            float distanceSquared =
                offset.sqrMagnitude;

            if (distanceSquared >=
                nearestDistanceSquared)
            {
                continue;
            }

            nearestDistanceSquared =
                distanceSquared;

            nearestEnemy = enemy;
        }

        return nearestEnemy != null
            ? nearestEnemy.transform
            : null;
    }
   
public static EnemyHealth FindFirstHit(
    Vector2 segmentStart,
    Vector2 segmentEnd,
    float bulletRadius
)
{
    for (int index = 0; index < activeEnemies.Count; index++)
    {
        EnemyHealth enemy = activeEnemies[index];

        if (enemy == null ||
            !enemy.isActiveAndEnabled ||
            enemy.IsDead ||
            enemy.IsInvulnerable)
        {
            continue;
        }

        if (BulletHitboxMath.SegmentOverlapsCircle(
                segmentStart,
                segmentEnd,
                bulletRadius,
                enemy.WorldPosition,
                enemy.HitRadius
            ))
        {
            return enemy;
        }
    }

    return null;
}
}