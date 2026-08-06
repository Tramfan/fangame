using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerShooter : MonoBehaviour
{
    [SerializeField]
    private PlayerBullet bulletPrefab;

    [SerializeField]
    private Transform firePoint;

    [SerializeField, Min(1)]
    private int fireIntervalTicks = 6;

    [SerializeField]
    private KeyCode fireKey = KeyCode.Z;

    [SerializeField]
    private bool shootingEnabled = true;

    private bool shootHeld;
    private int cooldownTicks;

    private void Update()
    {
        shootHeld =
            shootingEnabled &&
            Input.GetKey(fireKey);
    }

    private void FixedUpdate()
    {
        if (!shootHeld)
        {
            cooldownTicks = 0;
            return;
        }

        if (cooldownTicks > 0)
        {
            cooldownTicks--;
        }

        if (cooldownTicks > 0)
        {
            return;
        }

        Fire();
        cooldownTicks = fireIntervalTicks;
    }

    private void Fire()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError(
                "Player bullet prefab is not assigned.",
                this
            );

            shootingEnabled = false;
            return;
        }

        Vector3 spawnPosition =
            firePoint != null
                ? firePoint.position
                : transform.position;

        Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }

    public void SetShootingEnabled(bool enabled)
    {
        shootingEnabled = enabled;

        if (!enabled)
        {
            shootHeld = false;
            cooldownTicks = 0;
        }
    }
}