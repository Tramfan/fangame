using System;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public sealed class EnemyBehaviorSequence : MonoBehaviour
{
    public enum StepType
    {
        MoveTo = 0,
        Wait = 1,
        StartAttack = 2,
        StopAttack = 3,

        [InspectorName("Wait For Attack End")]
        WaitForAttack = 4,

        Despawn = 5,
        FlyDirection = 6
    }

    public enum PositionSpace
    {
        World,
        SpawnOffset,
        StepOffset
    }

    [Serializable]
    private sealed class Step
    {
        public StepType type;

        [Min(1)]
        [Tooltip("Duration of movement or waiting in fixed ticks.")]
        public int durationTicks = 60;

        [Header("Move To")]
        public PositionSpace positionSpace;

        public Vector2 position;

        [Tooltip("Smooth acceleration and braking for MoveTo.")]
        public bool smoothMovement = true;

        [Header("Fly Direction")]
        [Tooltip("World angle: 0 right, 90 up, 180 left, -90 down.")]
        public float angleDegrees = -90f;

        [Min(0f)]
        public float initialSpeed = 2f;

        public float acceleration;

        [Min(1f)]
        [Tooltip("Power in speed = initialSpeed + acceleration * time^power.")]
        public float accelerationPower = 1f;

        [Min(0f)]
        [Tooltip("Speed maintained after braking.")]
        public float minimumSpeed;

        [Header("Attack")]
        [Tooltip("Repeater used by attack commands.")]
        public BulletVolleyRepeater attack;
    }

    [SerializeField]
    private bool playOnEnable = true;

    [SerializeField]
    private Step[] steps;

    private EnemyHealth health;

    private Vector3 spawnPosition;
    private Vector3 stepStartPosition;
    private Vector3 stepTargetPosition;
    private Vector2 flightDirection;

    private int stepIndex;
    private int elapsedTicks;
    private bool stepStarted;
    private bool isRunning;

    public bool IsRunning => isRunning;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        spawnPosition = transform.position;

        if (health != null)
        {
            health.Died += HandleDeath;
        }

        if (playOnEnable)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= HandleDeath;
        }

        Stop();
    }

    public void Play()
    {
        Stop();

        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        if (!ValidateSequence())
        {
            return;
        }

        stepIndex = 0;
        elapsedTicks = 0;
        stepStarted = false;
        isRunning = true;
    }

    public void Stop()
    {
        isRunning = false;
        StopAssignedAttacks();
    }

    private void FixedUpdate()
    {
        if (!isRunning)
        {
            return;
        }

        if (health != null && health.IsDead)
        {
            Stop();
            return;
        }

        // За один виклик витрачаємо не більше одного тика руху
        // або очікування. Миттєві команди не додають затримки.
        bool tickConsumed = false;

        while (isRunning && stepIndex < steps.Length)
        {
            Step step = steps[stepIndex];

            if (!stepStarted)
            {
                BeginStep(step);
            }

            switch (step.type)
            {
                case StepType.MoveTo:
                {
                    if (tickConsumed)
                    {
                        return;
                    }

                    tickConsumed = true;
                    elapsedTicks++;

                    int duration = Mathf.Max(1, step.durationTicks);
                    float progress = Mathf.Clamp01(
                        elapsedTicks / (float)duration
                    );

                    if (step.smoothMovement)
                    {
                        progress =
                            progress * progress * (3f - 2f * progress);
                    }

                    transform.position = Vector3.Lerp(
                        stepStartPosition,
                        stepTargetPosition,
                        progress
                    );

                    if (elapsedTicks < duration)
                    {
                        return;
                    }

                    break;
                }

                case StepType.FlyDirection:
                {
                    if (tickConsumed)
                    {
                        return;
                    }

                    tickConsumed = true;
                    elapsedTicks++;

                    float ageSeconds =
                        elapsedTicks * Time.fixedDeltaTime;

                    MotionMath.EvaluateLinear(
                        (Vector2)stepStartPosition,
                        flightDirection,
                        Mathf.Max(0f, step.initialSpeed),
                        step.acceleration,
                        ageSeconds,
                        out Vector2 position,
                        out _,
                        accelerationPower:
                            Mathf.Max(1f, step.accelerationPower),
                        minimumSpeed:
                            Mathf.Max(0f, step.minimumSpeed)
                    );

                    transform.position = new Vector3(
                        position.x,
                        position.y,
                        stepStartPosition.z
                    );

                    if (elapsedTicks <
                        Mathf.Max(1, step.durationTicks))
                    {
                        return;
                    }

                    break;
                }

                case StepType.Wait:
                {
                    if (tickConsumed)
                    {
                        return;
                    }

                    tickConsumed = true;
                    elapsedTicks++;

                    if (elapsedTicks <
                        Mathf.Max(1, step.durationTicks))
                    {
                        return;
                    }

                    break;
                }

                case StepType.StartAttack:
                {
                    if (!step.attack.isActiveAndEnabled)
                    {
                        Abort("StartAttack requires an enabled repeater.");
                        return;
                    }

                    step.attack.Play();

                    if (!step.attack.IsRunning)
                    {
                        Abort("The attack repeater could not start.");
                        return;
                    }

                    break;
                }

                case StepType.StopAttack:
                {
                    step.attack.Stop();
                    break;
                }

                case StepType.WaitForAttack:
                {
                    if (!step.attack.isActiveAndEnabled)
                    {
                        Abort("The awaited attack repeater was disabled.");
                        return;
                    }

                    if (step.attack.IsRunning)
                    {
                        return;
                    }

                    break;
                }

                case StepType.Despawn:
                {
                    Stop();
                    Destroy(gameObject);
                    return;
                }

                default:
                {
                    Abort("Unknown behavior step.");
                    return;
                }
            }

            stepIndex++;
            stepStarted = false;
        }

        if (isRunning)
        {
            // Без Despawn ворог залишається в останній точці.
            Stop();
        }
    }

    private void BeginStep(Step step)
    {
        elapsedTicks = 0;
        stepStarted = true;

        if (step.type != StepType.MoveTo &&
            step.type != StepType.FlyDirection)
        {
            return;
        }

        stepStartPosition = transform.position;

        if (step.type == StepType.FlyDirection)
        {
            // Напрямок обчислюємо один раз на початку кроку.
            float radians = step.angleDegrees * Mathf.Deg2Rad;

            flightDirection = new Vector2(
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            );

            return;
        }

        Vector2 destination;

        switch (step.positionSpace)
        {
            case PositionSpace.SpawnOffset:
                destination =
                    (Vector2)spawnPosition + step.position;
                break;

            case PositionSpace.StepOffset:
                destination =
                    (Vector2)stepStartPosition + step.position;
                break;

            default:
                destination = step.position;
                break;
        }

        // Рух відбувається в XY, глибину об'єкта зберігаємо.
        stepTargetPosition = new Vector3(
            destination.x,
            destination.y,
            stepStartPosition.z
        );
    }

    private bool ValidateSequence()
    {
        if (steps == null || steps.Length == 0)
        {
            Debug.LogError(
                "Enemy Behavior Sequence has no steps.",
                this
            );

            return false;
        }

        for (int index = 0; index < steps.Length; index++)
        {
            Step step = steps[index];

            if (step == null)
            {
                Debug.LogError(
                    $"Enemy behavior step {index} is missing.",
                    this
                );

                return false;
            }

            bool needsAttack =
                step.type == StepType.StartAttack ||
                step.type == StepType.StopAttack ||
                step.type == StepType.WaitForAttack;

            if (needsAttack && step.attack == null)
            {
                Debug.LogError(
                    $"Enemy behavior step {index} needs an attack repeater.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    private void StopAssignedAttacks()
    {
        if (steps == null)
        {
            return;
        }

        foreach (Step step in steps)
        {
            if (step != null && step.attack != null)
            {
                step.attack.Stop();
            }
        }
    }

    private void HandleDeath(EnemyHealth enemy)
    {
        Stop();
    }

    private void Abort(string message)
    {
        Debug.LogError(
            $"Enemy behavior step {stepIndex}: {message}",
            this
        );

        Stop();
    }
}