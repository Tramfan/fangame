using UnityEngine;
using UnityEngine.Localization.Components;

[DisallowMultipleComponent]
public sealed class DifficultySelector :
    MenuCarouselSelector
{
    [SerializeField]
    private LocalizeStringEvent valueLocalization;

    [SerializeField]
    private RunSetupController runSetupController;

    [SerializeField]
    private DifficultyDefinition[] options;

    [SerializeField, Min(0)]
    private int defaultIndex = 1;

    private int currentIndex;

    public DifficultyDefinition CurrentDefinition =>
        options[currentIndex];

    public GameDifficulty CurrentDifficulty =>
        CurrentDefinition.Difficulty;

    private void Awake()
    {
        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }

        currentIndex = Mathf.Clamp(
            defaultIndex,
            0,
            options.Length - 1
        );

        RefreshLabel();
    }

    protected override void ChangeSelection(
        int direction
    )
    {
        currentIndex += direction;

        if (currentIndex < 0)
        {
            currentIndex = options.Length - 1;
        }
        else if (currentIndex >= options.Length)
        {
            currentIndex = 0;
        }

        RefreshLabel();
    }

    protected override void ConfirmSelection()
    {
        runSetupController.ConfirmDifficulty(
            CurrentDifficulty
        );
    }

    private void RefreshLabel()
    {
        valueLocalization.StringReference =
            CurrentDefinition.DisplayName;

        valueLocalization.RefreshString();
    }

    private bool ValidateSetup()
    {
        if (valueLocalization == null)
        {
            Debug.LogError(
                "Difficulty Selector has no " +
                "Value Localization.",
                this
            );

            return false;
        }

        if (runSetupController == null)
        {
            Debug.LogError(
                "Difficulty Selector has no " +
                "Run Setup Controller.",
                this
            );

            return false;
        }

        if (options == null ||
            options.Length == 0)
        {
            Debug.LogError(
                "Difficulty Selector has no options.",
                this
            );

            return false;
        }

        foreach (DifficultyDefinition option
                 in options)
        {
            if (option == null)
            {
                Debug.LogError(
                    "Difficulty Selector contains " +
                    "an empty option.",
                    this
                );

                return false;
            }
        }

        return true;
    }
}