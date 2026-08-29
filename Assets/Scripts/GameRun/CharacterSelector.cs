using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CharacterSelector :
    MenuCarouselSelector
{
    [SerializeField]
    private LocalizeStringEvent nameLocalization;

    [SerializeField]
    private LocalizeStringEvent
        shotTypeLocalization;

    [SerializeField]
    private Image portraitImage;

    [SerializeField]
    private RunSetupController runSetupController;

    [SerializeField]
    private CharacterDefinition[] options;

    [SerializeField, Min(0)]
    private int defaultCharacterIndex;

    [SerializeField, Min(0)]
    private int defaultShotTypeIndex;

    private int currentCharacterIndex;
    private int currentShotTypeIndex;

    public CharacterDefinition CurrentCharacter =>
        options[currentCharacterIndex];

    public ShotTypeDefinition CurrentShotType
    {
        get
        {
            ShotTypeDefinition[] shotTypes =
                CurrentCharacter.ShotTypes;

            int index = Mathf.Clamp(
                currentShotTypeIndex,
                0,
                shotTypes.Length - 1
            );

            return shotTypes[index];
        }
    }

    private void Awake()
    {
        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }

        currentCharacterIndex = Mathf.Clamp(
            defaultCharacterIndex,
            0,
            options.Length - 1
        );

        currentShotTypeIndex = Mathf.Max(
            0,
            defaultShotTypeIndex
        );

        RefreshView();
    }

    protected override void ChangeSelection(
        int direction
    )
    {
        currentCharacterIndex += direction;

        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex =
                options.Length - 1;
        }
        else if (currentCharacterIndex >=
                 options.Length)
        {
            currentCharacterIndex = 0;
        }

        RefreshView();
    }

    protected override bool
        ChangeSecondarySelection(
            int direction
        )
    {
        ShotTypeDefinition[] shotTypes =
            CurrentCharacter.ShotTypes;

        if (shotTypes.Length <= 1)
        {
            return false;
        }

        int visibleIndex = Mathf.Clamp(
            currentShotTypeIndex,
            0,
            shotTypes.Length - 1
        );

        visibleIndex += direction;

        if (visibleIndex < 0)
        {
            visibleIndex =
                shotTypes.Length - 1;
        }
        else if (visibleIndex >=
                 shotTypes.Length)
        {
            visibleIndex = 0;
        }

        currentShotTypeIndex = visibleIndex;

        RefreshView();
        return true;
    }

    protected override void ConfirmSelection()
    {
        runSetupController.ConfirmLoadout(
            CurrentCharacter,
            CurrentShotType
        );
    }

    private void RefreshView()
    {
        CharacterDefinition character =
            CurrentCharacter;

        ShotTypeDefinition shotType =
            CurrentShotType;

        nameLocalization.StringReference =
            character.DisplayName;

        nameLocalization.RefreshString();

        shotTypeLocalization.StringReference =
            shotType.DisplayName;

        shotTypeLocalization.RefreshString();

        portraitImage.sprite =
            character.Portrait;

        portraitImage.enabled =
            character.Portrait != null;
    }

    private bool ValidateSetup()
    {
        if (nameLocalization == null)
        {
            Debug.LogError(
                "Character Selector has no " +
                "Name Localization.",
                this
            );

            return false;
        }

        if (shotTypeLocalization == null)
        {
            Debug.LogError(
                "Character Selector has no " +
                "Shot Type Localization.",
                this
            );

            return false;
        }

        if (portraitImage == null)
        {
            Debug.LogError(
                "Character Selector has no " +
                "Portrait Image.",
                this
            );

            return false;
        }

        if (runSetupController == null)
        {
            Debug.LogError(
                "Character Selector has no " +
                "Run Setup Controller.",
                this
            );

            return false;
        }

        if (options == null ||
            options.Length == 0)
        {
            Debug.LogError(
                "Character Selector has no options.",
                this
            );

            return false;
        }

        foreach (CharacterDefinition character
                 in options)
        {
            if (character == null)
            {
                Debug.LogError(
                    "Character Selector contains " +
                    "an empty character.",
                    this
                );

                return false;
            }

            ShotTypeDefinition[] shotTypes =
                character.ShotTypes;

            if (shotTypes == null ||
                shotTypes.Length == 0)
            {
                Debug.LogError(
                    $"Character {character.name} " +
                    "has no shot types.",
                    character
                );

                return false;
            }

            foreach (ShotTypeDefinition shotType
                     in shotTypes)
            {
                if (shotType == null)
                {
                    Debug.LogError(
                        $"Character {character.name} " +
                        "contains an empty shot type.",
                        character
                    );

                    return false;
                }
            }
        }

        return true;
    }
}