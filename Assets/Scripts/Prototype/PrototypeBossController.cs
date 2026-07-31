using System;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public sealed class PrototypeBossController : MonoBehaviour
{
    [Serializable]
    private sealed class Phase
    {
        [SerializeField, Min(1)]
        private int maxHealth = 100;

        [SerializeField, Min(1)]
        private int durationTicks = 1800;

        [SerializeField]
        private GameObject attackRoot;

        public int MaxHealth =>
            Mathf.Max(1, maxHealth);

        public int DurationTicks =>
            Mathf.Max(1, durationTicks);

        public GameObject AttackRoot => attackRoot;
    }

    [SerializeField]
    private Transform target;

    [SerializeField]
    private Phase[] phases;

    private EnemyHealth health;
    private int currentPhaseIndex = -1;

    public int TicksRemaining { get; private set; }

    public int SecondsRemaining =>
        Mathf.CeilToInt(TicksRemaining / 60f);

    public bool IsPhaseActive { get; private set; }

    public int CurrentPhaseNumber =>
        currentPhaseIndex + 1;

    public int PhaseCount =>
        phases != null ? phases.Length : 0;

    public event Action<
        PrototypeBossController,
        bool
    > PhaseEnded;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        DisableAllAttacks();
    }

    private void OnEnable()
    {
        health.Died += HandleDeath;
        StartPhase(0);
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

    private void StartPhase(int phaseIndex)
    {
        if (phases == null ||
            phaseIndex < 0 ||
            phaseIndex >= phases.Length ||
            phases[phaseIndex] == null)
        {
            Debug.LogError(
                "Boss has no valid phase to start.",
                this
            );

            enabled = false;
            return;
        }

        currentPhaseIndex = phaseIndex;

        Phase phase = phases[currentPhaseIndex];

        health.ResetHealth(phase.MaxHealth);
        TicksRemaining = phase.DurationTicks;
        IsPhaseActive = true;

        PrepareAimedEmitters(phase.AttackRoot);

        if (phase.AttackRoot != null)
        {
            phase.AttackRoot.SetActive(true);
        }

        Debug.Log(
            $"Boss phase {CurrentPhaseNumber}/" +
            $"{PhaseCount} started.",
            this
        );
    }

    private void EndPhase(bool timedOut)
    {
        if (!IsPhaseActive)
        {
            return;
        }

        IsPhaseActive = false;

        Phase phase = phases[currentPhaseIndex];

        if (phase.AttackRoot != null)
        {
            phase.AttackRoot.SetActive(false);
        }

        PhaseEnded?.Invoke(this, timedOut);

        Debug.Log(
            $"Boss phase {CurrentPhaseNumber} ended: " +
            (timedOut
                ? "time expired."
                : "health depleted."),
            this
        );

        int nextPhaseIndex =
            currentPhaseIndex + 1;

        if (nextPhaseIndex < phases.Length)
        {
            StartPhase(nextPhaseIndex);
            return;
        }

        Destroy(gameObject);
    }

    private void PrepareAimedEmitters(
        GameObject attackRoot
    )
    {
        if (attackRoot == null)
        {
            return;
        }

        PrototypeAimedFanEmitter[] emitters =
            attackRoot.GetComponentsInChildren
                <PrototypeAimedFanEmitter>(true);

        if (emitters.Length > 0 && target == null)
        {
            Debug.LogError(
                "Boss target is not assigned.",
                this
            );

            return;
        }

        foreach (PrototypeAimedFanEmitter emitter
                 in emitters)
        {
            emitter.SetTarget(target);
        }
    }

    private void DisableAllAttacks()
    {
        if (phases == null)
        {
            return;
        }

        foreach (Phase phase in phases)
        {
            if (phase?.AttackRoot != null)
            {
                phase.AttackRoot.SetActive(false);
            }
        }
    }
}