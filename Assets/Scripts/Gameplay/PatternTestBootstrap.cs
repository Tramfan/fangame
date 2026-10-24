using UnityEngine;

[DefaultExecutionOrder(-3000)]
[DisallowMultipleComponent]
public sealed class PatternTestBootstrap : MonoBehaviour
{
    [SerializeField]
    private GameObject bossRoot;

    [Header("Test Settings")]
    [SerializeField]
    private GameDifficulty difficulty =
        GameDifficulty.Normal;

    [SerializeField]
    private bool useFixedSeed = true;

    [SerializeField]
    private int fixedSeed = 12345;

    private void Awake()
    {
        Time.timeScale = 1f;

        ReplayRuntimeContext.StopPlayback();
        PracticeRunContext.Begin(PracticeStart.Boss);

        GameRunContext.BeginNewRun();
        GameRunContext.SelectDifficulty(difficulty);

        int seed = useFixedSeed
            ? fixedSeed
            : GameRunContext.Seed;

        // Выполняется раньше Awake у GameplayRandom.
        GameplayRandom.SetSeedForNextRun(seed);

        if (bossRoot == null)
        {
            Debug.LogError(
                "Pattern Test Bootstrap has no Boss Root.",
                this
            );

            enabled = false;
            return;
        }

        bossRoot.SetActive(false);
    }

    private void Start()
    {
        bossRoot.SetActive(true);
    }
}