using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyWaveSpawner : MonoBehaviour
{
    [Serializable]
    private sealed class SpawnStep
    {
        [SerializeField]
        private GameObject enemyPrefab;

        [SerializeField]
        private Vector2 spawnPosition =
            new(4.5f, 0f);

        [SerializeField, Min(0)]
        private int delayAfterSpawnTicks = 120;

        public GameObject EnemyPrefab => enemyPrefab;

        public Vector2 SpawnPosition => spawnPosition;

        public int DelayAfterSpawnTicks =>
            Mathf.Max(0, delayAfterSpawnTicks);
    }

    [SerializeField]
    private Transform target;

    [SerializeField]
    private BulletPool bulletPool;

    [SerializeField]
    private SpawnStep[] steps;

    [SerializeField]
    private bool loop;

    [SerializeField, Min(0)]
    private int loopDelayTicks = 180;

    private readonly List<GameObject> activeEnemies =
        new();

    private int currentStepIndex;
    private int ticksUntilNextSpawn;

    private bool sequenceRunning;
    private bool spawningFinished;
    private bool completionRaised;

    public event Action<EnemyWaveSpawner> Completed;

    private void OnEnable()
    {
        StartSequence();
    }

    private void OnDisable()
    {
        sequenceRunning = false;
        spawningFinished = false;
        completionRaised = false;

        currentStepIndex = 0;
        ticksUntilNextSpawn = 0;

        activeEnemies.Clear();
    }

    private void FixedUpdate()
    {
        RemoveDestroyedEnemies();

        if (sequenceRunning)
        {
            UpdateSpawnSequence();
        }

        TryComplete();
    }

    private void StartSequence()
    {
        currentStepIndex = 0;
        ticksUntilNextSpawn = 0;

        sequenceRunning = false;
        spawningFinished = false;
        completionRaised = false;

        activeEnemies.Clear();

        if (target == null)
        {
            Debug.LogError(
                "Spawner target is not assigned.",
                this
            );

            return;
        }

        if (bulletPool == null)
        {
            Debug.LogError(
                "Spawner Bullet Pool is not assigned.",
                this
            );

            return;
        }

        if (steps == null || steps.Length == 0)
        {
            Debug.LogError(
                "Spawner has no spawn steps.",
                this
            );

            return;
        }

        if (!HasValidStep())
        {
            Debug.LogError(
                "Spawner has no valid enemy prefabs.",
                this
            );

            return;
        }

        sequenceRunning = true;
        SpawnNextStep();
    }

    private void UpdateSpawnSequence()
    {
        if (ticksUntilNextSpawn > 0)
        {
            ticksUntilNextSpawn--;

            if (ticksUntilNextSpawn > 0)
            {
                return;
            }
        }

        SpawnNextStep();
    }

    private bool HasValidStep()
    {
        foreach (SpawnStep step in steps)
        {
            if (step != null &&
                step.EnemyPrefab != null)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnNextStep()
    {
        while (sequenceRunning)
        {
            if (currentStepIndex >= steps.Length)
            {
                HandleSequenceEnd();
                return;
            }

            SpawnStep step = steps[currentStepIndex];
            currentStepIndex++;

            if (step == null ||
                step.EnemyPrefab == null)
            {
                continue;
            }

            SpawnEnemy(step);

            int delay = step.DelayAfterSpawnTicks;

            if (currentStepIndex >= steps.Length)
            {
                if (loop)
                {
                    currentStepIndex = 0;

                    ticksUntilNextSpawn =
                        Mathf.Max(
                            1,
                            delay + loopDelayTicks
                        );
                }
                else
                {
                    sequenceRunning = false;
                    spawningFinished = true;
                }

                return;
            }

            if (delay > 0)
            {
                ticksUntilNextSpawn = delay;
                return;
            }
        }
    }

    private void HandleSequenceEnd()
    {
        if (loop)
        {
            currentStepIndex = 0;

            ticksUntilNextSpawn =
                Mathf.Max(1, loopDelayTicks);

            return;
        }

        sequenceRunning = false;
        spawningFinished = true;
    }

    private void SpawnEnemy(SpawnStep step)
    {
        GameObject enemy = Instantiate(
            step.EnemyPrefab,
            step.SpawnPosition,
            Quaternion.identity
        );

        activeEnemies.Add(enemy);

        AimedFanEmitter[] aimedEmitters =
            enemy.GetComponentsInChildren
                <AimedFanEmitter>(true);

        foreach (AimedFanEmitter emitter
                 in aimedEmitters)
        {
            emitter.Configure(target, bulletPool);
        }

        RingEmitter[] ringEmitters =
            enemy.GetComponentsInChildren
                <RingEmitter>(true);

        foreach (RingEmitter emitter in ringEmitters)
        {
            emitter.SetBulletPool(bulletPool);
        }
    }

    private void RemoveDestroyedEnemies()
    {
        for (int index = activeEnemies.Count - 1;
             index >= 0;
             index--)
        {
            if (activeEnemies[index] == null)
            {
                activeEnemies.RemoveAt(index);
            }
        }
    }

    private void TryComplete()
    {
        if (!spawningFinished ||
            completionRaised ||
            activeEnemies.Count > 0)
        {
            return;
        }

        completionRaised = true;

        Debug.Log(
            "Enemy wave completed.",
            this
        );

        Completed?.Invoke(this);
    }
}