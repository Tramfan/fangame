using UnityEngine;

public static class ReplayRuntimeContext
{
    public static ReplayRunData LatestRecording { get; private set; }
    public static ReplayRunData ActivePlayback { get; private set; }

    public static bool IsPlaybackActive =>
        ActivePlayback != null;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntime()
    {
        LatestRecording = null;
        ActivePlayback = null;
    }

    public static void StoreRecording(ReplayRunData recording)
    {
        if (recording == null ||
            !recording.Finished ||
            !recording.Cleared ||
            recording.TickCount == 0)
        {
            return;
        }

        LatestRecording = recording;
    }

    public static bool TryBeginPlayback(ReplayRunData replay)
    {
        if (replay == null ||
            !replay.Finished ||
            !replay.Cleared ||
            replay.TickCount == 0)
        {
            return false;
        }

        LatestRecording = replay;
        ActivePlayback = replay;
        return true;
    }

    public static bool TryBeginLatestPlayback()
    {
        if (LatestRecording == null)
        {
            if (!ReplayFileStorage.TryLoadLatest(
                    out ReplayRunData replay,
                    out string path))
            {
                return false;
            }

            LatestRecording = replay;
            Debug.Log($"Replay loaded: {path}");
        }

        return TryBeginPlayback(LatestRecording);
    }

    public static void StopPlayback()
    {
        ActivePlayback = null;
    }
}