using UnityEngine;

[DisallowMultipleComponent]
public sealed class StageSequenceController : MonoBehaviour
{
    [SerializeField]
    private EnemyWaveSpawner openingWave;

    [SerializeField]
    private GameObject bossRoot;

    [SerializeField]
    private BulletPool bulletPool;

    [SerializeField, Min(0)]
    private int delayBeforeBossTicks = 60;

    private int ticksUntilBoss;
    private bool waitingForBoss;

    private void Awake()
    {
        if (openingWave == null)
        {
            Debug.LogError(
                "Stage sequence has no opening wave.",
                this
            );

            enabled = false;
            return;
        }

        if (bossRoot == null)
        {
            Debug.LogError(
                "Stage sequence has no boss root.",
                this
            );

            enabled = false;
            return;
        }

        if (bulletPool == null)
        {
            Debug.LogError(
                "Stage sequence has no Bullet Pool.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (openingWave != null)
        {
            openingWave.Completed +=
                HandleOpeningWaveCompleted;
        }
    }

    private void Start()
    {
        if (!enabled)
        {
            return;
        }

        bossRoot.SetActive(false);
        openingWave.gameObject.SetActive(true);

        Debug.Log(
            "Opening enemy wave started.",
            this
        );
    }

    private void OnDisable()
    {
        if (openingWave != null)
        {
            openingWave.Completed -=
                HandleOpeningWaveCompleted;
        }
    }

    private void FixedUpdate()
    {
        if (!waitingForBoss)
        {
            return;
        }

        if (ticksUntilBoss > 0)
        {
            ticksUntilBoss--;

            if (ticksUntilBoss > 0)
            {
                return;
            }
        }

        StartBoss();
    }

    private void HandleOpeningWaveCompleted(
        EnemyWaveSpawner completedWave
    )
    {
        openingWave.gameObject.SetActive(false);
        bulletPool.ClearActiveBullets();

        ticksUntilBoss = delayBeforeBossTicks;
        waitingForBoss = true;

        if (ticksUntilBoss == 0)
        {
            StartBoss();
        }
    }

    private void StartBoss()
    {
        waitingForBoss = false;
        bossRoot.SetActive(true);

        Debug.Log(
            "Boss encounter started.",
            this
        );
    }
}