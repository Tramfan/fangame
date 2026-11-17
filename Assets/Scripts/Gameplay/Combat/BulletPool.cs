using System;
using UnityEngine;

[DefaultExecutionOrder(-800)]
[DisallowMultipleComponent]
public sealed class BulletPool : MonoBehaviour
{
    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField, Min(64)]
    private int initialSize = 1536;

    private Bullet[] allBullets = Array.Empty<Bullet>();
    private Bullet[] activeBullets = Array.Empty<Bullet>();
    private int[] freeIndices = Array.Empty<int>();
    private int[] pendingFreeIndices = Array.Empty<int>();
    private bool[] isPendingFree = Array.Empty<bool>();

    private int totalCreated;
    private int activeCount;
    private int freeCount;
    private int pendingFreeCount;
    private bool isSimulatingTick;
    private bool hasDeadSlotsInActive;

    public int ActiveCount => activeCount;

    private void Awake()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError(
                "Bullet Pool has no Bullet Prefab assigned.",
                this
            );
            enabled = false;
            return;
        }

        AllocateCapacity(initialSize);

        for (int index = 0; index < initialSize; index++)
        {
            CreateBullet();
        }
    }

    private void FixedUpdate()
    {
        isSimulatingTick = true;

        try
        {
            // Снимок длины: пули, рождённые внутри этого тика,
            // начнут движение со следующего тика.
            int countThisTick = activeCount;

            for (int index = 0; index < countThisTick; index++)
            {
                Bullet bullet = activeBullets[index];

                if (bullet != null && bullet.IsActiveInPool)
                {
                    bullet.StepSimulation();
                }
            }
        }
        finally
        {
            isSimulatingTick = false;
            FlushPendingFreeIndices();
        }

        if (hasDeadSlotsInActive)
        {
            CompactActiveBullets();
        }
    }

    public Bullet Spawn(
        Vector2 position,
        Vector2 direction,
        float speed,
        Transform source = null,
        bool patternControlled = false,
        BulletStyleDefinition style = null,
        Color? tint = null,
        BulletMotionDefinition motion = null
    )
    {
        if (!enabled || bulletPrefab == null)
        {
            return null;
        }

        Bullet bullet;

        if (freeCount > 0)
        {
            freeCount--;
            int poolIndex = freeIndices[freeCount];
            bullet = allBullets[poolIndex];
        }
        else
        {
            bullet = CreateBulletAndTake();
        }

        isPendingFree[bullet.PoolIndex] = false;

        bullet.ApplyStyleGeometry(style);

        bullet.Initialize(
            position,
            direction,
            speed,
            source,
            patternControlled,
            motion
        );

        bullet.ApplyAppearance(style, tint ?? Color.white);

        bullet.IsActiveInPool = true;
        bullet.gameObject.SetActive(true);

        if (activeCount >= activeBullets.Length)
        {
            Array.Resize(ref activeBullets, activeBullets.Length * 2);
        }

        activeBullets[activeCount] = bullet;
        activeCount++;

        return bullet;
    }

    public void ClearActiveBullets()
    {
        for (int index = 0; index < activeCount; index++)
        {
            Bullet bullet = activeBullets[index];

            if (bullet != null && bullet.IsActiveInPool)
            {
                ReturnInternal(bullet, deferFreeIndex: false);
            }

            activeBullets[index] = null;
        }

        for (int index = 0; index < pendingFreeCount; index++)
        {
            int poolIndex = pendingFreeIndices[index];
            isPendingFree[poolIndex] = false;
            PushFreeIndex(poolIndex);
        }

        pendingFreeCount = 0;
        activeCount = 0;
        hasDeadSlotsInActive = false;
    }

    internal void Return(Bullet bullet)
    {
        if (bullet == null || !bullet.IsActiveInPool)
        {
            return;
        }

        ReturnInternal(bullet, deferFreeIndex: isSimulatingTick);
        hasDeadSlotsInActive = true;
    }

    private void ReturnInternal(Bullet bullet, bool deferFreeIndex)
    {
        bullet.IsActiveInPool = false;
        bullet.gameObject.SetActive(false);

        int poolIndex = bullet.PoolIndex;

        if (deferFreeIndex)
        {
            if (isPendingFree[poolIndex])
            {
                return;
            }

            isPendingFree[poolIndex] = true;

            if (pendingFreeCount >= pendingFreeIndices.Length)
            {
                Array.Resize(
                    ref pendingFreeIndices,
                    pendingFreeIndices.Length * 2
                );
            }

            pendingFreeIndices[pendingFreeCount] = poolIndex;
            pendingFreeCount++;
            return;
        }

        if (isPendingFree[poolIndex])
        {
            return;
        }

        PushFreeIndex(poolIndex);
    }

    private void FlushPendingFreeIndices()
    {
        if (pendingFreeCount == 0)
        {
            return;
        }

        for (int index = 0; index < pendingFreeCount; index++)
        {
            int poolIndex = pendingFreeIndices[index];

            if (isPendingFree[poolIndex])
            {
                isPendingFree[poolIndex] = false;
                PushFreeIndex(poolIndex);
            }
        }

        pendingFreeCount = 0;
    }

    private void PushFreeIndex(int poolIndex)
    {
        if (freeCount >= freeIndices.Length)
        {
            Array.Resize(ref freeIndices, freeIndices.Length * 2);
        }

        freeIndices[freeCount] = poolIndex;
        freeCount++;
    }

    private void CompactActiveBullets()
    {
        int writeIndex = 0;

        for (int readIndex = 0; readIndex < activeCount; readIndex++)
        {
            Bullet bullet = activeBullets[readIndex];

            if (bullet != null && bullet.IsActiveInPool)
            {
                activeBullets[writeIndex] = bullet;
                writeIndex++;
            }
        }

        for (int clearIndex = writeIndex; clearIndex < activeCount; clearIndex++)
        {
            activeBullets[clearIndex] = null;
        }

        activeCount = writeIndex;
        hasDeadSlotsInActive = false;
    }

    private void AllocateCapacity(int capacity)
    {
        allBullets = new Bullet[capacity];
        activeBullets = new Bullet[capacity];
        freeIndices = new int[capacity];
        pendingFreeIndices = new int[capacity];
        isPendingFree = new bool[capacity];
    }

    private Bullet CreateBullet()
    {
        int poolIndex = totalCreated;

        if (poolIndex >= allBullets.Length)
        {
            int newCapacity = Mathf.Max(64, allBullets.Length * 2);
            Array.Resize(ref allBullets, newCapacity);
            Array.Resize(ref activeBullets, newCapacity);
            Array.Resize(ref freeIndices, newCapacity);
            Array.Resize(ref pendingFreeIndices, newCapacity);
            Array.Resize(ref isPendingFree, newCapacity);
        }

        Bullet bullet = Instantiate(bulletPrefab, transform);
        bullet.AssignPool(this, poolIndex);
        bullet.IsActiveInPool = false;
        bullet.gameObject.SetActive(false);

        allBullets[poolIndex] = bullet;
        freeIndices[freeCount] = poolIndex;

        totalCreated++;
        freeCount++;

        return bullet;
    }

    private Bullet CreateBulletAndTake()
    {
        Debug.LogWarning(
            $"BulletPool expanded at runtime to {totalCreated + 1}. " +
            "Increase Initial Size in Inspector to avoid mid-battle Instantiate.",
            this
        );

        Bullet bullet = CreateBullet();
        freeCount--;
        return bullet;
    }
}