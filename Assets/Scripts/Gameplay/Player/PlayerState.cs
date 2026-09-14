using System;
using UnityEngine;

public sealed class PlayerState : MonoBehaviour
{
    [SerializeField]
    private BattleFlowController battleState;
[SerializeField, Min(0)]
private int scorePerGraze = 100;
    public event Action<int> GrazeChanged;

    public int GrazeCount { get; private set; }

    public bool IsDefeated =>
        battleState != null &&
        battleState.Result == BattleResult.Defeated;

    private void Awake()
    {
        if (battleState == null)
        {
            Debug.LogError(
                "Player has no battle flow assigned.",
                this
            );
        }
    }

    public void RegisterGraze()
    {
        GrazeCount++;

        GrazeChanged?.Invoke(GrazeCount);

        Debug.Log(
            $"Graze: {GrazeCount}",
            this
        );
        GameRunContext.AddScore(
    scorePerGraze
);
    }

    public void TakeHit()
    {
        if (battleState != null)
        {
            battleState.Defeat();
        }
    }

    public void RestartBattle()
    {
        if (battleState != null)
        {
            battleState.RestartBattle();
        }
    }
    private void OnValidate()
{
    scorePerGraze =
        Mathf.Max(
            0,
            scorePerGraze
        );
}
}