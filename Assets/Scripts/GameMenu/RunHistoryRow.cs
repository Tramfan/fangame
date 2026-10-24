using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class RunHistoryRow : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text loadoutText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text dateText;

    private readonly List<(
        LocalizedString source,
        LocalizedString.ChangeHandler handler)> listeners = new();

    private Action action;
    private Button button;

    private string characterValue;
    private string shotValue;

    public Button Selectable
    {
        get
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            return button;
        }
    }

    public void Bind(
        RunRecordStorage.Entry entry,
        int number,
        Action onClick,
        CharacterDefinition character,
        ShotTypeDefinition shotType,
        DifficultyDefinition difficulty,
        LocalizedString stageName)
    {
        ClearLocalization();

        action = onClick;

        Selectable.onClick.RemoveListener(HandleClick);
        Selectable.onClick.AddListener(HandleClick);
        Selectable.interactable = action != null;

        TMP_Text[] cells =
        {
            numberText,
            scoreText,
            loadoutText,
            difficultyText,
            stageText,
            dateText
        };

        HashSet<TMP_Text> uniqueCells = new();

        foreach (TMP_Text cell in cells)
        {
            if (cell == null ||
                !cell.transform.IsChildOf(transform) ||
                !uniqueCells.Add(cell))
            {
                Debug.LogError(
                    "History row: every Text field must reference " +
                    "a different text object inside this row, " +
                    "not a column header.",
                    this
                );
                return;
            }

            // Данными ячейки управляет этот скрипт.
            foreach (LocalizeStringEvent localizer in
                     cell.GetComponents<LocalizeStringEvent>())
            {
                localizer.enabled = false;
            }
        }

        numberText.text = number.ToString();

        scoreText.text = entry.Score
            .ToString("N0", CultureInfo.InvariantCulture)
            .Replace(",", " ");

        characterValue = entry.CharacterId;
        shotValue = entry.ShotTypeId;

        UpdateLoadout();

        difficultyText.text = entry.Difficulty.ToString();
        stageText.text = entry.SceneName;

        dateText.text =
            new DateTime(entry.UtcTicks, DateTimeKind.Utc)
                .ToLocalTime()
                .ToString("dd.MM.yyyy HH:mm");

        Listen(character?.DisplayName, value =>
        {
            characterValue = value;
            UpdateLoadout();
        });

        Listen(shotType?.DisplayName, value =>
        {
            shotValue = value;
            UpdateLoadout();
        });

        Listen(difficulty?.DisplayName, value =>
        {
            difficultyText.text = value;
        });

        Listen(stageName, value =>
        {
            stageText.text = value;
        });
    }

    private void UpdateLoadout()
    {
        loadoutText.text =
            characterValue + " / " + shotValue;
    }

    private void Listen(
        LocalizedString source,
        LocalizedString.ChangeHandler handler)
    {
        if (source == null || source.IsEmpty)
        {
            return;
        }

        listeners.Add((source, handler));
        source.StringChanged += handler;
    }

    private void ClearLocalization()
    {
        foreach (var listener in listeners)
        {
            listener.source.StringChanged -= listener.handler;
        }

        listeners.Clear();
    }

    private void HandleClick()
    {
        action?.Invoke();
    }

    private void OnDisable()
    {
        ClearLocalization();
        action = null;
    }

    private void OnDestroy()
    {
        ClearLocalization();

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
}