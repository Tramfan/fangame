using UnityEngine;

public enum BulletOwner
{
    Enemy,
    Player
}

[DefaultExecutionOrder(-800)]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed partial class Bullet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float speed = 2.5f;

    [SerializeField]
    private Vector2 removalBounds = new(6f, 6f);

    [Header("Reflection")]
    [SerializeField, Min(0.05f)]
    private float reflectionTurnRadius = 0.45f;

    [SerializeField, Min(1f)]
    private float reflectedSpeedMultiplier = 6f;

    [SerializeField, Min(1)]
    private int reflectedDamage = 1;

    [SerializeField]
    private Color reflectedColor = new(0.25f, 1f, 1f, 1f);

    [Header("Shield")]
    [SerializeField, Min(1)]
    private int shieldPowerCost = 1;

    [Header("Diagnostics")]
    [SerializeField]
    private bool logGrazeDiagnostics;

    private Rigidbody2D body;
    private CircleCollider2D bulletCollider;
    private BulletPool pool;

    private Vector2 direction = Vector2.down;
    private Transform source;
    private BulletOwner owner = BulletOwner.Enemy;
    private bool isPatternControlled;

    internal int SpawnGeneration { get; private set; }

    public bool IsPatternControlled =>
        isPatternControlled && owner == BulletOwner.Enemy;

    private void Awake()
    {
        bulletCollider = GetComponent<CircleCollider2D>();
        body = GetComponent<Rigidbody2D>();

        originalHitboxRadius = bulletCollider.radius;
        originalHitboxOffset = bulletCollider.offset;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
            originalMaterial = spriteRenderer.sharedMaterial;
            originalColor = spriteRenderer.color;
            originalVisualScale = spriteRenderer.transform.localScale;
        }
    }

    internal void AssignPool(BulletPool bulletPool)
    {
        pool = bulletPool;
    }

    public void Initialize(
        Vector2 movementDirection,
        float movementSpeed,
        Transform bulletSource = null,
        bool patternControlled = false,
        BulletMotionDefinition motion = null
    )
    {
        // Кожна видача з пулу отримує новий номер.
        unchecked
        {
            SpawnGeneration++;
        }

        // Скидаємо подію попереднього використання кулі.
        timedVolleyEmitter = null;
        ticksUntilTimedVolley = -1;
        returnAfterTimedVolley = false;

        direction = movementDirection.normalized;
        speed = Mathf.Max(0f, movementSpeed);
        source = bulletSource;
        bulletCollider.enabled = true;

        owner = BulletOwner.Enemy;
        isPatternControlled = patternControlled;
        grazedBeforePatternRelease = false;

        hasGrantedGraze = false;
        hasHitPlayer = false;

        useMotionFunction = motion != null;

        motionType = motion != null
            ? motion.MotionType
            : BulletMotionType.Straight;

        motionOrigin = transform.position;
        motionDirection = direction;
        motionInitialSpeed = speed;

        motionAcceleration = motion != null
            ? motion.Acceleration
            : 0f;

        motionAccelerationPower = motion != null
            ? motion.AccelerationPower
            : 1f;

        motionMinimumSpeed = motion != null
            ? motion.MinimumSpeed
            : 0f;

        motionSineAmplitude = motion != null
            ? motion.SineAmplitude
            : 0f;

        motionSinePeriod = motion != null
            ? motion.SinePeriodSeconds
            : 1f;

        motionSpiralAngularSpeed = motion != null
            ? motion.SpiralAngularSpeedDegreesPerSecond
            : 0f;

        motionAgeTicks = 0;

        // На початку синусоїди дотична може відрізнятися
        // від основного напрямку польоту.
        if (useMotionFunction &&
            motionType == BulletMotionType.Sine)
        {
            MotionMath.EvaluateSine(
                motionOrigin,
                motionDirection,
                motionInitialSpeed,
                motionSineAmplitude,
                motionSinePeriod,
                0f,
                out _,
                out Vector2 initialVelocity
            );

            speed = initialVelocity.magnitude;

            if (initialVelocity.sqrMagnitude > 0.00000001f)
            {
                direction = initialVelocity / speed;
            }
        }

        PlayerArea grazeArea = PlayerArea.GrazeArea;

        wasInsideGrazeArea =
            grazeArea != null &&
            grazeArea.OverlapsCircle(
                GetBulletCenter(transform.position),
                GetBulletWorldRadius()
            );

        ignoreGrazeUntilExit = wasInsideGrazeArea;

        isTurning = false;
        turnSign = 0f;
        turnDegreesRemaining = 0f;
        turnCenter = Vector2.zero;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // ApplyAppearance() встановить вигляд
        // та орієнтацію підготовленої кулі.
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (pool == null)
        {
            Debug.LogError("Bullet has no pool assigned.", this);
            gameObject.SetActive(false);
            return;
        }

        pool.Return(this);
    }
}