using UnityEngine;

[DisallowMultipleComponent]
public sealed class ReplayRecorder :
    MonoBehaviour
{
    [SerializeField]
    private PlayerInputSource inputSource;

    [SerializeField]
    private BattleFlowController battleFlow;

    public ReplayRunData Recording
    {
        get;
        private set;
    }

    public bool IsRecording
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (inputSource == null)
        {
            inputSource =
                GetComponentInParent<
                    PlayerInputSource
                >();
        }

        if (inputSource == null)
        {
            Debug.LogError(
                "Replay Recorder has no Input Source.",
                this
            );

            enabled = false;
            return;
        }

        if (battleFlow == null)
        {
            Debug.LogError(
                "Replay Recorder has no Battle Flow.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (ReplayRuntimeContext.IsPlaybackActive)
        {
            enabled = false;
            return;
        }

        Recording =
            new ReplayRunData();

        Recording.BeginFromCurrentRun();

        IsRecording = true;

        inputSource.InputTickCaptured +=
            HandleInputTickCaptured;

        battleFlow.ResultChanged +=
            HandleBattleResultChanged;
    }

    private void OnDisable()
    {
        if (inputSource != null)
        {
            inputSource.InputTickCaptured -=
                HandleInputTickCaptured;
        }

        if (battleFlow != null)
        {
            battleFlow.ResultChanged -=
                HandleBattleResultChanged;
        }

        // При уходе со сцены незавершённый бой
        // не становится готовым реплеем.
        CancelRecording();
    }

    private void HandleInputTickCaptured(
        int tick,
        PlayerInputButtons buttons
    )
    {
        if (!IsRecording ||
            inputSource.IsReplayInput)
        {
            return;
        }

        if (tick != Recording.TickCount)
        {
            Debug.LogError(
                $"Replay input tick mismatch: " +
                $"expected {Recording.TickCount}, " +
                $"received {tick}.",
                this
            );

            CancelRecording();
            return;
        }

        Recording.RecordFrame(buttons);
    }

    private void HandleBattleResultChanged(
        BattleResult result
    )
    {
        FinishRecording(
            result == BattleResult.Cleared
        );
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

    private void FinishRecording(
        bool cleared
    )
    {
        if (!IsRecording)
        {
            return;
        }

        IsRecording = false;

        Recording.Finish(
            cleared,
            GameRunContext.CurrentScore
        );

        ReplayRuntimeContext.StoreRecording(
            Recording
        );

        if (ReplayFileStorage.TrySave(
                Recording,
                out string savedPath))
        {
            Debug.Log(
                $"Replay saved: {savedPath}",
                this
            );
        }

        Debug.Log(
            $"Replay recorded: " +
            $"{Recording.TickCount} ticks, " +
            $"score {Recording.FinalScore}, " +
            $"cleared {Recording.Cleared}.",
            this
        );
    }
}
