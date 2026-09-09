using System;
using UnityEngine;

[DefaultExecutionOrder(-400)]
[DisallowMultipleComponent]
public sealed class PlayerOptionController :
    MonoBehaviour
{
    [SerializeField]
    private PlayerInputSource inputSource;

    [SerializeField]
    private PlayerPower playerPower;

    [SerializeField]
    private Transform optionRoot;

    private PlayerOptionFormationDefinition formation;

    private Transform[] optionTransforms =
        Array.Empty<Transform>();

    private PlayerOptionFormationDefinition.PowerTier
        activeTier;

    private int observedPower = int.MinValue;
    private bool hasStarted;

    private void Start()
    {
        FindMissingComponents();

        if (inputSource == null)
        {
            Debug.LogError(
                "Player Option Controller has no " +
                "Input Source.",
                this
            );

            enabled = false;
            return;
        }

        if (playerPower == null)
        {
            Debug.LogError(
                "Player Option Controller has no " +
                "Player Power.",
                this
            );

            enabled = false;
            return;
        }

        hasStarted = true;
        RebuildOptions();
    }

    private void FixedUpdate()
    {
        if (formation == null)
        {
            return;
        }

        if (observedPower != playerPower.CurrentPower)
        {
            observedPower = playerPower.CurrentPower;
            RefreshActiveTier();
        }

        MoveActiveOptions();
    }

    public void Configure(
        PlayerOptionFormationDefinition newFormation
    )
    {
        formation = newFormation;
        observedPower = int.MinValue;

        if (hasStarted)
        {
            RebuildOptions();
        }
    }

    public Transform GetOptionTransform(
        int optionIndex
    )
    {
        if (optionIndex < 0 ||
            optionIndex >= optionTransforms.Length)
        {
            return null;
        }

        Transform option =
            optionTransforms[optionIndex];

        if (option == null ||
            !option.gameObject.activeInHierarchy)
        {
            return null;
        }

        return option;
    }

    private void FindMissingComponents()
    {
        if (inputSource == null)
        {
            inputSource =
                GetComponentInParent<PlayerInputSource>();
        }

        if (playerPower == null)
        {
            playerPower =
                GetComponentInParent<PlayerPower>();
        }
    }

    private void RebuildOptions()
    {
        ClearOptions();
        activeTier = null;

        if (formation == null)
        {
            return;
        }

        if (formation.OptionPrefab == null)
        {
            Debug.LogError(
                "Player option formation has no prefab.",
                formation
            );

            return;
        }

        EnsureOptionRoot();

        int maximumCount =
            formation.GetMaximumOptionCount();

        optionTransforms =
            new Transform[maximumCount];

        for (int index = 0;
             index < maximumCount;
             index++)
        {
            GameObject optionObject =
                Instantiate(
                    formation.OptionPrefab,
                    optionRoot
                );

            optionObject.name =
                $"PlayerOption_{index + 1}";

            Transform optionTransform =
                optionObject.transform;

            optionTransform.localPosition =
                Vector3.zero;

            optionTransform.localRotation =
                Quaternion.identity;

            optionObject.SetActive(false);

            optionTransforms[index] =
                optionTransform;
        }

        observedPower = playerPower.CurrentPower;
        RefreshActiveTier();
    }

    private void RefreshActiveTier()
    {
        activeTier =
            formation.GetTier(
                playerPower.CurrentPower
            );

        int activeCount =
            activeTier != null
                ? activeTier.OptionCount
                : 0;

        for (int index = 0;
             index < optionTransforms.Length;
             index++)
        {
            Transform option =
                optionTransforms[index];

            bool shouldBeActive =
                index < activeCount &&
                activeTier.Options[index] != null;

            if (shouldBeActive &&
                !option.gameObject.activeSelf)
            {
                option.localPosition =
                    Vector3.zero;
            }

            option.gameObject.SetActive(
                shouldBeActive
            );
        }
    }

    private void MoveActiveOptions()
    {
        if (activeTier == null)
        {
            return;
        }

        bool focused = inputSource.FocusHeld;

        for (int index = 0;
             index < activeTier.OptionCount;
             index++)
        {
            Transform option =
                optionTransforms[index];

            if (option == null ||
                !option.gameObject.activeSelf)
            {
                continue;
            }

            PlayerOptionFormationDefinition
                .OptionPlacement placement =
                    activeTier.Options[index];

            Vector2 targetPosition =
                placement.GetTargetPosition(
                    focused
                );

            Vector3 target =
                new Vector3(
                    targetPosition.x,
                    targetPosition.y,
                    0f
                );

            option.localPosition =
                Vector3.MoveTowards(
                    option.localPosition,
                    target,
                    formation.MovementSpeed *
                    Time.fixedDeltaTime
                );
        }
    }

    private void EnsureOptionRoot()
    {
        if (optionRoot != null)
        {
            return;
        }

        GameObject rootObject =
            new GameObject("PlayerOptions");

        optionRoot = rootObject.transform;
        optionRoot.SetParent(transform, false);
    }

    private void ClearOptions()
    {
        foreach (Transform option
                 in optionTransforms)
        {
            if (option != null)
            {
                Destroy(option.gameObject);
            }
        }

        optionTransforms =
            Array.Empty<Transform>();
    }
}