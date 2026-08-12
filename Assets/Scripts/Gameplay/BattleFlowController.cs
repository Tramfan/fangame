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

    [Header("Result Screens")]
    [SerializeField]
    private GameObject gameOverRoot;

    [SerializeField]
    private GameObject stageClearRoot;

    [Header("Scenes")]
    [SerializeField]
    private string mainMenuSceneName =
        "MainMenu";

    [Header("Result Controls")]
    [SerializeField]
    private KeyCode restartKey = KeyCode.R;

    [SerializeField]
    private KeyCode returnToMenuKey =
        KeyCode.Escape;

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
                "Battle Flow has no Boss assigned.",
                this
            );

            enabled = false;
            return;
        }

        boss.Completed +=
            HandleBossCompleted;
    }

    private void OnDisable()
    {
        if (boss != null)
        {
            boss.Completed -=
                HandleBossCompleted;
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
            return;
        }

        if (Input.GetKeyDown(returnToMenuKey))
        {
            ReturnToMainMenu();
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

    public void ReturnToMainMenu()
    {
        if (string.IsNullOrWhiteSpace(
                mainMenuSceneName))
        {
            Debug.LogError(
                "Main Menu Scene Name is empty.",
                this
            );

            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            mainMenuSceneName,
            LoadSceneMode.Single
        );
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}