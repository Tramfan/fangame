using UnityEngine;

[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public sealed class PlayerLoadoutApplicator :
    MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField]
    private SpriteRenderer playerRenderer;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private PlayerShooter playerShooter;

    [Header("Direct Scene Play")]
    [SerializeField]
    private CharacterDefinition fallbackCharacter;

    [SerializeField]
    private ShotTypeDefinition fallbackShotType;

    private void Awake()
    {
        FindMissingComponents();

        if (!ValidateComponents())
        {
            enabled = false;
            return;
        }

        CharacterDefinition character =
            GameRunContext.SelectedCharacter != null
                ? GameRunContext.SelectedCharacter
                : fallbackCharacter;

        ShotTypeDefinition shotType =
            GameRunContext.SelectedShotType != null
                ? GameRunContext.SelectedShotType
                : fallbackShotType;

        if (character == null ||
            shotType == null)
        {
            Debug.LogError(
                "Player has neither a selected " +
                "nor a fallback loadout.",
                this
            );

            enabled = false;
            return;
        }

        if (!CharacterContainsShotType(
                character,
                shotType))
        {
            Debug.LogError(
                $"Shot type {shotType.Id} does not " +
                $"belong to character {character.Id}.",
                this
            );

            enabled = false;
            return;
        }

        if (shotType.ShootingEnabled &&
            shotType.BulletPrefab == null)
        {
            Debug.LogError(
                $"Shot type {shotType.Id} has no " +
                "Player Bullet prefab.",
                shotType
            );

            playerShooter.SetShootingEnabled(false);
            enabled = false;
            return;
        }

        ApplyCharacter(character);
        ApplyShotType(shotType);

        Debug.Log(
            $"Applied player loadout: " +
            $"{character.Id}, {shotType.Id}",
            this
        );
    }

    private void FindMissingComponents()
    {
        if (playerRenderer == null)
        {
            playerRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }

        if (playerShooter == null)
        {
            playerShooter =
                GetComponentInChildren<PlayerShooter>(
                    true
                );
        }
    }

    private bool ValidateComponents()
    {
        if (playerRenderer == null)
        {
            Debug.LogError(
                "Player Loadout Applicator has no " +
                "Sprite Renderer.",
                this
            );

            return false;
        }

        if (playerController == null)
        {
            Debug.LogError(
                "Player Loadout Applicator has no " +
                "Player Controller.",
                this
            );

            return false;
        }

        if (playerShooter == null)
        {
            Debug.LogError(
                "Player Loadout Applicator has no " +
                "Player Shooter.",
                this
            );

            return false;
        }

        return true;
    }

    private void ApplyCharacter(
        CharacterDefinition character
    )
    {
        if (character.PlayerSprite != null)
        {
            playerRenderer.sprite =
                character.PlayerSprite;
        }

        playerController.ConfigureMovement(
            character.NormalSpeed,
            character.FocusSpeed
        );
    }

    private void ApplyShotType(
        ShotTypeDefinition shotType
    )
    {
        playerShooter.Configure(
            shotType.BulletPrefab,
            shotType.FireIntervalTicks,
            shotType.ShootingEnabled
        );
    }

    private static bool CharacterContainsShotType(
        CharacterDefinition character,
        ShotTypeDefinition shotType
    )
    {
        ShotTypeDefinition[] availableShotTypes =
            character.ShotTypes;

        if (availableShotTypes == null)
        {
            return false;
        }

        foreach (ShotTypeDefinition available
                 in availableShotTypes)
        {
            if (available == shotType)
            {
                return true;
            }
        }

        return false;
    }
}