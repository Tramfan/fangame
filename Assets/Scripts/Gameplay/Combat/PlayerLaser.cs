using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public sealed class PlayerLaser :
    MonoBehaviour,
    IPlayerProjectile,
    IOriginBoundPlayerProjectile
{
    [Header("Beam")]
    [SerializeField, Min(0.01f)]
    private float beamWidth = 0.12f;

    [SerializeField]
    private float topY = 6f;

    [Header("Damage")]
    [SerializeField, Min(1)]
    private int damageIntervalTicks = 6;

    private readonly List<EnemyHealth>
        overlappingEnemies = new();
private SpriteRenderer spriteRenderer;
    private Transform origin;
    private int damage = 1;
    private int damageCooldownTicks;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        origin = null;
        damage = 1;
        damageCooldownTicks = 0;

        overlappingEnemies.Clear();
    }

    public void BindOrigin(
        Transform newOrigin
    )
    {
        origin = newOrigin;
    }

    public void Initialize(
        Vector2 direction,
        float speed,
        int hitDamage
    )
    {
        damage = Mathf.Max(
            1,
            hitDamage
        );

        UpdateBeamTransform();
    }

    private void FixedUpdate()
    {
        if (origin == null ||
            !origin.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        UpdateBeamTransform();
        UpdateDamage();
    }

    private void UpdateBeamTransform()
    {
        if (origin == null)
        {
            return;
        }

        Vector3 originPosition =
            origin.position;

        float beamLength =
            Mathf.Max(
                0.01f,
                topY - originPosition.y
            );

        transform.position =
            new Vector3(
                originPosition.x,
                originPosition.y +
                beamLength * 0.5f,
                originPosition.z
            );

        transform.rotation =
            Quaternion.identity;

        Vector2 spriteSize =
    spriteRenderer.sprite.bounds.size;

transform.localScale =
    new Vector3(
        beamWidth / spriteSize.x,
        beamLength / spriteSize.y,
        1f
    
            );
    }

    private void UpdateDamage()
    {
        if (damageCooldownTicks > 0)
        {
            damageCooldownTicks--;
        }

        if (damageCooldownTicks > 0)
        {
            return;
        }

        DamageOverlappingEnemies();

        damageCooldownTicks =
            Mathf.Max(
                1,
                damageIntervalTicks
            );
    }

    private void DamageOverlappingEnemies()
    {
        for (int index =
                 overlappingEnemies.Count - 1;
             index >= 0;
             index--)
        {
            EnemyHealth enemy =
                overlappingEnemies[index];

            if (enemy == null ||
                !enemy.isActiveAndEnabled ||
                enemy.IsDead)
            {
                overlappingEnemies.RemoveAt(
                    index
                );

                continue;
            }

            if (enemy.IsInvulnerable)
            {
                continue;
            }

            enemy.TakeDamage(
                damage
            );
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null ||
            overlappingEnemies.Contains(enemy))
        {
            return;
        }

        overlappingEnemies.Add(
            enemy
        );
    }

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
        {
            return;
        }

        overlappingEnemies.Remove(
            enemy
        );
    }
}