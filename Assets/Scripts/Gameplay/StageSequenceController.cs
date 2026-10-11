using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public sealed class StageSequenceController : MonoBehaviour
{
    [SerializeField]
    private EnemyWaveSpawner openingWave;

    [SerializeField]
    private GameObject bossRoot;

    [SerializeField]
    private BulletPool bulletPool;

    [Header("Boss Entrance")]
    [SerializeField, Min(0)]
    [Tooltip("Pause after the last enemy bullet has disappeared.")]
    private int delayBeforeBossTicks = 60;

    [SerializeField, Min(1)]
    [Tooltip("Maximum time to wait for bullets that do not leave the screen.")]
    private int maxBulletWaitTicks = 600;

    private int ticksUntilBoss;
    private int ticksWaitingForBullets;
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

        if (bulletPool.ActiveCount > 0)
        {
            ticksWaitingForBullets++;

            if (ticksWaitingForBullets <
                Mathf.Max(1, maxBulletWaitTicks))
            {
                return;
            }

            Debug.LogWarning(
                "Stage sequence timed out waiting " +
                "for the last enemy bullets.",
                this
            );

            bulletPool.ClearActiveBullets();
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

        ticksWaitingForBullets = 0;
        ticksUntilBoss =
            Mathf.Max(0, delayBeforeBossTicks);

        waitingForBoss = true;

        Debug.Log(
            "Enemy wave completed; waiting for " +
            "its bullets to leave.",
            this
        );
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