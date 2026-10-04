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
        [Tooltip("Delay after the last repeat of this step.")]
        private int delayAfterSpawnTicks = 120;

        [SerializeField, Min(1)]
        [Tooltip("How many enemies this step spawns.")]
        private int repeatCount = 1;

        [SerializeField, Min(1)]
        [Tooltip("Fixed ticks between repeated spawns.")]
        private int repeatIntervalTicks = 18;

        [Header("Arc Movement Override")]
        [SerializeField]
        private bool overrideArcMovement;

        [SerializeField]
        [Tooltip("0 right, 90 up, 180 left, -90 down.")]
        private float movementAngleDegrees = -90f;

        [SerializeField, Min(0f)]
        private float movementDistance = 10.9f;

        [SerializeField]
        [Tooltip("Signed bend perpendicular to travel; 0 is straight.")]
        private float movementArcOffset;

        [SerializeField, Min(1)]
        private int movementTicks = 180;

        public GameObject EnemyPrefab => enemyPrefab;

        public Vector2 SpawnPosition => spawnPosition;

        public int DelayAfterSpawnTicks =>
            Mathf.Max(0, delayAfterSpawnTicks);

        public int RepeatCount =>
            Mathf.Max(1, repeatCount);

        public int RepeatIntervalTicks =>
            Mathf.Max(1, repeatIntervalTicks);

        public bool OverrideArcMovement =>
            overrideArcMovement;

        public float MovementAngleDegrees =>
            movementAngleDegrees;

        public float MovementDistance =>
            Mathf.Max(0f, movementDistance);

        public float MovementArcOffset =>
            movementArcOffset;

        public int MovementTicks =>
            Mathf.Max(1, movementTicks);
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
    private int currentRepeatIndex;
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
        currentRepeatIndex = 0;
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
        currentRepeatIndex = 0;
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

            if (step == null ||
                step.EnemyPrefab == null)
            {
                currentStepIndex++;
                currentRepeatIndex = 0;
                continue;
            }

            SpawnEnemy(step);

            currentRepeatIndex++;

            if (currentRepeatIndex < step.RepeatCount)
            {
                ticksUntilNextSpawn =
                    step.RepeatIntervalTicks;

                return;
            }

            currentRepeatIndex = 0;
            currentStepIndex++;

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

        if (step.OverrideArcMovement)
        {
            ArcEnemyMover mover =
                enemy.GetComponent<ArcEnemyMover>();

            if (mover == null)
            {
                Debug.LogError(
                    "Arc movement override requires " +
                    "Arc Enemy Mover on the enemy prefab.",
                    enemy
                );
            }
            else
            {
                mover.ConfigureAngle(
                    step.MovementAngleDegrees,
                    step.MovementDistance,
                    step.MovementArcOffset,
                    step.MovementTicks
                );
            }
        }

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
        EnemyLaserEmitter[] laserEmitters =
    enemy.GetComponentsInChildren
        <EnemyLaserEmitter>(true);

foreach (EnemyLaserEmitter emitter in laserEmitters)
{
    emitter.Configure(target);
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
