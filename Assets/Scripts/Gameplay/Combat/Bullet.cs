using System;
using UnityEngine;

public enum BulletOwner : byte
{
    Enemy = 0,
    Player = 1
}
[DefaultExecutionOrder(-800)]
[DisallowMultipleComponent]
public sealed partial class Bullet : MonoBehaviour
{
    // <-- Вставлять поля и ReleasePatternControl() нужно сюда, внутрь класса

    private bool useMotionFunction;
    private BulletMotionType motionType;
    private Vector2 motionOrigin;
    private Vector2 motionDirection;
    private float motionInitialSpeed;
    private float motionAcceleration;
    private float motionAccelerationPower = 1f;
    private float motionMinimumSpeed;
    private float motionSineAmplitude;
    private float motionSinePeriod = 1f;
    private float motionSpiralAngularSpeed;
    private int motionAgeTicks;

   

    [Header("Movement")]
    [SerializeField]
    private float speed = 2.5f;
    [SerializeField]
    private Vector2 removalBounds = new(6f, 6f);

    [Header("Default Hitbox")]
    [SerializeField, Min(0.001f)]
    private float defaultHitboxRadius = 0.08f;

    [SerializeField]
    private Vector2 defaultHitboxOffset = Vector2.zero;

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

    private BulletPool pool;

    private Vector2 position;
    private Vector2 previousPosition;
    private Vector2 direction = Vector2.down;
    private Transform source;
    private BulletOwner owner = BulletOwner.Enemy;
    private bool isPatternControlled;

    private float cachedHitboxRadius;
    private Vector2 cachedHitboxOffset;

    internal int PoolIndex { get; set; }
    internal bool IsActiveInPool { get; set; }
    internal int SpawnGeneration { get; private set; }

    public bool IsPatternControlled =>
        isPatternControlled && owner == BulletOwner.Enemy;

    public Vector2 Position => position;
 
 public bool CollisionEnabled { get; set; } = true;
    public float HitboxWorldRadius => cachedHitboxRadius;
    public Vector2 HitboxWorldCenter => position + cachedHitboxOffset;

    public void ReleasePatternControlWithVelocity(Vector2 velocity)
    {
        float newSpeed = velocity.magnitude;
        speed = newSpeed;

        if (newSpeed > 0.0001f)
        {
            direction = velocity / newSpeed;
        }

        ReleasePatternControl();
    }
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
            originalMaterial = spriteRenderer.sharedMaterial;
            originalColor = spriteRenderer.color;
            originalVisualScale = spriteRenderer.transform.localScale;
        }
    }

    internal void AssignPool(BulletPool bulletPool, int index)
    {
        pool = bulletPool;
        PoolIndex = index;
    }

    internal void SetExternalPosition(Vector2 newPosition)
    {
        previousPosition = position;
        position = newPosition;
        transform.position = new Vector3(newPosition.x, newPosition.y, 0f);
    }

    public void Initialize(
        Vector2 startPosition,
        Vector2 movementDirection,
        float movementSpeed,
        Transform bulletSource = null,
        bool patternControlled = false,
        BulletMotionDefinition motion = null
    )
    {
       // Кожен новий спавн починається з увімкненими зіткненнями.
        CollisionEnabled = true;
// Повторний спавн скидає стан попереднього проявлення.
isSpawnAppearancePending = false;
        unchecked
        {
            SpawnGeneration++;
        }

        position = startPosition;
        previousPosition = startPosition;
        transform.position = new Vector3(startPosition.x, startPosition.y, 0f);

        timedVolleyEmitter = null;
        ticksUntilTimedVolley = -1;
        returnAfterTimedVolley = false;

        direction = movementDirection.sqrMagnitude > 0.000001f
            ? movementDirection.normalized
            : Vector2.down;

        speed = Mathf.Max(0f, movementSpeed);
        source = bulletSource;

        owner = BulletOwner.Enemy;
        isPatternControlled = patternControlled;
        grazedBeforePatternRelease = false;

        hasGrantedGraze = false;
        hasHitPlayer = false;

        useMotionFunction = motion != null;
        motionType = motion != null
            ? motion.MotionType
            : BulletMotionType.Straight;

        motionOrigin = startPosition;
        motionDirection = direction;
        motionInitialSpeed = speed;
        motionAcceleration = motion != null ? motion.Acceleration : 0f;
        motionAccelerationPower = motion != null ? motion.AccelerationPower : 1f;
        motionMinimumSpeed = motion != null ? motion.MinimumSpeed : 0f;
        motionSineAmplitude = motion != null ? motion.SineAmplitude : 0f;
        motionSinePeriod = motion != null ? motion.SinePeriodSeconds : 1f;
        motionSpiralAngularSpeed = motion != null
            ? motion.SpiralAngularSpeedDegreesPerSecond
            : 0f;

        motionAgeTicks = 0;

        if (useMotionFunction && motionType == BulletMotionType.Sine)
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
                position + cachedHitboxOffset,
                cachedHitboxRadius
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
    }

    private void ReturnToPool()
    {
        if (!IsActiveInPool)
        {
            return;
        }

        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}