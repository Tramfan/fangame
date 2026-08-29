using System;

public static class GameRunContext
{
    public static int Seed
    {
        get;
        private set;
    }

    public static GameDifficulty Difficulty
    {
        get;
        private set;
    }

    public static string CharacterId
    {
        get;
        private set;
    } = string.Empty;

    public static string CampaignId
    {
        get;
        private set;
    } = string.Empty;

    public static string ShotTypeId
    {
        get;
        private set;
    } = string.Empty;

    public static CharacterDefinition SelectedCharacter
    {
        get;
        private set;
    }

    public static ShotTypeDefinition SelectedShotType
    {
        get;
        private set;
    }

    public static bool HasDifficulty
    {
        get;
        private set;
    }

    public static bool HasCharacter
    {
        get;
        private set;
    }

    public static bool HasShotType
    {
        get;
        private set;
    }

    public static bool IsReady =>
        HasDifficulty &&
        HasCharacter &&
        HasShotType;

    public static void BeginNewRun()
    {
        Seed = CreateTimeSeed();

        Difficulty = GameDifficulty.Normal;

        ClearLoadout();

        HasDifficulty = false;
    }

    public static void SelectDifficulty(
        GameDifficulty difficulty
    )
    {
        Difficulty = difficulty;
        HasDifficulty = true;

        ClearLoadout();
    }

    public static void SelectLoadout(
        CharacterDefinition character,
        ShotTypeDefinition shotType
    )
    {
        SelectedCharacter = character;
        SelectedShotType = shotType;

        CharacterId = character.Id;
        CampaignId = character.CampaignId;
        ShotTypeId = shotType.Id;

        HasCharacter = true;
        HasShotType = true;
    }

    private static void ClearLoadout()
    {
        CharacterId = string.Empty;
        CampaignId = string.Empty;
        ShotTypeId = string.Empty;

        SelectedCharacter = null;
        SelectedShotType = null;

        HasCharacter = false;
        HasShotType = false;
    }

    private static int CreateTimeSeed()
    {
        long ticks = DateTime.UtcNow.Ticks;

        return unchecked(
            (int)ticks ^
            (int)(ticks >> 32) ^
            Environment.TickCount
        );
    }
}