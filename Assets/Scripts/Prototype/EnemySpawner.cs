using UnityEngine;

public sealed class EnemyWaveSpawner : MonoBehaviour
{
    [System.Serializable]
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
    private SpawnStep[] steps;

    [SerializeField]
    private bool loop = true;

    [SerializeField, Min(0)]
    private int loopDelayTicks = 180;

    private int currentStepIndex;
    private int ticksUntilNextSpawn;
    private bool sequenceRunning;

    private void OnEnable()
    {
        StartSequence();
    }

    private void OnDisable()
    {
        sequenceRunning = false;
        currentStepIndex = 0;
        ticksUntilNextSpawn = 0;
    }

    private void FixedUpdate()
    {
        if (!sequenceRunning)
        {
            return;
        }

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

    private void StartSequence()
    {
        currentStepIndex = 0;
        ticksUntilNextSpawn = 0;
        sequenceRunning = false;

        if (target == null)
        {
            Debug.LogError(
                "Spawner target is not assigned.",
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
                if (!loop)
                {
                    sequenceRunning = false;
                    return;
                }

                currentStepIndex = 0;

                ticksUntilNextSpawn =
                    Mathf.Max(1, loopDelayTicks);

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
                if (!loop)
                {
                    sequenceRunning = false;
                    return;
                }

                currentStepIndex = 0;

                ticksUntilNextSpawn =
                    Mathf.Max(
                        1,
                        delay + loopDelayTicks
                    );

                return;
            }

            if (delay > 0)
            {
                ticksUntilNextSpawn = delay;
                return;
            }

            // Нулевая задержка означает, что следующий
            // противник появляется в том же такте.
        }
    }

    private void SpawnEnemy(SpawnStep step)
    {
        GameObject enemy = Instantiate(
            step.EnemyPrefab,
            step.SpawnPosition,
            Quaternion.identity
        );

        AimedFanEmitter[] emitters =
            enemy.GetComponentsInChildren
                <AimedFanEmitter>(true);

        foreach (AimedFanEmitter emitter in emitters)
        {
            emitter.SetTarget(target);
        }
    }
}