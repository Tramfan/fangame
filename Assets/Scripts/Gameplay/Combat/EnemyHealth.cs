using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyHealth :
    MonoBehaviour,
    IDamageable
{
    [SerializeField, Min(1)]
    private int maxHealth = 3;

    [SerializeField]
    private bool destroyOnDeath = true;

    public int MaxHealth => maxHealth;

    public int CurrentHealth { get; private set; }

    public bool IsDead => CurrentHealth <= 0;

    public bool IsInvulnerable { get; private set; }

    public event Action<EnemyHealth> Died;

    private void Awake()
    {
        ResetHealth();
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
    }

    public void ResetHealth(int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        CurrentHealth = maxHealth;
    }

    public void SetInvulnerable(bool value)
    {
        IsInvulnerable = value;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 ||
            IsDead ||
            IsInvulnerable)
        {
            return;
        }

        CurrentHealth = Mathf.Max(
            0,
            CurrentHealth - amount
        );

        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Died?.Invoke(this);

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("Test: Take 1 Damage")]
    private void TestTakeDamage()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Damage testing works only in Play Mode.",
                this
            );

            return;
        }

        TakeDamage(1);

        Debug.Log(
            $"{name}: {CurrentHealth}/{MaxHealth} HP",
            this
        );
    }
}