using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHealth))]
public sealed class PickupDropOnDeath : MonoBehaviour
{
    [Serializable]
    private sealed class DropEntry
    {
        [SerializeField]
        private PickupMovement pickupPrefab;

        [SerializeField, Min(1)]
        private int count = 1;

        public PickupMovement PickupPrefab =>
            pickupPrefab;

        public int Count =>
            Mathf.Max(1, count);
    }

    [SerializeField]
    private DropEntry[] drops;

    [Header("Random Scatter")]
    [SerializeField, Min(0f)]
    private float minimumLaunchSpeed = 1.4f;

    [SerializeField, Min(0f)]
    private float maximumLaunchSpeed = 2.2f;

    [SerializeField, Range(0f, 160f)]
    private float spreadAngle = 100f;

    private EnemyHealth enemyHealth;
    private bool hasDropped;

    private void Awake()
    {
        enemyHealth =
            GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        hasDropped = false;

        enemyHealth.Died +=
            HandleEnemyDied;
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Died -=
                HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(
        EnemyHealth defeatedEnemy
    )
    {
        if (hasDropped)
        {
            return;
        }

        hasDropped = true;

        GameplayRandom random =
            GameplayRandom.Instance;

        if (random == null)
        {
            Debug.LogError(
                "Scene has no Gameplay Random.",
                this
            );

            return;
        }

        if (drops == null ||
            drops.Length == 0)
        {
            Debug.LogError(
                "Pickup Drop On Death has no drops.",
                this
            );

            return;
        }

        foreach (DropEntry drop in drops)
        {
            if (drop == null ||
                drop.PickupPrefab == null)
            {
                continue;
            }

            for (int index = 0;
                 index < drop.Count;
                 index++)
            {
                SpawnPickup(
                    drop.PickupPrefab,
                    random
                );
            }
        }
    }

    private void SpawnPickup(
        PickupMovement pickupPrefab,
        GameplayRandom random
    )
    {
        float halfSpread =
            spreadAngle * 0.5f;

        float angle = random.Range(
            90f - halfSpread,
            90f + halfSpread
        );

        float speed = random.Range(
            minimumLaunchSpeed,
            maximumLaunchSpeed
        );

        float radians =
            angle * Mathf.Deg2Rad;

        Vector2 direction = new(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        );

        PickupMovement pickup =
            Instantiate(
                pickupPrefab,
                transform.position,
                Quaternion.identity
            );

        pickup.Launch(
            direction * speed
        );
    }

    private void OnValidate()
    {
        minimumLaunchSpeed =
            Mathf.Max(
                0f,
                minimumLaunchSpeed
            );

        maximumLaunchSpeed =
            Mathf.Max(
                minimumLaunchSpeed,
                maximumLaunchSpeed
            );

        spreadAngle = Mathf.Clamp(
            spreadAngle,
            0f,
            160f
        );
    }
}