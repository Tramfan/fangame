using System.Collections.Generic;
using UnityEngine;

public sealed class RadialWavePattern : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform center;

    [SerializeField]
    private BulletPool bulletPool;

    [Header("Pattern")]
    [SerializeField, Min(1)]
    private int bulletCount = 16;

    [SerializeField, Min(0f)]
    private float baseRadius = 2f;

    [SerializeField, Min(0f)]
    private float amplitude = 1.35f;

    [SerializeField, Min(1)]
    private int waveCount = 3;

    [SerializeField]
    private float waveSpeed = 2f;

    [SerializeField]
    private float angleOffset;

    [Header("Assembly")]
    [SerializeField, Min(1)]
    [Tooltip("How many fixed ticks bullets take to fly from the boss to their places.")]
    private int travelTicks = 45;

    [SerializeField, Min(0)]
    [Tooltip("Extra safe ticks after all bullets reach their places.")]
    private int safeTicksAfterArrival = 30;

    private sealed class WaveBullet
    {
        public Bullet bullet;
        public float angle;
    }

    private readonly List<WaveBullet> bullets = new();

    private int elapsedTravelTicks;
    private int elapsedSafeTicks;
    private float phase;

    private void OnEnable()
    {
        if (bulletPool == null)
        {
            Debug.LogError(
                "Radial Wave Pattern needs a Bullet Pool.",
                this
            );

            enabled = false;
            return;
        }

        elapsedTravelTicks = 0;
        elapsedSafeTicks = 0;
        phase = 0f;

        CreateBullets();
        UpdateTravelPositions(0f);
    }

    private void OnDisable()
    {
        foreach (WaveBullet waveBullet in bullets)
        {
            if (waveBullet.bullet != null &&
                waveBullet.bullet.gameObject.activeSelf &&
                waveBullet.bullet.IsPatternControlled)
            {
                bulletPool.Return(waveBullet.bullet);
            }
        }

        bullets.Clear();
    }

    private void FixedUpdate()
    {
        int duration = Mathf.Max(1, travelTicks);

        if (elapsedTravelTicks < duration)
        {
            elapsedTravelTicks++;

            float progress =
                elapsedTravelTicks / (float)duration;

            UpdateTravelPositions(progress);
            return;
        }

        if (elapsedSafeTicks <
            Mathf.Max(0, safeTicksAfterArrival))
        {
            elapsedSafeTicks++;
            return;
        }

        phase += waveSpeed * Time.fixedDeltaTime;

        UpdateBullets();
        ArmSafeBullets();
    }

    private void CreateBullets()
    {
        bullets.Clear();

        int count = Mathf.Max(1, bulletCount);
        Vector2 origin = GetCenterPosition();

        for (int index = 0; index < count; index++)
        {
            float angle =
                angleOffset +
                360f / count * index;

            Bullet bullet = bulletPool.Spawn(
                origin,
                Vector2.zero,
                0f,
                transform.root,
                true
            );

            if (bullet == null)
            {
                continue;
            }

            bullet.CollisionEnabled = false;

            bullets.Add(new WaveBullet
            {
                bullet = bullet,
                angle = angle
            });
        }
    }

    private void UpdateTravelPositions(float progress)
    {
        Vector2 origin = GetCenterPosition();

        float easedProgress =
            Mathf.SmoothStep(0f, 1f, progress);

        foreach (WaveBullet waveBullet in bullets)
        {
            if (!CanControl(waveBullet))
            {
                continue;
            }

            Vector2 destination =
                GetBulletPosition(
                    waveBullet.angle,
                    origin
                );

            waveBullet.bullet.SetExternalPosition(
                Vector2.Lerp(
                    origin,
                    destination,
                    easedProgress
                )
            );
        }
    }

    private void UpdateBullets()
    {
        Vector2 origin = GetCenterPosition();

        foreach (WaveBullet waveBullet in bullets)
        {
            if (!CanControl(waveBullet))
            {
                continue;
            }

            waveBullet.bullet.SetExternalPosition(
                GetBulletPosition(
                    waveBullet.angle,
                    origin
                )
            );
        }
    }

    private Vector2 GetBulletPosition(
        float angle,
        Vector2 origin
    )
    {
        float radians =
            angle * Mathf.Deg2Rad;

        float radius =
            baseRadius +
            amplitude * Mathf.Sin(
                radians * waveCount +
                phase
            );

        Vector2 direction = new(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        );

        return origin + direction * radius;
    }

    private void ArmSafeBullets()
    {
        PlayerArea grazeArea =
            PlayerArea.GrazeArea;

        foreach (WaveBullet waveBullet in bullets)
        {
            if (!CanControl(waveBullet) ||
                waveBullet.bullet.CollisionEnabled)
            {
                continue;
            }

            if (grazeArea != null &&
                grazeArea.OverlapsCircle(
                    waveBullet.bullet.HitboxWorldCenter,
                    waveBullet.bullet.HitboxWorldRadius
                ))
            {
                continue;
            }

            waveBullet.bullet.CollisionEnabled = true;
        }
    }

    private Vector2 GetCenterPosition()
    {
        return center != null
            ? center.position
            : transform.position;
    }

    private static bool CanControl(
        WaveBullet waveBullet
    )
    {
        return waveBullet.bullet != null &&
            waveBullet.bullet.gameObject.activeSelf &&
            waveBullet.bullet.IsPatternControlled;
    }
}