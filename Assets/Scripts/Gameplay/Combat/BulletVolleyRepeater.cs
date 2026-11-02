using UnityEngine;

[DisallowMultipleComponent]
public sealed class BulletVolleyRepeater : MonoBehaviour
{
    [Header("Volley")]
    [SerializeField]
    private BulletVolleyEmitter volleyEmitter;

    [Header("Timing")]
    [SerializeField]
    private bool playOnEnable = true;

    [SerializeField, Min(0)]
    private int initialDelayTicks;

    [SerializeField]
    [Tooltip("Keep firing until stopped or disabled.")]
    private bool repeatForever;

    [SerializeField, Min(1)]
    private int repeatCount = 5;

    [SerializeField, Min(1)]
    private int intervalTicks = 15;

    [SerializeField, Min(1)]
    [Tooltip("Ticks from the last volley of a series to the next series.")]
    private int seriesDelayTicks = 60;

    [Header("Rotation By Volley")]
    [SerializeField]
    private float initialAngleOffsetDegrees;

    [SerializeField]
    private float angleStepDegrees = 7f;

    [Header("Rotation By Time")]
    [SerializeField]
    [Tooltip("Degrees per second. Positive: counterclockwise.")]
    private float rotationSpeedDegreesPerSecond;

    private bool isRunning;
    private int firedCount;
    private int ticksUntilFire;
    private int elapsedTicks;
    private float inheritedAngleOffset;

    public bool IsRunning => isRunning;

    public int ElapsedTicks => elapsedTicks;

    public float ElapsedSeconds =>
        elapsedTicks * Time.fixedDeltaTime;

    private void OnEnable()
    {
        Stop();

        if (playOnEnable)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        Stop();
    }

    public void Play(float angleOffsetDegrees = 0f)
    {
        Stop();

        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        if (volleyEmitter == null)
        {
            Debug.LogError(
                "Bullet Volley Repeater has no Volley Emitter assigned.",
                this
            );

            return;
        }

        inheritedAngleOffset = angleOffsetDegrees;
        ticksUntilFire = Mathf.Max(0, initialDelayTicks);
        isRunning = true;
    }

    public void Stop()
    {
        isRunning = false;
        firedCount = 0;
        ticksUntilFire = 0;
        elapsedTicks = 0;
        inheritedAngleOffset = 0f;
    }

    private void FixedUpdate()
    {
        if (!isRunning)
        {
            return;
        }

        if (volleyEmitter == null ||
            !volleyEmitter.isActiveAndEnabled)
        {
            Stop();
            return;
        }

        // Время идёт и между залпами, и между сериями.
        float attackTime = ElapsedSeconds;
        elapsedTicks++;

        if (ticksUntilFire > 0)
        {
            ticksUntilFire--;

            if (ticksUntilFire > 0)
            {
                return;
            }
        }

        float angleOffset =
            inheritedAngleOffset +
            initialAngleOffsetDegrees +
            angleStepDegrees * firedCount +
            rotationSpeedDegreesPerSecond * attackTime;

        volleyEmitter.Fire(angleOffset);
        firedCount++;

        int volleysPerSeries = Mathf.Max(1, repeatCount);

        bool seriesFinished =
            firedCount % volleysPerSeries == 0;

        if (seriesFinished)
        {
            if (!repeatForever)
            {
                isRunning = false;
                return;
            }

            ticksUntilFire = Mathf.Max(1, seriesDelayTicks);
        }
        else
        {
            ticksUntilFire = Mathf.Max(1, intervalTicks);
        }
    }

    [ContextMenu("Play Test Series")]
    private void PlayTestSeries()
    {
        Play();
    }

    [ContextMenu("Stop Test Series")]
    private void StopTestSeries()
    {
        Stop();
    }
}