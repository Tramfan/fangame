using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class MainMenuReplayButton :
    MonoBehaviour
{
    [SerializeField]
    private CharacterDefinition[] availableCharacters;

    [SerializeField]
    private string gameplaySceneName =
        "TestStage";

    private Button replayButton;
    private bool sceneLoading;

    private void Awake()
    {
        replayButton = GetComponent<Button>();

        replayButton.onClick.AddListener(
            PlayLatestReplay
        );
    }

    private void Start()
    {
        replayButton.interactable =
            HasAvailableReplay();
    }

    private void OnDestroy()
    {
        if (replayButton != null)
        {
            replayButton.onClick.RemoveListener(
                PlayLatestReplay
            );
        }
    }

    public void PlayLatestReplay()
    {
        if (sceneLoading)
        {
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

        if (!ReplayRuntimeContext
                .TryBeginLatestPlayback())
        {
            Debug.LogWarning(
                "No compatible saved replay was found.",
                this
            );

            return;
        }

        ReplayRunData replay =
            ReplayRuntimeContext.ActivePlayback;

        if (!TryResolveLoadout(
                replay,
                out CharacterDefinition character,
                out ShotTypeDefinition shotType))
        {
            ReplayRuntimeContext.StopPlayback();
            return;
        }

        GameRunContext.SelectDifficulty(
            replay.Difficulty
        );

        GameRunContext.SelectLoadout(
            character,
            shotType
        );

        GameplayRandom.SetSeedForNextRun(
            replay.Seed
        );

        Debug.Log(
            $"Starting saved replay: " +
            $"{replay.Difficulty}, " +
            $"{replay.CharacterId}, " +
            $"{replay.ShotTypeId}, " +
            $"seed {replay.Seed}",
            this
        );

        sceneLoading = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single
        );
    }

    private static bool HasAvailableReplay()
    {
        ReplayRunData memoryReplay =
            ReplayRuntimeContext.LatestRecording;

        if (memoryReplay != null &&
            memoryReplay.Finished &&
            memoryReplay.TickCount > 0)
        {
            return true;
        }

        return ReplayFileStorage.TryLoadLatest(
            out ReplayRunData _,
            out string _
        );
    }

    private bool TryResolveLoadout(
        ReplayRunData replay,
        out CharacterDefinition character,
        out ShotTypeDefinition shotType
    )
    {
        character = null;
        shotType = null;

        if (replay == null)
        {
            Debug.LogError(
                "Replay data is missing.",
                this
            );

            return false;
        }

        if (availableCharacters == null ||
            availableCharacters.Length == 0)
        {
            Debug.LogError(
                "Replay button has no available characters.",
                this
            );

            return false;
        }

        foreach (CharacterDefinition candidate
                 in availableCharacters)
        {
            if (candidate == null ||
                !string.Equals(
                    candidate.Id,
                    replay.CharacterId,
                    StringComparison.Ordinal
                ) ||
                !string.Equals(
                    candidate.CampaignId,
                    replay.CampaignId,
                    StringComparison.Ordinal
                ))
            {
                continue;
            }

            character = candidate;
            break;
        }

        if (character == null)
        {
            Debug.LogError(
                $"Replay character was not found: " +
                $"{replay.CharacterId}.",
                this
            );

            return false;
        }

        ShotTypeDefinition[] shotTypes =
            character.ShotTypes;

        if (shotTypes != null)
        {
            foreach (ShotTypeDefinition candidate
                     in shotTypes)
            {
                if (candidate != null &&
                    string.Equals(
                        candidate.Id,
                        replay.ShotTypeId,
                        StringComparison.Ordinal
                    ))
                {
                    shotType = candidate;
                    break;
                }
            }
        }

        if (shotType == null)
        {
            Debug.LogError(
                $"Replay shot type was not found: " +
                $"{replay.ShotTypeId}.",
                this
            );

            return false;
        }

        return true;
    }
}
