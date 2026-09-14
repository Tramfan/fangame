using UnityEngine;

[DefaultExecutionOrder(-1100)]
[DisallowMultipleComponent]
public sealed class ReplayPlayer : MonoBehaviour
{
    [SerializeField] private PlayerInputSource inputSource;
    [SerializeField] private BattleFlowController battleFlow;

    private ReplayRunData playback;
    private int playbackTick;
    private bool ownsReplayInput;
    private bool inputEndReported;

    private void Awake()
    {
        if (inputSource == null)
            inputSource = GetComponentInParent<PlayerInputSource>();
    }

    private void OnEnable()
    {
        playback = ReplayRuntimeContext.ActivePlayback;

        if (playback == null)
        {
            enabled = false;
            return;
        }

        if (inputSource == null || battleFlow == null)
        {
            Debug.LogError(
                "ReplayPlayer requires PlayerInputSource and BattleFlowController.",
                this
            );

            enabled = false;
            return;
        }

        playbackTick = 0;
        inputEndReported = false;
        ownsReplayInput = true;

        inputSource.SetReplayInput(PlayerInputButtons.None);
        battleFlow.ResultChanged += HandleBattleResult;
    }

    private void FixedUpdate()
    {
        if (playbackTick >= playback.TickCount)
        {
            inputSource.SetReplayInput(PlayerInputButtons.None);

            if (!inputEndReported)
            {
                inputEndReported = true;

                Debug.LogError(
                    "Replay input ended before the battle produced a result.",
                    this
                );
            }

            return;
        }

        PlayerInputButtons buttons =
            playback.GetButtons(playbackTick);

        inputSource.SetReplayInput(buttons);
        playbackTick++;
    }

    private void HandleBattleResult(BattleResult result)
    {
        bool cleared = result == BattleResult.Cleared;
        bool resultMatches = cleared == playback.Cleared;
        bool scoreMatches =
            GameRunContext.CurrentScore == playback.FinalScore;
        bool tickCountMatches =
            playbackTick == playback.TickCount;

        if (resultMatches && scoreMatches && tickCountMatches)
        {
            Debug.Log(
                $"Replay verified: {playbackTick} ticks, " +
                $"score {GameRunContext.CurrentScore}, " +
                $"cleared {cleared}.",
                this
            );
        }
        else
        {
            Debug.LogError(
                $"Replay desync. " +
                $"Ticks: {playbackTick}/{playback.TickCount}; " +
                $"score: {GameRunContext.CurrentScore}/" +
                $"{playback.FinalScore}; " +
                $"cleared: {cleared}/{playback.Cleared}.",
                this
            );
        }

        inputSource.SetReplayInput(PlayerInputButtons.None);
        ReplayRuntimeContext.StopPlayback();
    }

    private void OnDisable()
    {
        if (battleFlow != null)
            battleFlow.ResultChanged -= HandleBattleResult;

        if (ownsReplayInput && inputSource != null)
            inputSource.StopReplayInput();

        ownsReplayInput = false;
    }
}