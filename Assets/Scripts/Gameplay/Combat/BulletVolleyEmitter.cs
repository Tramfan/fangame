using UnityEngine;

[DisallowMultipleComponent]
public sealed class BulletVolleyEmitter : MonoBehaviour
{
    public enum VolleyShape
    {
        Ring,
        Fan
    }

    [Header("References")]
    [SerializeField]
    private BulletPool bulletPool;

[Header("Difficulty")]
    [SerializeField]
    [Tooltip("Минимальная сложность, на которой срабатывает этот эмиттер.")]
    private GameDifficulty minimumDifficulty = GameDifficulty.Normal;

    [Header("Appearance")]
    [SerializeField]
    private BulletStyleDefinition bulletStyle;

    [SerializeField]
    private BulletColorPreset bulletColor = BulletColorPreset.Custom;

    [SerializeField]
    [Tooltip("Используется, если выбран Bullet Color = Custom.")]
    private Color bulletTint = Color.white;

    [Header("Spawn Appearance")]
    [SerializeField, Min(0f)]
    [Tooltip("Відстань від точки появи до початку проявлення кулі.")]
    private float spawnHiddenDistance;

    [SerializeField, Min(0f)]
    [Tooltip("Відстань, за яку куля стає повністю видимою та небезпечною.")]
    private float spawnFadeDistance;

    [Header("Volley")]
    [SerializeField]
    private VolleyShape shape = VolleyShape.Ring;

    [SerializeField]
    private DifficultyInt bulletCount = new(12, 16, 22);

    [SerializeField]
    private DifficultyFloat bulletSpeed = new(2.2f, 2.5f, 3.0f);

    [SerializeField]
    private float baseAngleDegrees = -90f;

    [SerializeField]
    private DifficultyFloat fanSpreadDegrees = new(50f, 60f, 75f);

    [Header("Speed Layers")]
    [SerializeField]
    [Tooltip("Кількість куль із різними швидкостями в кожному напрямку.")]
    private DifficultyInt speedLayerCount = new(1);

    [SerializeField]
    [Tooltip("Приріст початкової швидкості між сусідніми шарами.")]
    private DifficultyFloat speedLayerStep = new(0.5f);

    [Header("Random Variation")]
    [SerializeField, Min(0)]
    private int bulletCountJitter;

    [SerializeField, Min(0f)]
    private float angleJitterDegrees;

    [SerializeField, Min(0f)]
    private float speedJitter;

    [Header("Groups")]
    [SerializeField]
    private DifficultyInt groupCount = new(1);

    [SerializeField]
    private float groupAngleStepDegrees = 30f;

    [Header("Aiming")]
    [SerializeField]
    private bool aimAtPlayer;

    [SerializeField]
    private Transform target;

    [Header("Bullet Movement")]
    [SerializeField]
    private BulletMotionDefinition bulletMotion;

    [Header("Child Volley")]
    [SerializeField]
    private BulletVolleyEmitter childEmitter;

    [SerializeField]
    private DifficultyInt childVolleyDelayTicks = new(45);

    [SerializeField]
    private bool returnAfterChildVolley = true;

    private bool overrideOrbitDirection;
    private Transform orbitCenter;
    private Transform orbitAnchor;

    internal bool AimsAtPlayer => aimAtPlayer;

    internal void ConfigureOrbitDirection(
        bool overrideDirection,
        Transform center,
        Transform outwardAnchor
    )
    {
        overrideOrbitDirection = overrideDirection;
        orbitCenter = center;
        orbitAnchor = outwardAnchor;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetBulletPool(BulletPool newBulletPool)
    {
        bulletPool = newBulletPool;
    }

    public void Fire(float angleOffsetDegrees = 0f)
    {
        FireAt(
            transform.position,
            transform.root,
            angleOffsetDegrees,
            fromEmitter: true
        );
    }

    public bool FireAt(
        Vector2 origin,
        Transform source,
        float angleOffsetDegrees = 0f
    )
    {
        return FireAt(
            origin,
            source,
            angleOffsetDegrees,
            fromEmitter: false
        );
    }

    private bool FireAt(
        Vector2 origin,
        Transform source,
        float angleOffsetDegrees,
        bool fromEmitter
    )
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return false;
        }

        GameDifficulty difficulty = GameRunContext.Difficulty;

        if (difficulty < minimumDifficulty)
        {
            return false;
        }

        if (bulletPool == null)
        {
            Debug.LogError(
                "Bullet Volley Emitter has no Bullet Pool assigned.",
                this
            );
            return false;
        }

        float directionAngle = baseAngleDegrees;

        if (fromEmitter && overrideOrbitDirection)
        {
            // Повторювач задає ритм, а вісь залпу задає режим орбіти.
            angleOffsetDegrees = 0f;

            if (orbitCenter != null && orbitAnchor != null)
            {
                Vector2 outward = orbitAnchor.position - orbitCenter.position;

                if (outward.sqrMagnitude > 0f)
                {
                    directionAngle =
                        Mathf.Atan2(outward.y, outward.x) * Mathf.Rad2Deg;
                }
            }
        }
        else if (aimAtPlayer)
        {
            if (target == null)
            {
                Debug.LogError(
                    "Bullet Volley Emitter has no target assigned.",
                    this
                );
                return false;
            }

            Vector2 toTarget = (Vector2)target.position - origin;

            if (toTarget.sqrMagnitude > 0.000001f)
            {
                directionAngle =
                    Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            }
        }

        GameplayRandom random = GameplayRandom.Instance;
        int count = bulletCount.ResolveMin(difficulty, 1);

        if (random != null && bulletCountJitter > 0)
        {
            int minCount = Mathf.Max(1, count - bulletCountJitter);

            int maxCount = (int)System.Math.Min(
                (long)count + bulletCountJitter,
                int.MaxValue - 1L
            );

            // Верхня межа цілочисельного Range не входить у діапазон.
            count = random.Range(minCount, maxCount + 1);
        }

        int groups = groupCount.ResolveMin(difficulty, 1);
        int layers = speedLayerCount.ResolveMin(difficulty, 1);
        float layerSpeedStep = speedLayerStep.ResolveMin(difficulty, 0f);
        float resolvedBaseSpeed = bulletSpeed.ResolveMin(difficulty, 0f);
        int resolvedChildDelay = childVolleyDelayTicks.ResolveMin(difficulty, 1);
        Color resolvedTint = BulletColorUtility.Resolve(bulletColor, bulletTint);

        float firstAngle = directionAngle + angleOffsetDegrees;
        float bulletAngleStep = 0f;

        if (shape == VolleyShape.Ring)
        {
            bulletAngleStep = 360f / count;
        }
        else if (count > 1)
        {
            float spread = fanSpreadDegrees.ResolveClamped(
                difficulty,
                0f,
                360f
            );

            firstAngle -= spread * 0.5f;
            bulletAngleStep = spread / (count - 1);
        }

        bool spawnedAny = false;

        for (int groupIndex = 0; groupIndex < groups; groupIndex++)
        {
            float groupAngleOffset =
                groupAngleStepDegrees * groupIndex;

            for (int bulletIndex = 0;
                 bulletIndex < count;
                 bulletIndex++)
            {
                // Усі шари одного стека мають спільний напрямок.
                float angle =
                    firstAngle +
                    groupAngleOffset +
                    bulletAngleStep * bulletIndex +
                    RandomOffset(random, angleJitterDegrees);

                float radians = angle * Mathf.Deg2Rad;

                Vector2 direction = new(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                );

                // Спільна випадкова поправка зберігає крок швидкості.
                float stackBaseSpeed = Mathf.Max(
                    0f,
                    resolvedBaseSpeed + RandomOffset(random, speedJitter)
                );

                for (int layerIndex = 0;
                     layerIndex < layers;
                     layerIndex++)
                {
                    float speed =
                        stackBaseSpeed +
                        layerSpeedStep * layerIndex;

                    Bullet bullet = bulletPool.Spawn(
                        origin,
                        direction,
                        speed,
                        source,
                        style: bulletStyle,
                        tint: resolvedTint,
                        motion: bulletMotion
                    );

                    if (bullet == null)
                    {
                        return spawnedAny;
                    }

                    bullet.ConfigureSpawnAppearance(
                        spawnHiddenDistance,
                        spawnFadeDistance
                    );

                    spawnedAny = true;

                    if (childEmitter != null)
                    {
                        bullet.ArmTimedVolley(
                            childEmitter,
                            resolvedChildDelay,
                            returnAfterChildVolley
                        );
                    }
                }
            }
        }

        return spawnedAny;
    }

    private static float RandomOffset(
        GameplayRandom random,
        float maximum
    )
    {
        if (random == null || maximum <= 0f)
        {
            return 0f;
        }

        return random.Range(-maximum, maximum);
    }

    [ContextMenu("Fire Test Volley")]
    private void FireTestVolley()
    {
        Fire();
    }
}