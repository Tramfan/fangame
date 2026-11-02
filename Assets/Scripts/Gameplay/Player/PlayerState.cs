using System;
using UnityEngine;

public sealed class PlayerState : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    [Tooltip("Ignore damage while demonstrating or testing patterns.")]
    private bool debugInvulnerable;
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

        GameRunContext.AddScore(
    scorePerGraze
);
    }

    public void TakeHit()

    {
        if (debugInvulnerable)
        {
            return;
        }
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