using System.Collections.Generic;
using UnityEngine;

public sealed class FigureScatterAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform boss;

    [SerializeField]
    private Transform emitter;

    [SerializeField]
    private BulletPool bulletPool;

    [Header("Figure")]
    [SerializeField]
    private Vector2[] vertices =
    {
        new(-2.2f, 0.3f),
        new(-1.3f, 1.4f),
        new(0.8f, 1.4f),
        new(2.0f, 0.5f),
        new(1.3f, -1.2f),
        new(-1.4f, -1.2f)
    };

    [SerializeField, Min(0.01f)]
    private float bulletSpacing = 0.28f;

    [Header("Timing")]
    [SerializeField, Min(0.01f)]
    private float drawInterval = 0.05f;

    [SerializeField, Min(0f)]
    private float holdTime = 0.7f;

    [SerializeField, Min(0.01f)]
    private float releaseInterval = 0.045f;

    [SerializeField, Min(0f)]
    private float cooldown = 1f;

    [Header("Release")]
    [SerializeField, Min(0f)]
    private float minBulletSpeed = 2.5f;

    [SerializeField, Min(0f)]
    private float maxBulletSpeed = 3f;

    [SerializeField, Min(0f)]
    private float angleJitter = 25f;


    [Header("Cycles")]
    [SerializeField]
    private float rotationPerCycle = 20f;

    [SerializeField]
    private uint randomSeed = 123456789;

    private enum State
    {
        Drawing,
        Holding,
        Releasing,
        Cooldown
    }

    private readonly List<Vector2> sampledPoints =
        new();

    private readonly List<FigureScatterBullet>
        spawnedBullets = new();

    private State state;

    private Vector2 attackCenter;

    private int drawIndex;
    private int releaseIndex;
    private int cycleIndex;

    private float timer;
    private uint rngState;

    private void OnEnable()
    {
        if (boss == null ||
            emitter == null ||
            bulletPool == null)
        {
            Debug.LogError(
                "Figure Scatter Attack needs Boss, " +
                "Emitter and Bullet Pool.",
                this
            );

            enabled = false;
            return;
        }

        cycleIndex = 0;

        int runSeed =
            GameplayRandom.Instance != null
                ? GameplayRandom.Instance.Seed
                : 0;

        rngState =
            randomSeed ^
            unchecked((uint)runSeed);

        if (rngState == 0)
        {
            rngState = 0x6D2B79F5u;
        }

        BeginCycle();
    }

    private void OnDisable()
    {
        // Недорисованные пули возвращаются в пул.
        // Уже выпущенные продолжают лететь.
        foreach (FigureScatterBullet bullet
                 in spawnedBullets)
        {
            if (bullet != null)
            {
                bullet.ReturnIfWaiting();
            }
        }

        spawnedBullets.Clear();
        sampledPoints.Clear();
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        switch (state)
        {
            case State.Drawing:
                UpdateDrawing();
                break;

            case State.Holding:
                UpdateHolding();
                break;

            case State.Releasing:
                UpdateReleasing();
                break;

            case State.Cooldown:
                UpdateCooldown();
                break;
        }
    }

    private void BeginCycle()
    {
        attackCenter = boss.position;

        spawnedBullets.Clear();
        sampledPoints.Clear();

        float rotation =
            cycleIndex * rotationPerCycle;

        BuildFigurePoints(rotation);

        drawIndex = 0;
        releaseIndex = 0;
        timer = 0f;
        state = State.Drawing;

        if (sampledPoints.Count > 0)
        {
            emitter.position =
                attackCenter +
                sampledPoints[0];
        }
    }

    private void UpdateDrawing()
    {
        float interval =
            Mathf.Max(0.01f, drawInterval);

        while (timer >= interval)
        {
            timer -= interval;

            if (drawIndex >= sampledPoints.Count)
            {
                timer = 0f;
                state = State.Holding;
                return;
            }

            Vector2 worldPosition =
                attackCenter +
                sampledPoints[drawIndex];

            emitter.position = worldPosition;

            Bullet bullet = bulletPool.Spawn(
                worldPosition,
                Vector2.zero,
                0f,
                boss.root,
                true
            );

            if (bullet != null)
            {
                FigureScatterBullet movement =
                    bullet.GetComponent<
                        FigureScatterBullet
                    >();

                if (movement == null)
                {
                    movement =
                        bullet.gameObject.AddComponent<
                            FigureScatterBullet
                        >();
                }

           movement.Prepare(
    bullet,
    bulletPool
);

                spawnedBullets.Add(movement);
            }

            drawIndex++;
        }
    }

    private void UpdateHolding()
    {
        if (timer < Mathf.Max(0f, holdTime))
        {
            return;
        }

        timer = 0f;
        releaseIndex = 0;
        state = State.Releasing;
    }

    private void UpdateReleasing()
    {
        float interval =
            Mathf.Max(0.01f, releaseInterval);

        while (timer >= interval)
        {
            timer -= interval;

            if (releaseIndex >=
                spawnedBullets.Count)
            {
                timer = 0f;
                state = State.Cooldown;
                return;
            }

            int bulletIndex = releaseIndex;

            // Чередуем направление обхода фигуры.
            if ((cycleIndex & 1) != 0)
            {
                bulletIndex =
                    spawnedBullets.Count -
                    1 -
                    releaseIndex;
            }

            FigureScatterBullet bullet =
                spawnedBullets[bulletIndex];

            if (bullet != null &&
                bullet.IsAvailable)
            {
                ReleaseBullet(bullet);
            }

            releaseIndex++;
        }
    }

    private void UpdateCooldown()
    {
        if (timer < Mathf.Max(0f, cooldown))
        {
            return;
        }

        cycleIndex++;
        BeginCycle();
    }

    private void ReleaseBullet(
        FigureScatterBullet bullet
    )
    {
        Vector2 outward =
            (Vector2)bullet.transform.position -
            attackCenter;

        if (outward.sqrMagnitude <
            0.0001f)
        {
            outward = Vector2.down;
        }

        outward.Normalize();

        float jitter = RandomRange(
            -Mathf.Max(0f, angleJitter),
            Mathf.Max(0f, angleJitter)
        );

        Vector2 direction =
            Rotate(outward, jitter);

        float minimum =
            Mathf.Max(0f, minBulletSpeed);

        float maximum =
            Mathf.Max(
                minimum,
                maxBulletSpeed
            );

        float speed =
            RandomRange(
                minimum,
                maximum
            );

        bullet.Release(
            direction * speed
        );
    }

    private void BuildFigurePoints(
        float rotation
    )
    {
        if (vertices == null ||
            vertices.Length < 2)
        {
            return;
        }

        for (int index = 0;
             index < vertices.Length;
             index++)
        {
            Vector2 first = Rotate(
                vertices[index],
                rotation
            );

            Vector2 second = Rotate(
                vertices[
                    (index + 1) %
                    vertices.Length
                ],
                rotation
            );

            float distance =
                Vector2.Distance(
                    first,
                    second
                );

            int steps = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    distance /
                    Mathf.Max(
                        0.01f,
                        bulletSpacing
                    )
                )
            );

            for (int step = 0;
                 step < steps;
                 step++)
            {
                float progress =
                    step / (float)steps;

                sampledPoints.Add(
                    Vector2.Lerp(
                        first,
                        second,
                        progress
                    )
                );
            }
        }
    }

    private static Vector2 Rotate(
        Vector2 vector,
        float degrees
    )
    {
        float radians =
            degrees * Mathf.Deg2Rad;

        float cosine =
            Mathf.Cos(radians);

        float sine =
            Mathf.Sin(radians);

        return new Vector2(
            vector.x * cosine -
            vector.y * sine,
            vector.x * sine +
            vector.y * cosine
        );
    }

    private uint NextRandom()
    {
        uint value = rngState;

        value ^= value << 13;
        value ^= value >> 17;
        value ^= value << 5;

        rngState = value;

        return value;
    }

    private float RandomRange(
        float minimum,
        float maximum
    )
    {
        float t =
            (NextRandom() & 0x00FFFFFF) /
            16777216f;

        return Mathf.Lerp(
            minimum,
            maximum,
            t
        );
    }
}