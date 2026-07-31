using UnityEngine;
using UnityEngine.SceneManagement;

public enum PrototypeBattleResult
{
    Running,
    Defeated,
    Cleared
}

[DisallowMultipleComponent]
public sealed class PrototypeBattleState : MonoBehaviour
{
    [SerializeField]
    private PrototypeBossController boss;

    [SerializeField]
    private GameObject gameOverRoot;

    [SerializeField]
    private GameObject stageClearRoot;

    [SerializeField]
    private KeyCode restartKey = KeyCode.R;

    public PrototypeBattleResult Result
    {
        get;
        private set;
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        Result = PrototypeBattleResult.Running;

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
                "Battle state has no boss assigned.",
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
        if (Result != PrototypeBattleResult.Running &&
            Input.GetKeyDown(restartKey))
        {
            RestartBattle();
        }
    }

    public void Defeat()
    {
        if (Result != PrototypeBattleResult.Running)
        {
            return;
        }

        Result = PrototypeBattleResult.Defeated;

        if (gameOverRoot != null)
        {
            gameOverRoot.SetActive(true);
        }

        Time.timeScale = 0f;

        Debug.Log("Game Over.", this);
    }

    private void HandleBossCompleted(
        PrototypeBossController completedBoss
    )
    {
        if (Result != PrototypeBattleResult.Running)
        {
            return;
        }

        Result = PrototypeBattleResult.Cleared;

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

        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}