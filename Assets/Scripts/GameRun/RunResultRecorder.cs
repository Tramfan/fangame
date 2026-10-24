using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class RunResultRecorder : MonoBehaviour
{
    [SerializeField] private BattleFlowController battleFlow;
    [SerializeField] private ReplayRecorder replayRecorder;

    private bool eligible;
    private bool resultHandled;

    private void Awake()
    {
        // Запоминаем режим до завершения боя:
        // ReplayPlayer сбрасывает флаг воспроизведения при результате.
        eligible =
            !PracticeRunContext.IsPractice &&
            !ReplayRuntimeContext.IsPlaybackActive;

        if (replayRecorder == null)
        {
            replayRecorder = GetComponent<ReplayRecorder>();
        }

        if (battleFlow == null)
        {
            Debug.LogError(
                "Run Result Recorder has no Battle Flow.",
                this
            );
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (battleFlow != null)
        {
            battleFlow.ResultChanged += HandleResult;
        }
    }

    private void OnDisable()
    {
        if (battleFlow != null)
        {
            battleFlow.ResultChanged -= HandleResult;
        }
    }

    private void HandleResult(BattleResult result)
    {
        if (result == BattleResult.Running || resultHandled)
        {
            return;
        }

        resultHandled = true;

        if (!eligible || result != BattleResult.Cleared)
        {
            return;
        }

        // Повторный вызов безопасен независимо от порядка подписчиков.
        string replayPath = replayRecorder != null
            ? replayRecorder.FinishRecording(true)
            : string.Empty;

        RunRecordStorage.Entry entry = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            UtcTicks = DateTime.UtcNow.Ticks,
            Score = GameRunContext.CurrentScore,
            Difficulty = GameRunContext.Difficulty,
            SceneName = SceneManager.GetActiveScene().name,
            CampaignId = GameRunContext.CampaignId,
            CharacterId = GameRunContext.CharacterId,
            ShotTypeId = GameRunContext.ShotTypeId,
            GameVersion = Application.version,
            ReplayFileName = string.IsNullOrEmpty(replayPath)
                ? string.Empty
                : Path.GetFileName(replayPath)
        };

        RunRecordStorage.TrySave(entry);
    }
}