using UnityEngine;

public sealed class PrototypePlayerState : MonoBehaviour
{
    [SerializeField]
    private PrototypeBattleState battleState;

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