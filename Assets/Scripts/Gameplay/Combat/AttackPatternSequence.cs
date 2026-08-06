using UnityEngine;

public sealed class AttackPatternSequence : MonoBehaviour
{
    [System.Serializable]
    private sealed class AttackStep
    {
        [SerializeField]
        private GameObject attackObject;

        [SerializeField, Min(1)]
        private int durationTicks = 360;

        public GameObject AttackObject => attackObject;

        public int DurationTicks =>
            Mathf.Max(1, durationTicks);
    }

    [SerializeField]
    private AttackStep[] steps;

    [SerializeField]
    private bool loop = true;

    private int currentStepIndex = -1;
    private int ticksRemaining;

    private GameObject activeAttack;
    private bool sequenceRunning;

    private void OnEnable()
    {
        StartSequence();
    }

    private void OnDisable()
    {
        DisableAllAttacks();

        activeAttack = null;
        currentStepIndex = -1;
        ticksRemaining = 0;
        sequenceRunning = false;
    }

    private void FixedUpdate()
    {
        if (!sequenceRunning)
        {
            return;
        }

        ticksRemaining--;

        if (ticksRemaining > 0)
        {
            return;
        }

        if (activeAttack != null)
        {
            activeAttack.SetActive(false);
            activeAttack = null;
        }

        sequenceRunning = TryActivateNextStep();
    }

    private void StartSequence()
    {
        DisableAllAttacks();

        activeAttack = null;
        currentStepIndex = -1;
        ticksRemaining = 0;

        sequenceRunning = TryActivateNextStep();
    }

    private bool TryActivateNextStep()
    {
        if (steps == null || steps.Length == 0)
        {
            Debug.LogError(
                "Attack pattern sequence has no steps.",
                this
            );

            return false;
        }

        for (int attempt = 0;
             attempt < steps.Length;
             attempt++)
        {
            int nextStepIndex = currentStepIndex + 1;

            if (nextStepIndex >= steps.Length)
            {
                if (!loop)
                {
                    return false;
                }

                nextStepIndex = 0;
            }

            currentStepIndex = nextStepIndex;

            AttackStep step = steps[currentStepIndex];

            if (step == null ||
                step.AttackObject == null)
            {
                continue;
            }

            activeAttack = step.AttackObject;
            ticksRemaining = step.DurationTicks;

            activeAttack.SetActive(true);
            return true;
        }

        Debug.LogError(
            "Attack pattern sequence has no valid attack objects.",
            this
        );

        return false;
    }

    private void DisableAllAttacks()
    {
        if (steps == null)
        {
            return;
        }

        foreach (AttackStep step in steps)
        {
            if (step?.AttackObject != null)
            {
                step.AttackObject.SetActive(false);
            }
        }
    }
}