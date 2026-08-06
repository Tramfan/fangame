using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerPower : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int maxPower = 100;

    [SerializeField, Min(0)]
    private int startingPower = 50;

    [Header("Runtime")]
    [SerializeField]
    private int currentPower;

    public int MaxPower => maxPower;
    public int CurrentPower => currentPower;

    public bool HasPower => currentPower > 0;

    public float NormalizedPower =>
        maxPower > 0
            ? (float)currentPower / maxPower
            : 0f;

    public event Action<int, int> PowerChanged;

    private void Awake()
    {
        maxPower = Mathf.Max(1, maxPower);

        currentPower = Mathf.Clamp(
            startingPower,
            0,
            maxPower
        );
    }

    public bool TryAbsorbShieldHit(int powerCost)
    {
        if (currentPower <= 0)
        {
            return false;
        }

        int cost = Mathf.Max(1, powerCost);

        SetCurrentPower(
            Mathf.Max(0, currentPower - cost)
        );

        // Последняя пуля всё равно поглощается,
        // даже если её стоимость больше остатка.
        return true;
    }

    public void AddPower(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetCurrentPower(
            Mathf.Min(
                maxPower,
                currentPower + amount
            )
        );
    }

    private void SetCurrentPower(int newPower)
    {
        int clampedPower = Mathf.Clamp(
            newPower,
            0,
            maxPower
        );

        if (clampedPower == currentPower)
        {
            return;
        }

        currentPower = clampedPower;

        PowerChanged?.Invoke(
            currentPower,
            maxPower
        );
    }

    private void OnValidate()
    {
        maxPower = Mathf.Max(1, maxPower);

        startingPower = Mathf.Clamp(
            startingPower,
            0,
            maxPower
        );

        currentPower = Mathf.Clamp(
            currentPower,
            0,
            maxPower
        );
    }
}