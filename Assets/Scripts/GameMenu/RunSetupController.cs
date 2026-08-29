using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class RunSetupController : MonoBehaviour
{
    [SerializeField]
    private MenuNavigator navigator;

    [SerializeField]
    private MenuScreen difficultyScreen;

    [SerializeField]
    private MenuScreen characterScreen;

    [SerializeField]
    private string gameplaySceneName =
        "TestStage";

    private bool sceneLoading;

    public void BeginNewRun()
    {
        if (!CanOpen(difficultyScreen))
        {
            return;
        }

        GameRunContext.BeginNewRun();
        navigator.Open(difficultyScreen);
    }

    public void ConfirmDifficulty(
        GameDifficulty difficulty
    )
    {
        if (!CanOpen(characterScreen))
        {
            return;
        }

        GameRunContext.SelectDifficulty(
            difficulty
        );

        Debug.Log(
            $"Run difficulty: {difficulty}; " +
            $"seed: {GameRunContext.Seed}",
            this
        );

        navigator.Open(characterScreen);
    }

    public void ConfirmLoadout(
        CharacterDefinition character,
        ShotTypeDefinition shotType
    )
    {
        if (sceneLoading)
        {
            return;
        }

        if (character == null ||
            shotType == null)
        {
            Debug.LogError(
                "Cannot confirm an incomplete loadout.",
                this
            );

            return;
        }

        if (!GameRunContext.HasDifficulty)
        {
            Debug.LogError(
                "Difficulty was not selected.",
                this
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(
                gameplaySceneName))
        {
            Debug.LogError(
                "Gameplay Scene Name is empty.",
                this
            );

            return;
        }

GameRunContext.SelectLoadout(
    character,
    shotType
);

        GameplayRandom.SetSeedForNextRun(
            GameRunContext.Seed
        );

        Debug.Log(
            $"Starting run: " +
            $"{GameRunContext.Difficulty}, " +
            $"{GameRunContext.CharacterId}, " +
            $"{GameRunContext.ShotTypeId}, " +
            $"seed {GameRunContext.Seed}",
            this
        );

        sceneLoading = true;

        SceneManager.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single
        );
    }

    private bool CanOpen(MenuScreen screen)
    {
        if (navigator == null)
        {
            Debug.LogError(
                "Run Setup has no Menu Navigator.",
                this
            );

            return false;
        }

        if (screen == null)
        {
            Debug.LogError(
                "Run Setup target screen is not assigned.",
                this
            );

            return false;
        }

        return true;
    }
}