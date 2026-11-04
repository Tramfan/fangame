using UnityEngine;

public static class MotionMath
{
    // Напрямок має бути нормалізованим до виклику.
    public static void EvaluateLinear(
        Vector2 origin,
        Vector2 direction,
        float initialSpeed,
        float acceleration,
        float ageSeconds,
        out Vector2 position,
        out Vector2 velocity,
        float accelerationPower = 1f,
        float minimumSpeed = 0f
    )
    {
        EvaluateTravel(
            initialSpeed,
            acceleration,
            accelerationPower,
            minimumSpeed,
            ageSeconds,
            out float distance,
            out float speed
        );

        position = origin + direction * distance;
        velocity = direction * speed;
    }

    public static void EvaluateSine(
        Vector2 origin,
        Vector2 direction,
        float forwardSpeed,
        float amplitude,
        float periodSeconds,
        float ageSeconds,
        out Vector2 position,
        out Vector2 velocity,
        float acceleration = 0f,
        float accelerationPower = 1f,
        float minimumSpeed = 0f
    )
    {
        float time = Mathf.Max(0f, ageSeconds);

        EvaluateTravel(
            forwardSpeed,
            acceleration,
            accelerationPower,
            minimumSpeed,
            time,
            out float forwardDistance,
            out float currentForwardSpeed
        );

        Vector2 sideways = new(-direction.y, direction.x);
        float period = Mathf.Max(0.01f, periodSeconds);
        float angularFrequency = 2f * Mathf.PI / period;
        float phase = angularFrequency * time;

        float sideOffset = amplitude * Mathf.Sin(phase);
        float sideSpeed =
            amplitude * angularFrequency * Mathf.Cos(phase);

        position =
            origin +
            direction * forwardDistance +
            sideways * sideOffset;

        velocity =
            direction * currentForwardSpeed +
            sideways * sideSpeed;
    }

    public static void EvaluateSpiral(
        Vector2 origin,
        Vector2 direction,
        float radialSpeed,
        float angularSpeedDegreesPerSecond,
        float ageSeconds,
        out Vector2 position,
        out Vector2 velocity,
        float acceleration = 0f,
        float accelerationPower = 1f,
        float minimumSpeed = 0f
    )
    {
        float time = Mathf.Max(0f, ageSeconds);

        EvaluateTravel(
            radialSpeed,
            acceleration,
            accelerationPower,
            minimumSpeed,
            time,
            out float radius,
            out float currentRadialSpeed
        );

        float angularSpeed =
            angularSpeedDegreesPerSecond * Mathf.Deg2Rad;

        float angle = angularSpeed * time;
        Vector2 sideways = new(-direction.y, direction.x);

        Vector2 radialDirection =
            direction * Mathf.Cos(angle) +
            sideways * Mathf.Sin(angle);

        Vector2 tangentDirection = new(
            -radialDirection.y,
            radialDirection.x
        );

        position = origin + radialDirection * radius;

        // Швидкість має радіальну й дотичну складові.
        velocity =
            radialDirection * currentRadialSpeed +
            tangentDirection * (radius * angularSpeed);
    }

    private static void EvaluateTravel(
        float initialSpeed,
        float acceleration,
        float accelerationPower,
        float minimumSpeed,
        float ageSeconds,
        out float distance,
        out float speed
    )
    {
        float time = Mathf.Max(0f, ageSeconds);
        float startSpeed = Mathf.Max(0f, initialSpeed);

        if (acceleration == 0f)
        {
            distance = startSpeed * time;
            speed = startSpeed;
            return;
        }

        float power = Mathf.Max(1f, accelerationPower);
        float travelTime = time;
        float speedFloor = 0f;
        bool reachedFloor = false;

        if (acceleration < 0f)
        {
            speedFloor = Mathf.Clamp(minimumSpeed, 0f, startSpeed);
            float speedDrop = startSpeed - speedFloor;
            float brakingRatio = speedDrop / -acceleration;

            float brakingTime = power == 1f
                ? brakingRatio
                : Mathf.Pow(brakingRatio, 1f / power);

            travelTime = Mathf.Min(time, brakingTime);
            reachedFloor = time >= brakingTime;
        }

        float timePower = power == 1f
            ? travelTime
            : Mathf.Pow(travelTime, power);

        // Після гальмування додаємо шлях, пройдений зі сталою швидкістю.
        distance =
            startSpeed * travelTime +
            acceleration * travelTime * timePower / (power + 1f) +
            speedFloor * (time - travelTime);

        speed = reachedFloor
            ? speedFloor
            : Mathf.Max(speedFloor, startSpeed + acceleration * timePower);
    }
}