using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattleResult
{
    Running,
    Defeated,
    Cleared
}

[DisallowMultipleComponent]
public sealed class BattleFlowController : MonoBehaviour
{
    [SerializeField]
    private BossPhaseController boss;

    [SerializeField]
    private GameObject gameOverRoot;

    [SerializeField]
    private GameObject stageClearRoot;

    [SerializeField]
    private KeyCode restartKey = KeyCode.R;

    public BattleResult Result
    {
        get;
        private set;
    }

    public bool IsRunning =>
        Result == BattleResult.Running;

    private void Awake()
    {
        Time.timeScale = 1f;
        Result = BattleResult.Running;

        if (gameOverRoot != null)
        {
            gameOverRoot.SetActive(false);
        }

        if (stageClearRoot != null)
        {
            stageClearRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (boss == null)
        {
            Debug.LogError(
                "Battle flow has no boss assigned.",
                this
            );

            enabled = false;
            return;
        }

        boss.Completed += HandleBossCompleted;
    }

    private void OnDisable()
    {
        if (boss != null)
        {
            boss.Completed -= HandleBossCompleted;
        }
    }

    private void Update()
    {
        if (Result == BattleResult.Running)
        {
            return;
        }

        if (Input.GetKeyDown(restartKey))
        {
            RestartBattle();
        }
    }

    public void Defeat()
    {
        if (Result != BattleResult.Running)
        {
            return;
        }

        Result = BattleResult.Defeated;

        if (gameOverRoot != null)
        {
            gameOverRoot.SetActive(true);
        }

        Time.timeScale = 0f;

        Debug.Log("Game Over.", this);
    }

    private void HandleBossCompleted(
        BossPhaseController completedBoss
    )
    {
        if (Result != BattleResult.Running)
        {
            return;
        }

        Result = BattleResult.Cleared;

        if (stageClearRoot != null)
        {
            stageClearRoot.SetActive(true);
        }

        Time.timeScale = 0f;

        Debug.Log("Stage Clear.", this);
    }

    public void RestartBattle()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}