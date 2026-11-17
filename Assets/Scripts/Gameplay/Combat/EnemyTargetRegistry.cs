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

            if (distanceSquared >
                nearestDistanceSquared)
            {
                continue;
            }

            if (distanceSquared == nearestDistanceSquared &&
                nearestEnemy != null &&
                CompareHierarchyOrder(
                    enemy.transform,
                    nearestEnemy.transform
                ) >= 0)
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
        EnemyHealth bestEnemy = null;
        float bestEntryT = float.PositiveInfinity;
        float bestStartDistSq = float.PositiveInfinity;

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

            float combinedRadius =
                Mathf.Max(0f, bulletRadius) +
                Mathf.Max(0f, enemy.HitRadius);

            if (!TryGetSegmentCircleEntryT(
                    segmentStart,
                    segmentEnd,
                    enemy.WorldPosition,
                    combinedRadius,
                    out float entryT,
                    out float startDistSq
                ))
            {
                continue;
            }

            if (IsBetterHit(
                    entryT,
                    startDistSq,
                    enemy,
                    bestEntryT,
                    bestStartDistSq,
                    bestEnemy
                ))
            {
                bestEntryT = entryT;
                bestStartDistSq = startDistSq;
                bestEnemy = enemy;
            }
        }

        return bestEnemy;
    }

    private static bool TryGetSegmentCircleEntryT(
        Vector2 segmentStart,
        Vector2 segmentEnd,
        Vector2 circleCenter,
        float radius,
        out float entryT,
        out float startDistSq
    )
    {
        Vector2 m = segmentStart - circleCenter;
        startDistSq = m.sqrMagnitude;
        float radiusSq = radius * radius;

        if (startDistSq <= radiusSq)
        {
            entryT = 0f;
            return true;
        }

        Vector2 d = segmentEnd - segmentStart;
        float a = d.sqrMagnitude;

        if (a <= 0.0000000001f)
        {
            entryT = 0f;
            return false;
        }

        float b = Vector2.Dot(m, d);

        // Відрізок починається зовні та віддаляється від центра кола.
        if (b >= 0f)
        {
            entryT = 0f;
            return false;
        }

        float c = startDistSq - radiusSq;
        float discriminant = b * b - a * c;

        if (discriminant < 0f)
        {
            entryT = 0f;
            return false;
        }

        float t = (-b - Mathf.Sqrt(discriminant)) / a;

        if (t < 0f || t > 1f)
        {
            entryT = 0f;
            return false;
        }

        entryT = t;
        return true;
    }

    private static bool IsBetterHit(
        float candidateT,
        float candidateStartDistSq,
        EnemyHealth candidate,
        float bestT,
        float bestStartDistSq,
        EnemyHealth best
    )
    {
        if (best == null)
        {
            return true;
        }

        int entryComparison = candidateT.CompareTo(bestT);
        if (entryComparison != 0)
        {
            return entryComparison < 0;
        }

        int distanceComparison =
            candidateStartDistSq.CompareTo(bestStartDistSq);
        if (distanceComparison != 0)
        {
            return distanceComparison < 0;
        }

        return CompareHierarchyOrder(
            candidate.transform,
            best.transform
        ) < 0;
    }

    private static int CompareHierarchyOrder(
        Transform left,
        Transform right
    )
    {
        int leftDepth = GetHierarchyDepth(left);
        int rightDepth = GetHierarchyDepth(right);
        Transform leftAncestor = left;
        Transform rightAncestor = right;

        for (int depth = leftDepth; depth > rightDepth; depth--)
        {
            leftAncestor = leftAncestor.parent;
        }

        for (int depth = rightDepth; depth > leftDepth; depth--)
        {
            rightAncestor = rightAncestor.parent;
        }

        if (leftAncestor == rightAncestor)
        {
            return leftDepth.CompareTo(rightDepth);
        }

        // Порівнюємо перших різних предків, враховуючи весь шлях.
        while (leftAncestor.parent != rightAncestor.parent)
        {
            leftAncestor = leftAncestor.parent;
            rightAncestor = rightAncestor.parent;
        }

        return leftAncestor.GetSiblingIndex().CompareTo(
            rightAncestor.GetSiblingIndex()
        );
    }

    private static int GetHierarchyDepth(Transform target)
    {
        int depth = 0;
        for (Transform parent = target.parent;
             parent != null;
             parent = parent.parent)
        {
            depth++;
        }

        return depth;
    }
}
