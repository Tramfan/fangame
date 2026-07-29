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
        private Vector2 spawnPosition = new Vector2(4.5f, 0f);

        [SerializeField, Min(0f)]
        private float delayAfterSpawn = 2f;

        public PrototypeHorizontalEnemy EnemyPrefab =>
            enemyPrefab;

        public Vector2 SpawnPosition =>
            spawnPosition;

        public float DelayAfterSpawn =>
            Mathf.Max(0f, delayAfterSpawn);
    }

    [SerializeField]
    private Transform target;

    [SerializeField]
    private SpawnStep[] steps;

    [SerializeField]
    private bool loop = true;

    [SerializeField, Min(0f)]
    private float loopDelay = 2f;

    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(RunSequence());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
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

                if (step.DelayAfterSpawn > 0f)
                {
                    yield return new WaitForSeconds(
                        step.DelayAfterSpawn
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

            if (loop && loopDelay > 0f)
            {
                yield return new WaitForSeconds(loopDelay);
            }
        }
        while (loop);

        spawnRoutine = null;
    }

    private void SpawnEnemy(SpawnStep step)
    {
        PrototypeHorizontalEnemy enemy = Instantiate(
            step.EnemyPrefab,
            step.SpawnPosition,
            Quaternion.identity
        );

        PrototypeAimedFanEmitter[] emitters =
            enemy.GetComponentsInChildren
                <PrototypeAimedFanEmitter>(true);

        foreach (PrototypeAimedFanEmitter emitter
                 in emitters)
        {
            emitter.SetTarget(target);
        }
    }
}