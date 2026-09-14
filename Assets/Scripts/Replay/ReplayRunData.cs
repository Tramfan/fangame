using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ReplayRunData
{
    public const int CurrentFormatVersion = 1;

    [SerializeField]
    private int formatVersion =
        CurrentFormatVersion;

    [SerializeField]
    private string gameVersion =
        string.Empty;

    [SerializeField]
    private int seed;

    [SerializeField]
    private GameDifficulty difficulty;

    [SerializeField]
    private string campaignId =
        string.Empty;

    [SerializeField]
    private string characterId =
        string.Empty;

    [SerializeField]
    private string shotTypeId =
        string.Empty;

    [SerializeField]
    private List<byte> inputFrames =
        new();

    [SerializeField]
    private long finalScore;

    [SerializeField]
    private bool finished;

    [SerializeField]
    private bool cleared;

    public int FormatVersion =>
        formatVersion;

    public string GameVersion =>
        gameVersion;

    public int Seed =>
        seed;

    public GameDifficulty Difficulty =>
        difficulty;

    public string CampaignId =>
        campaignId;

    public string CharacterId =>
        characterId;

    public string ShotTypeId =>
        shotTypeId;

    public IReadOnlyList<byte> InputFrames =>
        inputFrames;

    public int TickCount =>
        inputFrames.Count;

    public long FinalScore =>
        finalScore;

    public bool Finished =>
        finished;

    public bool Cleared =>
        cleared;

    public void BeginFromCurrentRun()
    {
        formatVersion =
            CurrentFormatVersion;

        gameVersion =
            Application.version;

       seed = GameplayRandom.Instance != null
    ? GameplayRandom.Instance.Seed
    : GameRunContext.Seed;

        difficulty =
            GameRunContext.Difficulty;

        campaignId =
            GameRunContext.CampaignId;

        characterId =
            GameRunContext.CharacterId;

        shotTypeId =
            GameRunContext.ShotTypeId;

        inputFrames.Clear();

        finalScore = 0;
        finished = false;
        cleared = false;
    }

    public void RecordFrame(
        PlayerInputButtons buttons
    )
    {
        if (finished)
        {
            return;
        }

        inputFrames.Add(
            (byte)buttons
        );
    }

    public PlayerInputButtons GetButtons(
        int tick
    )
    {
        if (tick < 0 ||
            tick >= inputFrames.Count)
        {
            return PlayerInputButtons.None;
        }

        return
            (PlayerInputButtons)
            inputFrames[tick];
    }

    public void Finish(
        bool runCleared,
        long score
    )
    {
        if (finished)
        {
            return;
        }

        finished = true;
        cleared = runCleared;

        finalScore =
            Math.Max(
                0,
                score
            );
    }
}