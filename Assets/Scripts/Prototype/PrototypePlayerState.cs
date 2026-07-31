using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PrototypePlayerState : MonoBehaviour
{
    [SerializeField]
    private GameObject gameOverRoot;

    [SerializeField]
    private KeyCode restartKey = KeyCode.R;

    public bool IsDefeated { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;

        if (gameOverRoot != null)
        {
            gameOverRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsDefeated &&
            Input.GetKeyDown(restartKey))
        {
            RestartBattle();
        }
    }

    public void TakeHit()
    {
        if (IsDefeated)
        {
            return;
        }

        IsDefeated = true;

        if (gameOverRoot != null)
        {
            gameOverRoot.SetActive(true);
        }

        Time.timeScale = 0f;

        Debug.Log("Game Over.", this);
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