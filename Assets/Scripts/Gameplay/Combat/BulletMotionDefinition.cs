using UnityEngine;

public enum BulletMotionType
{
    Straight,
    Accelerating,
    Sine,
    Spiral
}

[CreateAssetMenu(
    fileName = "BulletMotion",
    menuName = "Fangame/Bullets/Motion"
)]
public sealed class BulletMotionDefinition : ScriptableObject
{
    [SerializeField]
    private BulletMotionType motionType;

    [Header("Acceleration")]
    [SerializeField]
    [Tooltip("Coefficient in speed = initialSpeed + acceleration * time^power.")]
    private float acceleration;

    [SerializeField, Min(1f)]
    [Tooltip("1: linear speed growth, 2: quadratic, 3: cubic.")]
    private float accelerationPower = 1f;

    [Header("Sine")]
    [SerializeField]
    [Tooltip("Maximum sideways offset in world units. Negative values mirror the wave.")]
    private float sineAmplitude = 0.3f;

    [SerializeField, Min(0.01f)]
    [Tooltip("Seconds per full oscillation.")]
    private float sinePeriodSeconds = 2f;

    [Header("Spiral")]
    [SerializeField]
    [Tooltip("Degrees per second. Positive: counterclockwise.")]
    private float spiralAngularSpeedDegreesPerSecond = 30f;

    public BulletMotionType MotionType => motionType;

    public float Acceleration =>
        motionType == BulletMotionType.Accelerating
            ? acceleration
            : 0f;

    public float AccelerationPower =>
        Mathf.Max(1f, accelerationPower);

    public float SineAmplitude => sineAmplitude;

    public float SinePeriodSeconds =>
        Mathf.Max(0.01f, sinePeriodSeconds);

    public float SpiralAngularSpeedDegreesPerSecond =>
        spiralAngularSpeedDegreesPerSecond;
}