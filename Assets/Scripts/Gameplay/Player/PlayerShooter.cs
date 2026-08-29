using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerShooter : MonoBehaviour
{
    [SerializeField]
    private PlayerInputSource inputSource;

    [SerializeField]
    private PlayerBullet bulletPrefab;

    [SerializeField]
    private Transform firePoint;

    [SerializeField, Min(1)]
    private int fireIntervalTicks = 6;

    [SerializeField]
    private bool shootingEnabled = true;

    private bool shootHeld;
    private int cooldownTicks;

    private void Awake()
    {
        if (inputSource == null)
        {
            inputSource =
                GetComponentInParent<PlayerInputSource>();
        }

        if (inputSource == null)
        {
            Debug.LogError(
                "Player Shooter has no Input Source.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        shootHeld =
            shootingEnabled &&
            inputSource != null &&
            inputSource.ShootHeld;
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
        cooldownTicks =
            Mathf.Max(1, fireIntervalTicks);
    }

    private void OnDisable()
    {
        shootHeld = false;
        cooldownTicks = 0;
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
    public void Configure(
    PlayerBullet newBulletPrefab,
    int newFireIntervalTicks,
    bool enableShooting
)
{
    bulletPrefab = newBulletPrefab;

    fireIntervalTicks = Mathf.Max(
        1,
        newFireIntervalTicks
    );

    SetShootingEnabled(enableShooting);
}
}