using UnityEngine;

public sealed class PlayerState : MonoBehaviour
{
    [SerializeField]
    private PrototypeBattleState battleState;

    public int GrazeCount { get; private set; }

    public bool IsDefeated =>
        battleState != null &&
        battleState.Result ==
            PrototypeBattleResult.Defeated;

    private void Awake()
    {
        if (battleState == null)
        {
            Debug.LogError(
                "Player has no battle state assigned.",
                this
            );
        }
    }

    public void RegisterGraze()
    {
        GrazeCount++;

        Debug.Log($"Graze: {GrazeCount}", this);
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
}