using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class RunRecordStorage
{
    [Serializable]
    public sealed class Entry
    {
        public string Id;
        public long UtcTicks;
        public long Score;
        public GameDifficulty Difficulty;
        public string SceneName;
        public string CampaignId;
        public string CharacterId;
        public string ShotTypeId;
        public string GameVersion;
        public string ReplayFileName;
    }

    public static string DirectoryPath =>
        Path.Combine(
            Application.persistentDataPath,
            "Records"
        );

    public static bool TrySave(Entry entry)
    {
        string temporaryPath = string.Empty;

        try
        {
            if (!IsValid(entry))
            {
                Debug.LogError(
                    "Cannot save an incomplete run record."
                );
                return false;
            }

            Directory.CreateDirectory(DirectoryPath);

            string path = Path.Combine(
                DirectoryPath,
                entry.Id + ".record.json"
            );

            temporaryPath = path + ".tmp";

            File.WriteAllText(
                temporaryPath,
                JsonUtility.ToJson(entry, true),
                new UTF8Encoding(false)
            );

            File.Move(temporaryPath, path);

            Debug.Log($"Run record saved: {path}");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to save run record: {exception.Message}"
            );
            return false;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception)
                {
                    // Оставшийся временный файл не читается таблицей.
                }
            }
        }
    }

    public static List<Entry> LoadAll()
    {
        List<Entry> entries = new();

        try
        {
            if (!Directory.Exists(DirectoryPath))
            {
                return entries;
            }

            foreach (string path in Directory.GetFiles(
                         DirectoryPath,
                         "*.record.json"))
            {
                try
                {
                    Entry entry = JsonUtility.FromJson<Entry>(
                        File.ReadAllText(path, Encoding.UTF8)
                    );

                    if (IsValid(entry))
                    {
                        entries.Add(entry);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"Skipped invalid record: {path}"
                        );
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        $"Failed to read record {path}: " +
                        $"{exception.Message}"
                    );
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to list run records: {exception.Message}"
            );
        }

        return entries;
    }

    public static string GetReplayPath(Entry entry)
    {
        string name = entry?.ReplayFileName;

        if (string.IsNullOrWhiteSpace(name) ||
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            name != Path.GetFileName(name))
        {
            return string.Empty;
        }

        return Path.Combine(
            ReplayFileStorage.ReplayDirectoryPath,
            name
        );
    }

    private static bool IsValid(Entry entry)
    {
        return entry != null &&
            Guid.TryParseExact(entry.Id, "N", out _) &&
            entry.UtcTicks > 0 &&
            entry.UtcTicks <= DateTime.MaxValue.Ticks &&
            entry.Score >= 0 &&
            !string.IsNullOrWhiteSpace(entry.SceneName);
    }
}