using System;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public sealed class PrototypeBossController : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int phaseDurationTicks = 1800;

    [SerializeField]
    private GameObject attackRoot;

    private EnemyHealth health;

    public int TicksRemaining { get; private set; }

    public int SecondsRemaining =>
        Mathf.CeilToInt(TicksRemaining / 60f);

    public bool IsPhaseActive { get; private set; }

    public event Action<
        PrototypeBossController,
        bool
    > PhaseEnded;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        health.Died += HandleDeath;

        TicksRemaining =
            Mathf.Max(1, phaseDurationTicks);

        IsPhaseActive = true;
    }

    private void OnDisable()
    {
        health.Died -= HandleDeath;
    }

    private void FixedUpdate()
    {
        if (!IsPhaseActive)
        {
            return;
        }

        TicksRemaining--;

        if (TicksRemaining > 0)
        {
            return;
        }

        TicksRemaining = 0;
        EndPhase(timedOut: true);
    }

    private void HandleDeath(EnemyHealth enemy)
    {
        EndPhase(timedOut: false);
    }

    private void EndPhase(bool timedOut)
    {
        if (!IsPhaseActive)
        {
            return;
        }

        IsPhaseActive = false;

        if (attackRoot != null)
        {
            attackRoot.SetActive(false);
        }

        PhaseEnded?.Invoke(this, timedOut);

        Debug.Log(
            timedOut
                ? "Boss phase ended: time expired."
                : "Boss phase ended: health depleted.",
            this
        );

        if (timedOut)
        {
            Destroy(gameObject);
        }
    }
}