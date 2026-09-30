using UnityEngine;
using UnityEngine.EventSystems;
[DisallowMultipleComponent]
public sealed class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private BattleFlowController battleFlow;

    [SerializeField]
    private GameObject pauseRoot;
[SerializeField]
private GameObject firstSelected;
    public bool IsPaused
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (battleFlow == null)
        {
            Debug.LogError(
                "Pause Menu has no Battle Flow assigned.",
                this
            );

            enabled = false;
            return;
        }

        if (pauseRoot == null)
        {
            Debug.LogError(
                "Pause Menu has no Pause Root assigned.",
                this
            );

            enabled = false;
            return;
        }

        SetPaused(false);
    }

    private void Update()
    {
 if (!battleFlow.IsRunning)
    {
        return;
    }

    bool wantsToOpen =
        !IsPaused &&
        Input.GetKeyDown(KeyCode.Escape);

    bool wantsToCancel =
        IsPaused &&
        Input.GetButtonDown("Cancel");

    if (!wantsToOpen &&
        !wantsToCancel)
    {
        return;
    }

    SetPaused(!IsPaused);
}

    public void Resume()
    {
        SetPaused(false);
    }

    public void RestartBattle()
    {
        SetPaused(false);
        battleFlow.RestartBattle();
    }

    public void ReturnToMainMenu()
    {
        SetPaused(false);
        battleFlow.ReturnToMainMenu();
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        pauseRoot.SetActive(paused);
        if (EventSystem.current != null)
{
    EventSystem.current.SetSelectedGameObject(null);

    if (paused && firstSelected != null)
    {
        EventSystem.current.SetSelectedGameObject(
            firstSelected
        );
    }
}

        Time.timeScale =
            paused ? 0f : 1f;
    }

    private void OnDisable()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;
        Time.timeScale = 1f;
    }
}