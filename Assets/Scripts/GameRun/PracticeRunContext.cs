using UnityEngine;

public enum PracticeStart
{
    None,
    Stage,
    Boss
}

public static class PracticeRunContext
{
    public static PracticeStart StartPoint { get; private set; } =
        PracticeStart.None;

    public static bool IsPractice =>
        StartPoint != PracticeStart.None;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetRuntime()
    {
        End();
    }

    public static void Begin(PracticeStart startPoint)
    {
        StartPoint = startPoint;
    }

    public static void End()
    {
        StartPoint = PracticeStart.None;
    }
}