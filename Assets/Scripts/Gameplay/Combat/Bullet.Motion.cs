using UnityEngine;

public sealed partial class Bullet
{
    public void ReleasePatternControl()
    {
        if (!isPatternControlled)
        {
            return;
        }

        isPatternControlled = false;
        grazedBeforePatternRelease = false;
    }

    public void ReleasePatternControl(Vector2 playerCenter)
    {
        ReleasePatternControl();
    }

    internal void StepSimulation()
    {
        if (!IsActiveInPool)
        {
            return;
        }

        previousPosition = position;

        if (!IsPatternControlled)
        {
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
                float ageSeconds = motionAgeTicks * Time.fixedDeltaTime;
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

                if ((motionType == BulletMotionType.Sine ||
                     motionType == BulletMotionType.Spiral) &&
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
                    position + direction * speed * Time.fixedDeltaTime;
            }

            position = nextPosition;
            transform.position = new Vector3(position.x, position.y, 0f);

            if (Mathf.Abs(position.x) > removalBounds.x ||
                Mathf.Abs(position.y) > removalBounds.y)
            {
                ReturnToPool();
                return;
            }
        }

       // Невидимі кулі та кулі під час проявлення ще безпечні.
if (UpdateSpawnAppearance())
{
    EvaluateCollisions();
}
    }
}