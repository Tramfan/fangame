using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerOptionFormation",
    menuName = "Fangame/Player Option Formation"
)]
public sealed class PlayerOptionFormationDefinition :
    ScriptableObject
{
    [Serializable]
    public sealed class OptionPlacement
    {
        [SerializeField]
        private Vector2 unfocusedPosition;

        [SerializeField]
        private Vector2 focusedPosition;

        public Vector2 UnfocusedPosition =>
            unfocusedPosition;

        public Vector2 FocusedPosition =>
            focusedPosition;

        public Vector2 GetTargetPosition(
            bool focused
        )
        {
            return focused
                ? focusedPosition
                : unfocusedPosition;
        }
    }

    [Serializable]
    public sealed class PowerTier
    {
        [SerializeField, Min(0)]
        private int minimumPower;

        [SerializeField]
        private OptionPlacement[] options =
            Array.Empty<OptionPlacement>();

        public int MinimumPower =>
            minimumPower;

        public OptionPlacement[] Options =>
            options;

        public int OptionCount =>
            options != null
                ? options.Length
                : 0;
    }

    [SerializeField]
    private GameObject optionPrefab;

    [SerializeField, Min(0f)]
    private float movementSpeed = 4f;

    [SerializeField]
    private PowerTier[] powerTiers =
        Array.Empty<PowerTier>();

    public GameObject OptionPrefab =>
        optionPrefab;

    public float MovementSpeed =>
        movementSpeed;

    public PowerTier GetTier(
        int currentPower
    )
    {
        PowerTier selectedTier = null;

        if (powerTiers == null)
        {
            return null;
        }

        foreach (PowerTier tier in powerTiers)
        {
            if (tier == null ||
                currentPower < tier.MinimumPower)
            {
                continue;
            }

            if (selectedTier == null ||
                tier.MinimumPower >
                selectedTier.MinimumPower)
            {
                selectedTier = tier;
            }
        }

        return selectedTier;
    }

    public int GetMaximumOptionCount()
    {
        int maximumCount = 0;

        if (powerTiers == null)
        {
            return maximumCount;
        }

        foreach (PowerTier tier in powerTiers)
        {
            if (tier == null)
            {
                continue;
            }

            maximumCount = Mathf.Max(
                maximumCount,
                tier.OptionCount
            );
        }

        return maximumCount;
    }
}