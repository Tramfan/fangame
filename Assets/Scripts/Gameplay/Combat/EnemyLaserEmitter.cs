using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
public sealed class EnemyLaserEmitter : MonoBehaviour
{
    [Header("Pattern")]
    [SerializeField, Range(1, 12)]
    private int laserCount = 1;

    [SerializeField]
    [Tooltip("0 right, 90 up, -90 down.")]
    private float angleDegrees = -90f;

    [SerializeField, Range(0f, 360f)]
    private float spreadDegrees = 30f;

    [SerializeField, Min(0.1f)]
    private float laserLength = 10f;

    [Header("Timing (fixed ticks)")]
    [SerializeField, Min(1)]
    private int warningTicks = 30;

    [SerializeField, Min(1)]
    private int activeTicks = 60;

    [Header("Repeating")]
    [SerializeField]
    private bool repeat;

    [SerializeField, Min(0)]
    [Tooltip("Pause after the active beam before the next warning.")]
    private int repeatDelayTicks = 30;

    [Header("Appearance")]
    [SerializeField, Min(0.001f)]
    private float warningWidth = 0.035f;

    [SerializeField, Min(0.001f)]
    private float activeWidth = 0.3f;

    [SerializeField]
    private Color warningColor =
        new(1f, 0.85f, 0.25f, 0.8f);

    [SerializeField]
    private Color activeColor =
        new(1f, 0.2f, 0.2f, 1f);

    [Header("Shield")]
    [SerializeField, Min(1)]
    private int shieldPowerCost = 1;

    [SerializeField, Min(1)]
    private int shieldDrainIntervalTicks = 6;

    private HoverReturnEnemyMover mover;
    private OrbitingBossEmitter orbitEmitter;

    private PlayerArea hitbox;
    private PlayerState playerState;
    private PlayerShield playerShield;

    private Material lineMaterial;
    private LineRenderer[] lines;
    private Vector2[] directions;

    private int elapsedTicks;
    private int shieldCooldown;
    private int ticksUntilRestart;

    private bool finished;
    private bool hitPlayer;

    private void Awake()
    {
        orbitEmitter =
            GetComponentInParent<
                OrbitingBossEmitter
            >();

        mover =
            GetComponentInParent<
                HoverReturnEnemyMover
            >();

        Shader shader =
            Shader.Find("Sprites/Default");

        if ((orbitEmitter == null &&
             mover == null) ||
            shader == null)
        {
            Debug.LogError(
                shader == null
                    ? "Sprites/Default shader is missing."
                    : "Enemy Laser Emitter needs an " +
                      "Orbiting Boss Emitter or " +
                      "Hover Return Enemy Mover.",
                this
            );

            enabled = false;
            return;
        }

        lineMaterial =
            new Material(shader);
    }

    private void OnEnable()
    {
        elapsedTicks = 0;
        shieldCooldown = 0;
        ticksUntilRestart = 0;
        finished = false;
        hitPlayer = false;
    }

    public void Configure(
        Transform target
    )
    {
        hitbox = null;
        playerState = null;
        playerShield = null;

        if (target != null)
        {
            foreach (
                PlayerArea area
                in target.GetComponentsInChildren<
                    PlayerArea
                >(true)
            )
            {
                if (area.AreaType !=
                    PlayerAreaType.Hitbox)
                {
                    continue;
                }

                hitbox = area;
                break;
            }
        }

        if (hitbox == null)
        {
            Debug.LogError(
                "Enemy laser cannot find player Hitbox.",
                this
            );

            return;
        }

        playerState =
            hitbox.GetComponentInParent<
                PlayerState
            >();

        playerShield =
            hitbox.GetComponentInParent<
                PlayerShield
            >();

        if (playerState == null)
        {
            Debug.LogError(
                "Enemy laser cannot find PlayerState.",
                this
            );
        }
    }

    private void FixedUpdate()
    {
        if (hitbox == null ||
            playerState == null)
        {
            return;
        }

        bool canFire =
            orbitEmitter != null
                ? orbitEmitter.IsFiring
                : mover != null &&
                  mover.IsHovering;

        if (!canFire)
        {
            if (lines != null)
            {
                ClearLines();
                finished = true;
            }

            return;
        }

        if (finished)
        {
            if (!repeat)
            {
                return;
            }

            if (ticksUntilRestart > 0)
            {
                ticksUntilRestart--;
                return;
            }

            StartNextCycle();
        }

        if (lines == null)
        {
            CreateLines();
        }

        bool dangerous =
            elapsedTicks >=
            Mathf.Max(1, warningTicks);

        UpdateLines(dangerous);

        if (dangerous)
        {
            CheckPlayerHit();
        }

        elapsedTicks++;

        int cycleLength =
            Mathf.Max(1, warningTicks) +
            Mathf.Max(1, activeTicks);

        if (elapsedTicks >= cycleLength)
        {
            ClearLines();
            finished = true;

            ticksUntilRestart =
                Mathf.Max(
                    0,
                    repeatDelayTicks
                );
        }
    }

    private void StartNextCycle()
    {
        elapsedTicks = 0;
        shieldCooldown = 0;
        hitPlayer = false;
        finished = false;
    }

    private void CreateLines()
    {
        int count =
            Mathf.Clamp(laserCount, 1, 12);

        lines =
            new LineRenderer[count];

        directions =
            new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            float offset =
                count == 1
                    ? 0f
                    : Mathf.Lerp(
                        -spreadDegrees * 0.5f,
                        spreadDegrees * 0.5f,
                        i / (float)(count - 1)
                    );

            float radians =
                (angleDegrees + offset) *
                Mathf.Deg2Rad;

            directions[i] =
                new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                );

            GameObject beam =
                new GameObject(
                    "Laser " + (i + 1)
                );

            beam.transform.SetParent(
                transform,
                false
            );

            LineRenderer line =
                beam.AddComponent<
                    LineRenderer
                >();

            line.sharedMaterial =
                lineMaterial;

            line.useWorldSpace = true;
            line.positionCount = 2;
            line.numCapVertices = 4;
            line.sortingOrder = 20;

            lines[i] = line;
        }
    }

    private void UpdateLines(
        bool dangerous
    )
    {
        Vector3 origin =
            transform.position;

        Color color =
            dangerous
                ? activeColor
                : warningColor;

        float width =
            Mathf.Max(
                0.001f,
                dangerous
                    ? activeWidth
                    : warningWidth
            );

        for (int i = 0;
             i < lines.Length;
             i++)
        {
            LineRenderer line =
                lines[i];

            line.startColor =
                line.endColor =
                    color;

            line.startWidth =
                line.endWidth =
                    width;

            line.SetPosition(
                0,
                origin
            );

            line.SetPosition(
                1,
                origin +
                (Vector3)(
                    directions[i] *
                    Mathf.Max(
                        0.1f,
                        laserLength
                    )
                )
            );
        }
    }

    private void CheckPlayerHit()
    {
        if (hitPlayer ||
            playerState.IsDefeated)
        {
            return;
        }

        Vector2 origin =
            transform.position;

        Vector2 centre =
            hitbox.Center;

        float length =
            Mathf.Max(
                0.1f,
                laserLength
            );

        float radius =
            hitbox.WorldRadius +
            Mathf.Max(
                0.001f,
                activeWidth
            ) * 0.5f;

        bool overlapping = false;

        for (int i = 0;
             i < directions.Length;
             i++)
        {
            Vector2 direction =
                directions[i];

            float projection =
                Mathf.Clamp(
                    Vector2.Dot(
                        centre - origin,
                        direction
                    ),
                    0f,
                    length
                );

            Vector2 closest =
                origin +
                direction *
                projection;

            if ((centre - closest)
                    .sqrMagnitude >
                radius * radius)
            {
                continue;
            }

            overlapping = true;
            break;
        }

        if (!overlapping)
        {
            shieldCooldown = 0;
            return;
        }

        if (playerShield != null &&
            playerShield.IsActive)
        {
            if (shieldCooldown > 0)
            {
                shieldCooldown--;
            }

            if (shieldCooldown == 0)
            {
                playerShield.TryAbsorb(
                    Mathf.Max(
                        1,
                        shieldPowerCost
                    )
                );

                shieldCooldown =
                    Mathf.Max(
                        1,
                        shieldDrainIntervalTicks
                    );
            }

            return;
        }

        hitPlayer = true;
        playerState.TakeHit();
    }

    private void ClearLines()
    {
        if (lines == null)
        {
            return;
        }

        foreach (LineRenderer line
                 in lines)
        {
            if (line != null)
            {
                Destroy(
                    line.gameObject
                );
            }
        }

        lines = null;
        directions = null;
    }

    private void OnDisable()
    {
        ClearLines();
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
        }
    }
}