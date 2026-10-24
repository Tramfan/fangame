using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(MenuScreen))]
[DisallowMultipleComponent]
public sealed class RunHistoryScreen : MonoBehaviour
{
    public enum ListKind
    {
        Records,
        Replays
    }

    [Serializable]
    public sealed class StageNameMapping
    {
        public string SceneName;
        public LocalizedString DisplayName = new();
    }

    [SerializeField] private ListKind kind;
    [SerializeField] private MenuNavigator navigator;
    [SerializeField] private RunHistoryRow[] rows;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private TMP_Text messageText;

    [Header("Definitions")]
    [SerializeField]
    private CharacterDefinition[] availableCharacters;

    [SerializeField]
    private DifficultyDefinition[] availableDifficulties;

    [SerializeField]
    private StageNameMapping[] stageNames;

    [Header("Messages")]
    [SerializeField]
    private LocalizedString emptyMessageLocalization =
        new("UI", "history.empty");

    [SerializeField]
    private LocalizedString replayErrorLocalization =
        new("UI", "history.replay_error");

    private MenuScreen menuScreen;
    private List<RunRecordStorage.Entry> entries = new();

    private string emptyValue = string.Empty;
    private string errorValue = string.Empty;

    private int pageIndex;
    private bool sceneLoading;
    private bool showingReplayError;

    private int PageCount =>
        Mathf.Max(
            1,
            (entries.Count + rows.Length - 1) / rows.Length
        );

    private void Awake()
    {
        menuScreen = GetComponent<MenuScreen>();

        if (navigator == null ||
            rows == null ||
            rows.Length == 0 ||
            previousPageButton == null ||
            nextPageButton == null ||
            backButton == null ||
            pageText == null ||
            messageText == null ||
            Array.Exists(rows, row => row == null))
        {
            Debug.LogError(
                "History screen has unassigned fields.",
                this
            );
            enabled = false;
            return;
        }

        foreach (TMP_Text label in new[] { pageText, messageText })
        {
            foreach (LocalizeStringEvent localizer in
                     label.GetComponents<LocalizeStringEvent>())
            {
                localizer.enabled = false;
            }
        }

        previousPageButton.onClick.AddListener(PreviousPage);
        nextPageButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(Back);
    }

    private void OnEnable()
    {
        if (!enabled)
        {
            return;
        }

        sceneLoading = false;
        pageIndex = 0;

        entries = RunRecordStorage.LoadAll();

        if (kind == ListKind.Replays)
        {
            entries.RemoveAll(entry =>
                !File.Exists(
                    RunRecordStorage.GetReplayPath(entry)
                ));
        }

        entries.Sort(CompareEntries);

        emptyMessageLocalization.StringChanged +=
            HandleEmptyMessage;

        replayErrorLocalization.StringChanged +=
            HandleReplayError;

        ShowPage();
    }

    private int CompareEntries(
        RunRecordStorage.Entry left,
        RunRecordStorage.Entry right)
    {
        if (kind == ListKind.Records)
        {
            int scoreOrder =
                right.Score.CompareTo(left.Score);

            if (scoreOrder != 0)
            {
                return scoreOrder;
            }
        }

        int dateOrder =
            right.UtcTicks.CompareTo(left.UtcTicks);

        return dateOrder != 0
            ? dateOrder
            : string.CompareOrdinal(left.Id, right.Id);
    }

    private void PreviousPage()
    {
        if (pageIndex > 0)
        {
            pageIndex--;
            ShowPage();
        }
    }

    private void NextPage()
    {
        if (pageIndex + 1 < PageCount)
        {
            pageIndex++;
            ShowPage();
        }
    }

    private void Back()
    {
        navigator.Back();
    }

    private void ShowPage()
    {
        List<Button> navigation = new();

        int firstIndex = pageIndex * rows.Length;

        for (int index = 0; index < rows.Length; index++)
        {
            int entryIndex = firstIndex + index;
            bool occupied = entryIndex < entries.Count;

            rows[index].gameObject.SetActive(occupied);

            if (!occupied)
            {
                continue;
            }

            RunRecordStorage.Entry entry =
                entries[entryIndex];

            Action action = null;

            if (kind == ListKind.Replays)
            {
                action = () => PlayReplay(entry);
            }

            TryResolveLoadout(
                entry.CampaignId,
                entry.CharacterId,
                entry.ShotTypeId,
                out CharacterDefinition character,
                out ShotTypeDefinition shotType
            );

            rows[index].Bind(
                entry,
                entryIndex + 1,
                action,
                character,
                shotType,
                FindDifficulty(entry.Difficulty),
                FindStageName(entry.SceneName)
            );

            if (rows[index].Selectable.interactable)
            {
                navigation.Add(rows[index].Selectable);
            }
        }

        previousPageButton.interactable =
            pageIndex > 0;

        nextPageButton.interactable =
            pageIndex + 1 < PageCount;

        if (previousPageButton.interactable)
        {
            navigation.Add(previousPageButton);
        }

        if (nextPageButton.interactable)
        {
            navigation.Add(nextPageButton);
        }

        navigation.Add(backButton);

        for (int index = 0; index < navigation.Count; index++)
        {
            Navigation links = new()
            {
                mode = Navigation.Mode.Explicit,

                selectOnUp = navigation[
                    (index - 1 + navigation.Count) %
                    navigation.Count],

                selectOnDown = navigation[
                    (index + 1) %
                    navigation.Count]
            };

            navigation[index].navigation = links;
        }

        pageText.text =
            $"{pageIndex + 1} / {PageCount}";

        showingReplayError = false;
        RefreshMessage();

        GameObject firstSelected =
            navigation[0].gameObject;

        menuScreen.SetFirstSelectedObject(firstSelected);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            EventSystem.current.SetSelectedGameObject(
                firstSelected
            );
        }
    }

    private void PlayReplay(RunRecordStorage.Entry entry)
    {
        if (sceneLoading)
        {
            return;
        }

        string path =
            RunRecordStorage.GetReplayPath(entry);

        if (!ReplayFileStorage.TryLoad(
                path,
                out ReplayRunData replay))
        {
            ShowReplayError();
            return;
        }

        if (!TryResolveLoadout(
                replay.CampaignId,
                replay.CharacterId,
                replay.ShotTypeId,
                out CharacterDefinition character,
                out ShotTypeDefinition shotType) ||
            !Application.CanStreamedLevelBeLoaded(
                entry.SceneName))
        {
            Debug.LogError(
                $"Replay requires a missing loadout or scene: " +
                $"{entry.CharacterId}/{entry.ShotTypeId}, " +
                $"{entry.SceneName}.",
                this
            );

            ShowReplayError();
            return;
        }

        if (!ReplayRuntimeContext.TryBeginPlayback(replay))
        {
            ShowReplayError();
            return;
        }

        PracticeRunContext.End();

        GameRunContext.BeginNewRun();
        GameRunContext.SelectDifficulty(replay.Difficulty);
        GameRunContext.SelectLoadout(character, shotType);

        GameplayRandom.SetSeedForNextRun(replay.Seed);

        sceneLoading = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            entry.SceneName,
            LoadSceneMode.Single
        );
    }

    private bool TryResolveLoadout(
        string campaignId,
        string characterId,
        string shotTypeId,
        out CharacterDefinition character,
        out ShotTypeDefinition shotType)
    {
        character = null;
        shotType = null;

        if (availableCharacters == null)
        {
            return false;
        }

        foreach (CharacterDefinition candidate
                 in availableCharacters)
        {
            if (candidate == null ||
                candidate.Id != characterId ||
                candidate.CampaignId != campaignId)
            {
                continue;
            }

            character = candidate;

            if (candidate.ShotTypes == null)
            {
                continue;
            }

            foreach (ShotTypeDefinition shot
                     in candidate.ShotTypes)
            {
                if (shot != null && shot.Id == shotTypeId)
                {
                    shotType = shot;
                    return true;
                }
            }
        }

        return false;
    }

    private DifficultyDefinition FindDifficulty(
        GameDifficulty difficulty)
    {
        if (availableDifficulties != null)
        {
            foreach (DifficultyDefinition definition
                     in availableDifficulties)
            {
                if (definition != null &&
                    definition.Difficulty == difficulty)
                {
                    return definition;
                }
            }
        }

        return null;
    }

    private LocalizedString FindStageName(string sceneName)
    {
        if (stageNames != null)
        {
            foreach (StageNameMapping mapping in stageNames)
            {
                if (mapping != null &&
                    mapping.SceneName == sceneName)
                {
                    return mapping.DisplayName;
                }
            }
        }

        return null;
    }

    private void HandleEmptyMessage(string value)
    {
        emptyValue = value;
        RefreshMessage();
    }

    private void HandleReplayError(string value)
    {
        errorValue = value;
        RefreshMessage();
    }

    private void ShowReplayError()
    {
        showingReplayError = true;
        RefreshMessage();
    }

    private void RefreshMessage()
    {
        messageText.text = showingReplayError
            ? errorValue
            : entries.Count == 0
                ? emptyValue
                : string.Empty;
    }

    private void OnDisable()
    {
        emptyMessageLocalization.StringChanged -=
            HandleEmptyMessage;

        replayErrorLocalization.StringChanged -=
            HandleReplayError;
    }

    private void OnDestroy()
    {
        if (previousPageButton != null)
        {
            previousPageButton.onClick.RemoveListener(
                PreviousPage
            );
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveListener(
                NextPage
            );
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(Back);
        }
    }
}