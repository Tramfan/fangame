using System;
using UnityEngine;

[DefaultExecutionOrder(-2000)]
[DisallowMultipleComponent]
public sealed class GameplayRandom : MonoBehaviour
{
    [SerializeField]
    private bool randomizeSeedOnStart = true;

    [SerializeField]
    private int fixedSeed = 12345;

    private static bool hasPendingSeed;
    private static int pendingSeed;

    private uint state;

    public static GameplayRandom Instance
    {
        get;
        private set;
    }

    public int Seed
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Debug.LogError(
                "Scene contains more than one Gameplay Random.",
                this
            );

            enabled = false;
            return;
        }

        Instance = this;

        int initialSeed;

        if (hasPendingSeed)
        {
            initialSeed = pendingSeed;
            hasPendingSeed = false;
        }
        else if (randomizeSeedOnStart)
        {
            initialSeed = CreateTimeSeed();
        }
        else
        {
            initialSeed = fixedSeed;
        }

        Initialize(initialSeed);

        Debug.Log(
            $"Gameplay random seed: {Seed}",
            this
        );
    }

    public void Initialize(int seed)
    {
        Seed = seed;
        state = unchecked((uint)seed);

        // Xorshift32 не может работать
        // с нулевым внутренним состоянием.
        if (state == 0)
        {
            state = 0x6D2B79F5u;
        }
    }

    public float Range(
        float minimum,
        float maximum
    )
    {
        if (maximum <= minimum)
        {
            return minimum;
        }

        return Mathf.Lerp(
            minimum,
            maximum,
            NextFloat()
        );
    }

    public int Range(
        int minimum,
        int maximumExclusive
    )
    {
        if (maximumExclusive <= minimum)
        {
            return minimum;
        }

        uint range =
            (uint)(maximumExclusive - minimum);

        return minimum +
            (int)(NextUInt() % range);
    }

    public static void SetSeedForNextRun(int seed)
    {
        pendingSeed = seed;
        hasPendingSeed = true;
    }

    private float NextFloat()
    {
        // Используем верхние 24 бита:
        // результат всегда находится в [0, 1).
        return
            (NextUInt() >> 8) *
            (1f / 16777216f);
    }

    private uint NextUInt()
    {
        uint value = state;

        value ^= value << 13;
        value ^= value >> 17;
        value ^= value << 5;

        state = value;
        return value;
    }

    private static int CreateTimeSeed()
    {
        long ticks = DateTime.UtcNow.Ticks;

        return unchecked(
            (int)ticks ^
            (int)(ticks >> 32) ^
            Environment.TickCount
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}