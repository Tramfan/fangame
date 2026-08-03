using System;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(PrototypeBossMover))]
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

        [SerializeField]
        private Vector2 position;

        public int MaxHealth =>
            Mathf.Max(1, maxHealth);

        public int DurationTicks =>
            Mathf.Max(1, durationTicks);

        public GameObject AttackRoot =>
            attackRoot;

        public Vector2 Position =>
            position;
    }

    [SerializeField]
    private Transform target;

    [SerializeField]
    private Phase[] phases;

    private EnemyHealth health;
    private PrototypeBossMover mover;

    private int currentPhaseIndex = -1;
    private int pendingPhaseIndex = -1;

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
public event Action<PrototypeBossController> Completed;
    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        mover = GetComponent<PrototypeBossMover>();

        DisableAllAttacks();
    }

    private void OnEnable()
    {
        health.Died += HandleDeath;
        mover.DestinationReached +=
            HandleDestinationReached;

        MoveToPhase(0);
    }

    private void OnDisable()
    {
        health.Died -= HandleDeath;
        mover.DestinationReached -=
            HandleDestinationReached;
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

    private void MoveToPhase(int phaseIndex)
    {
        if (phases == null ||
            phaseIndex < 0 ||
            phaseIndex >= phases.Length ||
            phases[phaseIndex] == null)
        {
            Debug.LogError(
                "Boss has no valid phase destination.",
                this
            );

            enabled = false;
            return;
        }

        pendingPhaseIndex = phaseIndex;
        IsPhaseActive = false;

        health.SetInvulnerable(true);

        mover.MoveTo(phases[phaseIndex].Position);
    }

    private void HandleDestinationReached()
    {
        if (pendingPhaseIndex < 0)
        {
            return;
        }

        int phaseIndex = pendingPhaseIndex;
        pendingPhaseIndex = -1;

        StartPhase(phaseIndex);
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
        health.SetInvulnerable(false);

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
        health.SetInvulnerable(true);

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
            MoveToPhase(nextPhaseIndex);
            return;
        }

        Completed?.Invoke(this);
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

        AimedFanEmitter[] emitters =
            attackRoot.GetComponentsInChildren
                <AimedFanEmitter>(true);

        if (emitters.Length > 0 && target == null)
        {
            Debug.LogError(
                "Boss target is not assigned.",
                this
            );

            return;
        }

        foreach (AimedFanEmitter emitter
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