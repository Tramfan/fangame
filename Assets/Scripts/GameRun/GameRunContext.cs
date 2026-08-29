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

        Difficulty =
            GameDifficulty.Normal;

        CharacterId = string.Empty;
        CampaignId = string.Empty;
        ShotTypeId = string.Empty;

        HasDifficulty = false;
        HasCharacter = false;
        HasShotType = false;
    }

    public static void SelectDifficulty(
        GameDifficulty difficulty
    )
    {
        Difficulty = difficulty;
        HasDifficulty = true;

        CharacterId = string.Empty;
        CampaignId = string.Empty;
        ShotTypeId = string.Empty;

        HasCharacter = false;
        HasShotType = false;
    }

    public static void SelectLoadout(
        string characterId,
        string campaignId,
        string shotTypeId
    )
    {
        CharacterId = characterId;
        CampaignId = campaignId;
        ShotTypeId = shotTypeId;

        HasCharacter = true;
        HasShotType = true;
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