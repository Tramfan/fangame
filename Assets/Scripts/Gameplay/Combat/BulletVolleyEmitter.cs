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

    [Header("Appearance")]
    [SerializeField]
    private BulletStyleDefinition bulletStyle;

    [SerializeField]
    private Color bulletTint = Color.white;

    [Header("Volley")]
    [SerializeField]
    private VolleyShape shape = VolleyShape.Ring;

    [SerializeField, Min(1)]
    private int bulletCount = 16;

    [SerializeField, Min(0f)]
    private float bulletSpeed = 2.5f;

    [SerializeField]
    private float baseAngleDegrees = -90f;

    [SerializeField, Range(0f, 360f)]
    private float fanSpreadDegrees = 60f;

    [Header("Random Variation")]
    [SerializeField, Min(0)]
    private int bulletCountJitter;

    [SerializeField, Min(0f)]
    private float angleJitterDegrees;

    [SerializeField, Min(0f)]
    private float speedJitter;

    [Header("Groups")]
    [SerializeField, Min(1)]
    private int groupCount = 1;

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

    [SerializeField, Min(1)]
    private int childVolleyDelayTicks = 45;

    [SerializeField]
    private bool returnAfterChildVolley = true;

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
        FireAt(transform.position, transform.root, angleOffsetDegrees);
    }

    public bool FireAt(
        Vector2 origin,
        Transform source,
        float angleOffsetDegrees = 0f
    )
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
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

        if (aimAtPlayer)
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
        int count = Mathf.Max(1, bulletCount);

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

        int groups = Mathf.Max(1, groupCount);
        float firstAngle = directionAngle + angleOffsetDegrees;
        float bulletAngleStep = 0f;

        if (shape == VolleyShape.Ring)
        {
            bulletAngleStep = 360f / count;
        }
        else if (count > 1)
        {
            float spread = Mathf.Clamp(fanSpreadDegrees, 0f, 360f);
            firstAngle -= spread * 0.5f;
            bulletAngleStep = spread / (count - 1);
        }

        bool spawnedAny = false;

        for (int groupIndex = 0; groupIndex < groups; groupIndex++)
        {
            float groupAngleOffset = groupAngleStepDegrees * groupIndex;

            for (int bulletIndex = 0; bulletIndex < count; bulletIndex++)
            {
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

                float speed = Mathf.Max(
                    0f,
                    bulletSpeed + RandomOffset(random, speedJitter)
                );

                Bullet bullet = bulletPool.Spawn(
                    origin,
                    direction,
                    speed,
                    source,
                    style: bulletStyle,
                    tint: bulletTint,
                    motion: bulletMotion
                );

                if (bullet == null)
                {
                    return spawnedAny;
                }

                spawnedAny = true;

                if (childEmitter != null)
                {
                    bullet.ArmTimedVolley(
                        childEmitter,
                        childVolleyDelayTicks,
                        returnAfterChildVolley
                    );
                }
            }
        }

        return spawnedAny;
    }

    private static float RandomOffset(GameplayRandom random, float maximum)
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