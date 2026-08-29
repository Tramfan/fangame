using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class DifficultySelector :
    MenuCarouselSelector
{
    [SerializeField]
    private TMP_Text valueLabel;

    [SerializeField]
    private RunSetupController runSetupController;

    [SerializeField]
    private GameDifficulty[] options =
    {
        GameDifficulty.Easy,
        GameDifficulty.Normal,
        GameDifficulty.Hard,
        GameDifficulty.Lunatic
    };

    [SerializeField, Min(0)]
    private int defaultIndex = 1;

    private int currentIndex;

    public GameDifficulty CurrentDifficulty =>
        options[currentIndex];

    private void Awake()
    {
        if (valueLabel == null)
        {
            Debug.LogError(
                "Difficulty Selector has no Value Label.",
                this
            );

            enabled = false;
            return;
        }

        if (runSetupController == null)
        {
            Debug.LogError(
                "Difficulty Selector has no " +
                "Run Setup Controller.",
                this
            );

            enabled = false;
            return;
        }

        if (options == null ||
            options.Length == 0)
        {
            Debug.LogError(
                "Difficulty Selector has no options.",
                this
            );

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
            currentIndex =
                options.Length - 1;
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
        valueLabel.text =
            CurrentDifficulty.ToString();
    }
}