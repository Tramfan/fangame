using System.Collections;
using UnityEngine;

public sealed class PrototypeEnemySpawner : MonoBehaviour
{
    [System.Serializable]
    private sealed class SpawnStep
    {
        [SerializeField]
        private PrototypeHorizontalEnemy enemyPrefab;

        [SerializeField]
        private Vector2 spawnPosition =
            new Vector2(4.5f, 0f);

        [SerializeField, Min(0)]
        private int delayAfterSpawnTicks = 120;

        public PrototypeHorizontalEnemy EnemyPrefab =>
            enemyPrefab;

        public Vector2 SpawnPosition =>
            spawnPosition;

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

    private static readonly WaitForFixedUpdate
        FixedTick = new WaitForFixedUpdate();

    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        spawnRoutine =
            StartCoroutine(RunSequence());
    }

    private void OnDisable()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private IEnumerator RunSequence()
    {
        if (target == null)
        {
            Debug.LogError(
                "Spawner target is not assigned.",
                this
            );

            yield break;
        }

        if (steps == null || steps.Length == 0)
        {
            Debug.LogError(
                "Spawner has no spawn steps.",
                this
            );

            yield break;
        }

        do
        {
            bool spawnedAtLeastOneEnemy = false;

            foreach (SpawnStep step in steps)
            {
                if (step == null ||
                    step.EnemyPrefab == null)
                {
                    continue;
                }

                spawnedAtLeastOneEnemy = true;

                SpawnEnemy(step);

                if (step.DelayAfterSpawnTicks > 0)
                {
                    yield return WaitTicks(
                        step.DelayAfterSpawnTicks
                    );
                }
                else
                {
                    yield return null;
                }
            }

            if (!spawnedAtLeastOneEnemy)
            {
                Debug.LogError(
                    "Spawner has no valid enemy prefabs.",
                    this
                );

                yield break;
            }

            if (loop && loopDelayTicks > 0)
            {
                yield return WaitTicks(
                    loopDelayTicks
                );
            }
        }
        while (loop);

        spawnRoutine = null;
    }

    private static IEnumerator WaitTicks(
        int tickCount
    )
    {
        for (int tick = 0; tick < tickCount; tick++)
        {
            yield return FixedTick;
        }
    }

    private void SpawnEnemy(SpawnStep step)
    {
        PrototypeHorizontalEnemy enemy =
            Instantiate(
                step.EnemyPrefab,
                step.SpawnPosition,
                Quaternion.identity
            );

        AimedFanEmitter[] emitters =
            enemy.GetComponentsInChildren
                <AimedFanEmitter>(true);

        foreach (
            AimedFanEmitter emitter
            in emitters
        )
        {
            emitter.SetTarget(target);
        }
    }
}