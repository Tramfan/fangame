using UnityEngine;

public sealed partial class Bullet
{
    private bool useMotionFunction;
    private BulletMotionType motionType;
    private Vector2 motionOrigin;
    private Vector2 motionDirection;
    private float motionInitialSpeed;
    private float motionAcceleration;
    private float motionAccelerationPower;
    private float motionMinimumSpeed;
    private float motionSineAmplitude;
    private float motionSinePeriod;
    private float motionSpiralAngularSpeed;
    private int motionAgeTicks;

    public void ReleasePatternControl(Vector2 launchVelocity)
    {
        if (!IsPatternControlled)
        {
            return;
        }

        grazedBeforePatternRelease = hasGrantedGraze;

        // Після звільнення рух визначає передана швидкість.
        useMotionFunction = false;

        direction = launchVelocity.sqrMagnitude > 0f
            ? launchVelocity.normalized
            : Vector2.zero;

        speed = launchVelocity.magnitude;
        isPatternControlled = false;

        UpdateVisualRotation();
    }

    public bool TrySetEnemyVelocity(Vector2 velocity)
    {
        if (owner != BulletOwner.Enemy ||
            IsPatternControlled ||
            isTurning)
        {
            return false;
        }

        // Явна команда швидкості замінює попередню траєкторію.
        useMotionFunction = false;

        speed = velocity.magnitude;

        direction = speed > 0f
            ? velocity / speed
            : Vector2.zero;

        UpdateVisualRotation();

        return true;
    }

    private void FixedUpdate()
    {
        UpdateGrazeState(body.position);

        if (IsPatternControlled)
        {
            return;
        }

        if (TryReleaseTimedVolley())
        {
            return;
        }

        Vector2 nextPosition;

        if (isTurning)
        {
            nextPosition = CalculateTurnPosition();
            UpdateVisualRotation();
        }
        else if (useMotionFunction && owner == BulletOwner.Enemy)
        {
            motionAgeTicks++;

            float ageSeconds =
                motionAgeTicks * Time.fixedDeltaTime;

            Vector2 velocity;

            switch (motionType)
            {
                case BulletMotionType.Sine:
                    MotionMath.EvaluateSine(
                        motionOrigin,
                        motionDirection,
                        motionInitialSpeed,
                        motionSineAmplitude,
                        motionSinePeriod,
                        ageSeconds,
                        out nextPosition,
                        out velocity,
                        acceleration: motionAcceleration,
                        accelerationPower: motionAccelerationPower,
                        minimumSpeed: motionMinimumSpeed
                    );
                    break;

                case BulletMotionType.Spiral:
                    MotionMath.EvaluateSpiral(
                        motionOrigin,
                        motionDirection,
                        motionInitialSpeed,
                        motionSpiralAngularSpeed,
                        ageSeconds,
                        out nextPosition,
                        out velocity,
                        acceleration: motionAcceleration,
                        accelerationPower: motionAccelerationPower,
                        minimumSpeed: motionMinimumSpeed
                    );
                    break;

                default:
                    MotionMath.EvaluateLinear(
                        motionOrigin,
                        motionDirection,
                        motionInitialSpeed,
                        motionAcceleration,
                        ageSeconds,
                        out nextPosition,
                        out velocity,
                        accelerationPower: motionAccelerationPower,
                        minimumSpeed: motionMinimumSpeed
                    );
                    break;
            }

            speed = velocity.magnitude;

            bool curvedMotion =
                motionType == BulletMotionType.Sine ||
                motionType == BulletMotionType.Spiral;

            if (curvedMotion &&
                velocity.sqrMagnitude > 0.00000001f)
            {
                Vector2 nextDirection = velocity / speed;

                if (direction != nextDirection)
                {
                    direction = nextDirection;
                    UpdateVisualRotation();
                }
            }
        }
        else
        {
            nextPosition =
                body.position +
                direction * speed * Time.fixedDeltaTime;
        }

        body.MovePosition(nextPosition);

        if (Mathf.Abs(nextPosition.x) > removalBounds.x ||
            Mathf.Abs(nextPosition.y) > removalBounds.y)
        {
            ReturnToPool();
        }
    }
}