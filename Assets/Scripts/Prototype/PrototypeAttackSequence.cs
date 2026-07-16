using System.Collections;
using UnityEngine;

public sealed class PrototypeAttackSequence : MonoBehaviour
{
    [System.Serializable]
    private sealed class AttackStep
    {
        [SerializeField] private GameObject attackObject;
        [SerializeField] private float duration = 6f;

        public GameObject AttackObject => attackObject;
        public float Duration => Mathf.Max(0.1f, duration);
    }

    [SerializeField] private AttackStep[] steps;
    [SerializeField] private bool loop = true;

    private Coroutine sequenceRoutine;

    private void OnEnable()
    {
        sequenceRoutine = StartCoroutine(RunSequence());
    }

    private void OnDisable()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }

        DisableAllAttacks();
    }

    private IEnumerator RunSequence()
    {
        DisableAllAttacks();

        if (steps == null || steps.Length == 0)
        {
            Debug.LogError("Attack sequence has no steps.", this);
            yield break;
        }

        do
        {
            bool attackWasStarted = false;

            foreach (AttackStep step in steps)
            {
                if (step == null || step.AttackObject == null)
                {
                    continue;
                }

                attackWasStarted = true;
                step.AttackObject.SetActive(true);

                yield return new WaitForSeconds(step.Duration);

                step.AttackObject.SetActive(false);
            }

            if (!attackWasStarted)
            {
                Debug.LogError(
                    "Attack sequence has no valid attack objects.",
                    this
                );

                yield break;
            }
        }
        while (loop);
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