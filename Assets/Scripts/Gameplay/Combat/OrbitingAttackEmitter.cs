using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-200)]
public sealed class OrbitingBossEmitter : MonoBehaviour
{
    private sealed class EmitterInstance
    {
        public Transform Anchor;
        public GameObject Attack;
        public float StartAngle;
    }

    [Header("References")]
    [SerializeField]
    private GameObject attackRoot;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private BulletPool bulletPool;

    [Header("Emitters")]
    [SerializeField, Min(1)]
    private int emitterCount = 2;

    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float orbitRadius = 1.5f;

    [SerializeField, Min(1)]
    private int launchTicks = 45;

    [SerializeField]
    [Tooltip("Starting angle of the first emitter.")]
    private float startAngleDegrees;

    [SerializeField]
    [Tooltip(
        "Positive: counterclockwise. " +
        "Negative: clockwise."
    )]
    private float degreesPerSecond = 90f;

    private readonly List<EmitterInstance> emitters =
        new();

    private int elapsedLaunchTicks;
    private float orbitAngleOffset;

    public bool IsFiring { get; private set; }

    private void OnEnable()
    {
        IsFiring = false;
        elapsedLaunchTicks = 0;
        orbitAngleOffset = 0f;

        if (attackRoot == null ||
            attackRoot == gameObject ||
            !attackRoot.transform.IsChildOf(transform))
        {
            Debug.LogError(
                "Orbiting Boss Emitter needs an " +
                "Attack Root that is its child object.",
                this
            );

            enabled = false;
            return;
        }

        // Attack Root служит образцом. Сам он
        // никогда не участвует в стрельбе.
        attackRoot.SetActive(false);

        if (!HasRequiredReferences())
        {
            enabled = false;
            return;
        }

        CreateEmitters();
    }

    private void OnDisable()
    {
        IsFiring = false;

        foreach (EmitterInstance emitter in emitters)
        {
            if (emitter.Attack != null)
            {
                emitter.Attack.SetActive(false);
            }

            if (emitter.Anchor != null)
            {
                emitter.Anchor.gameObject.SetActive(false);
                Destroy(emitter.Anchor.gameObject);
            }
        }

        emitters.Clear();

        if (attackRoot != null)
        {
            attackRoot.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!IsFiring)
        {
            elapsedLaunchTicks++;

            float progress = Mathf.Clamp01(
                elapsedLaunchTicks /
                (float)Mathf.Max(1, launchTicks)
            );

            float currentRadius =
                Mathf.Max(0f, orbitRadius) *
                progress;

            UpdateEmitterPositions(
                currentRadius
            );

            if (elapsedLaunchTicks >=
                Mathf.Max(1, launchTicks))
            {
                IsFiring = true;

                foreach (
                    EmitterInstance emitter
                    in emitters
                )
                {
                    emitter.Attack.SetActive(true);
                }
            }

            return;
        }

        orbitAngleOffset +=
            degreesPerSecond *
            Time.fixedDeltaTime;

        UpdateEmitterPositions(
            Mathf.Max(0f, orbitRadius)
        );
    }

    private bool HasRequiredReferences()
    {
        bool needsTarget =
            attackRoot
                .GetComponentInChildren<
                    AimedFanEmitter
                >(true) != null ||
            attackRoot
                .GetComponentInChildren<
                    EnemyLaserEmitter
                >(true) != null;

        bool needsPool =
            attackRoot
                .GetComponentInChildren<
                    AimedFanEmitter
                >(true) != null ||
            attackRoot
                .GetComponentInChildren<
                    RingEmitter
                >(true) != null;

        if (needsTarget &&
            target == null)
        {
            Debug.LogError(
                "Orbiting Boss Emitter needs " +
                "a Target for this attack.",
                this
            );

            return false;
        }

        if (needsPool &&
            bulletPool == null)
        {
            Debug.LogError(
                "Orbiting Boss Emitter needs " +
                "a Bullet Pool for this attack.",
                this
            );

            return false;
        }

        return true;
    }

    private void CreateEmitters()
    {
        int count =
            Mathf.Max(1, emitterCount);

        float angleStep =
            360f / count;

        for (
            int index = 0;
            index < count;
            index++
        )
        {
            GameObject anchorObject =
                new GameObject(
                    "Orbiting Emitter " +
                    (index + 1)
                );

            Transform anchor =
                anchorObject.transform;

            anchor.SetParent(
                transform,
                false
            );

            anchor.localPosition =
                Vector3.zero;

            GameObject attack =
                Instantiate(
                    attackRoot,
                    anchor,
                    false
                );

            attack.name =
                attackRoot.name;

            attack.transform.localPosition =
                Vector3.zero;

            attack.transform.localRotation =
                Quaternion.identity;

            attack.SetActive(false);

            EmitterInstance emitter =
                new EmitterInstance
                {
                    Anchor = anchor,
                    Attack = attack,
                    StartAngle =
                        startAngleDegrees +
                        angleStep * index
                };

            emitters.Add(emitter);

            ConfigureAttack(attack);
        }

        UpdateEmitterPositions(0f);
    }

    private void ConfigureAttack(
        GameObject attack
    )
    {
        AimedFanEmitter[] fans =
            attack.GetComponentsInChildren<
                AimedFanEmitter
            >(true);

        RingEmitter[] rings =
            attack.GetComponentsInChildren<
                RingEmitter
            >(true);

        EnemyLaserEmitter[] lasers =
            attack.GetComponentsInChildren<
                EnemyLaserEmitter
            >(true);

        foreach (
            AimedFanEmitter fan
            in fans
        )
        {
            fan.Configure(
                target,
                bulletPool
            );
        }

        foreach (
            RingEmitter ring
            in rings
        )
        {
            ring.SetBulletPool(
                bulletPool
            );
        }

        foreach (
            EnemyLaserEmitter laser
            in lasers
        )
        {
            laser.Configure(
                target
            );
        }
    }

    private void UpdateEmitterPositions(
        float radius
    )
    {
        foreach (
            EmitterInstance emitter
            in emitters
        )
        {
            float angle =
                emitter.StartAngle +
                orbitAngleOffset;

            float radians =
                angle *
                Mathf.Deg2Rad;

            emitter.Anchor.localPosition =
                new Vector3(
                    Mathf.Cos(radians) *
                        radius,
                    Mathf.Sin(radians) *
                        radius,
                    0f
                );
        }
    }
}