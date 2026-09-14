using UnityEngine;

public static class ReplayRuntimeContext
{
    public static ReplayRunData LatestRecording
    {
        get;
        private set;
    }

    public static ReplayRunData ActivePlayback
    {
        get;
        private set;
    }

    public static bool IsPlaybackActive =>
        ActivePlayback != null;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetRuntime()
    {
        LatestRecording = null;
        ActivePlayback = null;
    }

    public static void StoreRecording(
        ReplayRunData recording
    )
    {
        if (recording == null ||
            !recording.Finished ||
            recording.TickCount == 0)
        {
            return;
        }

        LatestRecording = recording;
    }

    public static bool TryBeginLatestPlayback()
    {
        if (LatestRecording == null ||
            !LatestRecording.Finished ||
            LatestRecording.TickCount == 0)
        {
            return false;
        }

        ActivePlayback =
            LatestRecording;

        return true;
    }

    public static void StopPlayback()
    {
        ActivePlayback = null;
    }
}