using UnityEngine;

public static class MotionMath
{
     // Рух уздовж прямої зі степеневим законом швидкості.
    // Від'ємний коефіцієнт гальмує об'єкт до зупинки.
    public static void EvaluateLinear(
        Vector2 origin,
        Vector2 direction,
        float initialSpeed,
        float acceleration,
        float ageSeconds,
        out Vector2 position,
        out Vector2 velocity,
        float accelerationPower = 1f
    )
    {
        float time = Mathf.Max(0f, ageSeconds);
        float startSpeed = Mathf.Max(0f, initialSpeed);

        // Для сталої швидкості степеневі обчислення не потрібні.
        if (acceleration == 0f)
        {
            position = origin + direction * startSpeed * time;
            velocity = direction * startSpeed;
            return;
        }

        float power = Mathf.Max(1f, accelerationPower);
        float movementTime = time;

        if (acceleration < 0f)
        {
            float stopRatio = startSpeed / -acceleration;

            float stopTime = power == 1f
                ? stopRatio
                : Mathf.Pow(stopRatio, 1f / power);

            movementTime = Mathf.Min(time, stopTime);
        }

        float timePower = power == 1f
            ? movementTime
            : Mathf.Pow(movementTime, power);

        // Відстань є інтегралом швидкості:
        // v0 * t + a * t^(p + 1) / (p + 1).
        float distance =
            startSpeed * movementTime +
            acceleration * movementTime * timePower / (power + 1f);

        float currentSpeed = Mathf.Max(
            0f,
            startSpeed + acceleration * timePower
        );

        position = origin + direction * distance;
        velocity = direction * currentSpeed;
    }

    // Волна относительно исходного направления полёта.
    public static void EvaluateSine(
        Vector2 origin,
        Vector2 direction,
        float forwardSpeed,
        float amplitude,
        float periodSeconds,
        float ageSeconds,
        out Vector2 position,
        out Vector2 velocity
    )
    {
        float time = Mathf.Max(0f, ageSeconds);
        float speed = Mathf.Max(0f, forwardSpeed);
        float period = Mathf.Max(0.01f, periodSeconds);

        Vector2 sideways = new(-direction.y, direction.x);

        float angularFrequency = 2f * Mathf.PI / period;
        float phase = angularFrequency * time;

        float sideOffset = amplitude * Mathf.Sin(phase);

        float sideSpeed =
            amplitude * angularFrequency * Mathf.Cos(phase);

        position =
            origin +
            direction * speed * time +
            sideways * sideOffset;

        // Производная позиции — скорость по касательной.
        velocity =
            direction * speed +
            sideways * sideSpeed;
            
    }
     // direction має бути нормалізованим.
    // Початкова точка залишається нерухомим центром спіралі.
    public static void EvaluateSpiral(
        Vector2 origin,
        Vector2 direction,
        float radialSpeed,
        float angularSpeedDegreesPerSecond,
        float ageSeconds,
        out Vector2 position,
        out Vector2 velocity
    )
    {
        float time = Mathf.Max(0f, ageSeconds);
        float speed = Mathf.Max(0f, radialSpeed);

        float angularSpeed =
            angularSpeedDegreesPerSecond * Mathf.Deg2Rad;

        float angle = angularSpeed * time;
        float radius = speed * time;

        Vector2 sideways = new(-direction.y, direction.x);

        Vector2 radialDirection =
            direction * Mathf.Cos(angle) +
            sideways * Mathf.Sin(angle);

        Vector2 tangentDirection = new(
            -radialDirection.y,
            radialDirection.x
        );

        position = origin + radialDirection * radius;

        // Повна швидкість поєднує віддалення від центру
        // та рух навколо нього.
        velocity =
            radialDirection * speed +
            tangentDirection * (radius * angularSpeed);
    }
}