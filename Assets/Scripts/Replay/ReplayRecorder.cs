using UnityEngine;

[DisallowMultipleComponent]
public sealed class ReplayRecorder : MonoBehaviour
{
    [SerializeField] private PlayerInputSource inputSource;
    [SerializeField] private BattleFlowController battleFlow;

    public ReplayRunData Recording { get; private set; }
    public bool IsRecording { get; private set; }
    public string SavedReplayPath { get; private set; } = string.Empty;

    private void Awake()
    {
        if (inputSource == null)
        {
            inputSource = GetComponentInParent<PlayerInputSource>();
        }

        if (inputSource == null || battleFlow == null)
        {
            Debug.LogError(
                "Replay Recorder requires Input Source and Battle Flow.",
                this
            );
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (inputSource == null || battleFlow == null)
        {
            return;
        }

        if (ReplayRuntimeContext.IsPlaybackActive ||
            PracticeRunContext.IsPractice)
        {
            enabled = false;
            return;
        }

        SavedReplayPath = string.Empty;
        Recording = new ReplayRunData();
        Recording.BeginFromCurrentRun();
        IsRecording = true;

        inputSource.InputTickCaptured += HandleInputTickCaptured;
        battleFlow.ResultChanged += HandleBattleResultChanged;
    }

    private void OnDisable()
    {
        if (inputSource != null)
        {
            inputSource.InputTickCaptured -= HandleInputTickCaptured;
        }

        if (battleFlow != null)
        {
            battleFlow.ResultChanged -= HandleBattleResultChanged;
        }

        CancelRecording();
    }

    private void HandleInputTickCaptured(
        int tick,
        PlayerInputButtons buttons)
    {
        if (!IsRecording || inputSource.IsReplayInput)
        {
            return;
        }

        if (tick != Recording.TickCount)
        {
            Debug.LogError(
                $"Replay input tick mismatch: expected " +
                $"{Recording.TickCount}, received {tick}.",
                this
            );
            CancelRecording();
            return;
        }

        Recording.RecordFrame(buttons);
    }

    private void HandleBattleResultChanged(BattleResult result)
    {
        if (result != BattleResult.Running)
        {
            FinishRecording(result == BattleResult.Cleared);
        }
    }

    private void CancelRecording()
    {
        if (!IsRecording)
        {
            return;
        }

        IsRecording = false;
        Recording = null;
    }

    public string FinishRecording(bool cleared)
    {
        if (!cleared)
        {
            CancelRecording();
            return string.Empty;
        }

        // Повторный вызов возвращает уже сохранённый путь.
        if (!IsRecording)
        {
            return SavedReplayPath;
        }

        IsRecording = false;

        Recording.Finish(
            true,
            GameRunContext.CurrentScore
        );

        ReplayRuntimeContext.StoreRecording(Recording);

        if (ReplayFileStorage.TrySave(Recording, out string path))
        {
            SavedReplayPath = path;
            Debug.Log($"Replay saved: {path}", this);
        }

        Debug.Log(
            $"Replay recorded: {Recording.TickCount} ticks, " +
            $"score {Recording.FinalScore}, cleared true.",
            this
        );

        return SavedReplayPath;
    }
}