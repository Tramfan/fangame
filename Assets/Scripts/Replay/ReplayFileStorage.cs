using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class ReplayFileStorage
{
    private const string ReplayExtension =
        ".replay.json";

    public static string ReplayDirectoryPath
    {
        get
        {
            string gameDirectory =
                Path.GetFullPath(
                    Path.Combine(
                        Application.dataPath,
                        ".."
                    )
                );

            return Path.Combine(
                gameDirectory,
                "Replays"
            );
        }
    }

    public static bool TrySave(
        ReplayRunData replay,
        out string savedPath
    )
    {
        savedPath = string.Empty;

        if (!IsCompleteReplay(replay))
        {
            Debug.LogError(
                "Cannot save an empty or unfinished replay."
            );

            return false;
        }

        string temporaryPath = string.Empty;

        try
        {
            Directory.CreateDirectory(
                ReplayDirectoryPath
            );

            savedPath = GetUniqueReplayPath(replay);
            temporaryPath = savedPath + ".tmp";

            string json = JsonUtility.ToJson(
                replay,
                true
            );

            File.WriteAllText(
                temporaryPath,
                json,
                new UTF8Encoding(false)
            );

            File.Move(
                temporaryPath,
                savedPath
            );

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to save replay: " +
                $"{exception.Message}"
            );

            savedPath = string.Empty;
            return false;
        }
        finally
        {
            if (!string.IsNullOrEmpty(temporaryPath) &&
                File.Exists(temporaryPath))
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception)
                {
                    // A leftover temporary file is harmless.
                }
            }
        }
    }

    public static bool TryLoadLatest(
        out ReplayRunData replay,
        out string loadedPath
    )
    {
        replay = null;
        loadedPath = string.Empty;

        if (!Directory.Exists(
                ReplayDirectoryPath))
        {
            return false;
        }

        string[] replayPaths;

        try
        {
            replayPaths = Directory.GetFiles(
                ReplayDirectoryPath,
                "*" + ReplayExtension,
                SearchOption.TopDirectoryOnly
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to list replay files: " +
                $"{exception.Message}"
            );

            return false;
        }

        Array.Sort(
            replayPaths,
            StringComparer.Ordinal
        );

        for (int index = replayPaths.Length - 1;
             index >= 0;
             index--)
        {
            string replayPath = replayPaths[index];

            if (!TryLoad(
                    replayPath,
                    out ReplayRunData candidate))
            {
                continue;
            }

            replay = candidate;
            loadedPath = replayPath;
            return true;
        }

        return false;
    }

    private static bool TryLoad(
        string replayPath,
        out ReplayRunData replay
    )
    {
        replay = null;

        try
        {
            string json = File.ReadAllText(
                replayPath,
                Encoding.UTF8
            );

            ReplayRunData candidate =
                JsonUtility.FromJson<ReplayRunData>(
                    json
                );

            if (!IsCompleteReplay(candidate) ||
                candidate.FormatVersion !=
                    ReplayRunData.CurrentFormatVersion ||
                !string.Equals(
                    candidate.GameVersion,
                    Application.version,
                    StringComparison.Ordinal
                ))
            {
                Debug.LogWarning(
                    $"Skipped incompatible replay: " +
                    $"{Path.GetFileName(replayPath)}"
                );

                return false;
            }

            replay = candidate;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"Failed to load replay " +
                $"{Path.GetFileName(replayPath)}: " +
                $"{exception.Message}"
            );

            return false;
        }
    }

    private static bool IsCompleteReplay(
        ReplayRunData replay
    )
    {
        return replay != null &&
            replay.Finished &&
            replay.TickCount > 0;
    }

    private static string GetUniqueReplayPath(
        ReplayRunData replay
    )
    {
        string timestamp = DateTime.UtcNow.ToString(
            "yyyyMMdd_HHmmss_fff"
        );

        string baseName =
            $"{timestamp}_" +
            $"{MakeSafeSegment(replay.CampaignId)}_" +
            $"{MakeSafeSegment(replay.CharacterId)}_" +
            $"{MakeSafeSegment(replay.ShotTypeId)}";

        string replayPath = Path.Combine(
            ReplayDirectoryPath,
            baseName + ReplayExtension
        );

        int suffix = 1;

        while (File.Exists(replayPath))
        {
            replayPath = Path.Combine(
                ReplayDirectoryPath,
                $"{baseName}_{suffix}" +
                ReplayExtension
            );

            suffix++;
        }

        return replayPath;
    }

    private static string MakeSafeSegment(
        string value
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        char[] invalidCharacters =
            Path.GetInvalidFileNameChars();

        StringBuilder builder = new();

        foreach (char character in value)
        {
            bool invalid =
                Array.IndexOf(
                    invalidCharacters,
                    character
                ) >= 0;

            builder.Append(
                invalid || char.IsWhiteSpace(character)
                    ? '-'
                    : character
            );
        }

        return builder.ToString();
    }
}
