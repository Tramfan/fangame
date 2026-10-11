using UnityEngine;

public sealed class AimedFanEmitter : MonoBehaviour
{
    [SerializeField]
    private BulletPool bulletPool;

    [SerializeField]
    private Transform target;

    [Header("Fan Shape")]
    [SerializeField, Min(1)]
    private int bulletCount = 5;

    [SerializeField, Range(0f, 360f)]
    [Tooltip("Total angle between the outer bullets.")]
    private float spreadAngle = 60f;

    [SerializeField, Min(0f)]
    [Tooltip("Maximum random angle added to each bullet.")]
    private float angleJitterDegrees;

    [Header("Speed")]
    [SerializeField, Min(0f)]
    private float bulletSpeed = 3f;

    [SerializeField, Min(1)]
    [Tooltip("Each fan bullet is repeated once for every layer.")]
    private int speedLayerCount = 1;

    [SerializeField]
    [Tooltip("Speed added by every layer after the first.")]
    private float speedLayerStep;

    [SerializeField, Min(0f)]
    [Tooltip("Maximum random speed added or removed from each bullet.")]
    private float speedJitter;

    [Header("Fan Release")]
    [SerializeField, Min(1)]
    [Tooltip("Fixed ticks used to release one fan burst.")]
    private int fanReleaseTicks = 1;

    [Header("Burst Series")]
    [SerializeField, Min(1)]
    [Tooltip("How many fan bursts form one shot series.")]
    private int burstCount = 1;

    [SerializeField, Min(1)]
    [Tooltip("Fixed ticks between bursts in one series.")]
    private int burstIntervalTicks = 8;

    [SerializeField, Min(1)]
    [Tooltip("Fixed ticks after a completed series before the next one.")]
    private int fireIntervalTicks = 90;

    [SerializeField, Min(0)]
    [Tooltip("Wait this many fixed ticks before the first shot.")]
    private int firstShotDelayTicks;

    [SerializeField]
    [Tooltip("Stop after the first complete burst series.")]
    private bool fireOnlyOnce;

    private int ticksUntilNextAction;
    private int burstsRemaining;
    private int releaseChunksRemaining;
    private int nextBulletIndex;
    private int activeBulletCount;
    private float activeFirstAngle;
    private float activeAngleStep;
    private bool releasingFan;
    private bool initialSeriesStarted;

    private void OnEnable()
    {
        ticksUntilNextAction =
            Mathf.Max(0, firstShotDelayTicks);

        burstsRemaining = 0;
        releaseChunksRemaining = 0;
        nextBulletIndex = 0;
        activeBulletCount = 0;
        releasingFan = false;
        initialSeriesStarted = false;

        TryStartInitialSeries();
    }

    private void FixedUpdate()
    {
        if (bulletPool == null || target == null)
        {
            return;
        }

        if (!initialSeriesStarted)
        {
            if (ticksUntilNextAction > 0)
            {
                ticksUntilNextAction--;
            }

            TryStartInitialSeries();
            return;
        }

        if (ticksUntilNextAction > 0)
        {
            ticksUntilNextAction--;

            if (ticksUntilNextAction > 0)
            {
                return;
            }
        }

        if (releasingFan)
        {
            ReleaseNextFanChunk();
            return;
        }

        if (burstsRemaining > 0)
        {
            StartNextFanBurst();
            return;
        }

        if (fireOnlyOnce)
        {
            return;
        }

        StartBurstSeries();
    }

    public void Configure(
        Transform newTarget,
        BulletPool newBulletPool
    )
    {
        target = newTarget;
        bulletPool = newBulletPool;

        TryStartInitialSeries();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        TryStartInitialSeries();
    }

    public void SetBulletPool(
        BulletPool newBulletPool
    )
    {
        bulletPool = newBulletPool;

        TryStartInitialSeries();
    }

    private void TryStartInitialSeries()
    {
        if (!isActiveAndEnabled ||
            initialSeriesStarted ||
            ticksUntilNextAction > 0 ||
            bulletPool == null ||
            target == null)
        {
            return;
        }

        StartBurstSeries();
        initialSeriesStarted = true;
    }

    private void StartBurstSeries()
    {
        burstsRemaining = Mathf.Max(1, burstCount);
        StartNextFanBurst();
    }

    private void StartNextFanBurst()
    {
        burstsRemaining--;

        Vector2 directionToTarget =
            target.position - transform.position;

        if (directionToTarget.sqrMagnitude <
            0.0001f)
        {
            ScheduleAfterFan();
            return;
        }

        float centerAngle = Mathf.Atan2(
            directionToTarget.y,
            directionToTarget.x
        ) * Mathf.Rad2Deg;

        activeBulletCount = Mathf.Max(1, bulletCount);

        activeFirstAngle = activeBulletCount == 1
            ? centerAngle
            : centerAngle - spreadAngle * 0.5f;

        activeAngleStep = activeBulletCount == 1
            ? 0f
            : spreadAngle / (activeBulletCount - 1);

        nextBulletIndex = 0;
        releaseChunksRemaining = Mathf.Min(
            activeBulletCount,
            Mathf.Max(1, fanReleaseTicks)
        );
        releasingFan = true;

        ReleaseNextFanChunk();
    }

    private void ReleaseNextFanChunk()
    {
        int bulletsLeft =
            activeBulletCount - nextBulletIndex;

        int bulletsThisChunk = Mathf.CeilToInt(
            bulletsLeft / (float)releaseChunksRemaining
        );

        for (int index = 0;
             index < bulletsThisChunk;
             index++)
        {
            FireBulletAtIndex(nextBulletIndex);
            nextBulletIndex++;
        }

        releaseChunksRemaining--;

        if (nextBulletIndex >= activeBulletCount)
        {
            releasingFan = false;
            ScheduleAfterFan();
            return;
        }

        ticksUntilNextAction = 1;
    }

    private void ScheduleAfterFan()
    {
        ticksUntilNextAction = burstsRemaining > 0
            ? Mathf.Max(1, burstIntervalTicks)
            : Mathf.Max(1, fireIntervalTicks);
    }

    private void FireBulletAtIndex(int bulletIndex)
    {
        float fanAngle =
            activeFirstAngle + activeAngleStep * bulletIndex;

        float angle = fanAngle + RandomOffset(
            Mathf.Max(0f, angleJitterDegrees)
        );

        float radians = angle * Mathf.Deg2Rad;

        Vector2 direction = new(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        );

        for (int layerIndex = 0;
             layerIndex < Mathf.Max(1, speedLayerCount);
             layerIndex++)
        {
            float speed = Mathf.Max(
                0f,
                bulletSpeed +
                speedLayerStep * layerIndex +
                RandomOffset(Mathf.Max(0f, speedJitter))
            );

            bulletPool.Spawn(
                transform.position,
                direction,
                speed,
                transform.root
            );
        }
    }

    private static float RandomOffset(float maximum)
    {
        if (maximum <= 0f)
        {
            return 0f;
        }

        GameplayRandom random = GameplayRandom.Instance;

        return random == null
            ? 0f
            : random.Range(-maximum, maximum);
    }
}