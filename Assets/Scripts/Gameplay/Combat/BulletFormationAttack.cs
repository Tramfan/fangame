using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BulletFormationAttack : MonoBehaviour
{
    public enum FormationShape
    {
        Circle,
        Outline,
        RandomDisk
    }

    [Header("References")]
    [SerializeField]
    private BulletPool bulletPool;

    [Header("Appearance")]
    [SerializeField]
    private BulletStyleDefinition bulletStyle;

    [SerializeField]
    private Color bulletTint = Color.white;

    [Header("Shape")]
    [SerializeField]
    private FormationShape shape = FormationShape.Circle;

    [SerializeField, Min(1)]
    private int bulletCount = 24;

    [SerializeField, Min(0.01f)]
    private float radius = 1.4f;

    [SerializeField]
    private Vector2[] vertices =
    {
        new(-1f, -1f),
        new(1f, -1f),
        new(1f, 1f),
        new(-1f, 1f)
    };

    [SerializeField, Min(0.01f)]
    private float outlineSpacing = 0.25f;

    [Header("Movement")]
    [SerializeField]
    [Tooltip("Center velocity in world units per second.")]
    private Vector2 centerVelocity;

    [SerializeField]
    private float rotationDegreesPerSecond = 60f;

    [SerializeField, Min(1)]
    private int lifetimeTicks = 360;

    private readonly struct Member
    {
        public readonly Bullet Bullet;
        public readonly Rigidbody2D Body;
        public readonly Vector2 Offset;
        public readonly int Generation;

        public Member(Bullet bullet, Vector2 offset)
        {
            Bullet = bullet;
            Body = bullet.GetComponent<Rigidbody2D>();
            Offset = offset;
            Generation = bullet.SpawnGeneration;
        }
    }

    private readonly List<Member> members = new();

    private Vector2 startCenter;
    private int elapsedTicks;

    private void OnEnable()
    {
        elapsedTicks = 0;
        startCenter = transform.position;

        if (bulletPool == null)
        {
            Debug.LogError("Bullet Formation Attack needs a Bullet Pool.", this);
            return;
        }

        switch (shape)
        {
            case FormationShape.Circle:
                SpawnCircle();
                break;

            case FormationShape.Outline:
                SpawnOutline();
                break;

            case FormationShape.RandomDisk:
                SpawnRandomDisk();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (members.Count == 0)
        {
            return;
        }

        if (elapsedTicks >= Mathf.Max(1, lifetimeTicks))
        {
            ReturnFormation();
            return;
        }

        elapsedTicks++;
        float time = elapsedTicks * Time.fixedDeltaTime;

        Vector2 center = startCenter + centerVelocity * time;
        float angle = rotationDegreesPerSecond * time * Mathf.Deg2Rad;
        float cosine = Mathf.Cos(angle);
        float sine = Mathf.Sin(angle);

        // Один поворот застосовується до всіх точок фігури.
        foreach (Member member in members)
        {
            if (!CanControl(member))
            {
                continue;
            }

            Vector2 offset = member.Offset;
            Vector2 rotatedOffset = new(
                offset.x * cosine - offset.y * sine,
                offset.x * sine + offset.y * cosine
            );

            member.Body.MovePosition(center + rotatedOffset);
        }
    }

    private void OnDisable()
    {
        ReturnFormation();
    }

    private void SpawnCircle()
    {
        int count = Mathf.Max(1, bulletCount);
        float ringRadius = Mathf.Max(0.01f, radius);

        for (int index = 0; index < count; index++)
        {
            float angle = 2f * Mathf.PI * index / count;

            Vector2 offset = new(
                Mathf.Cos(angle) * ringRadius,
                Mathf.Sin(angle) * ringRadius
            );

            SpawnMember(offset);
        }
    }

    private void SpawnOutline()
    {
        if (vertices == null || vertices.Length < 3)
        {
            Debug.LogError("A closed formation outline needs at least three vertices.", this);
            return;
        }

        float spacing = Mathf.Max(0.01f, outlineSpacing);

        for (int index = 0; index < vertices.Length; index++)
        {
            Vector2 start = vertices[index];
            Vector2 end = vertices[(index + 1) % vertices.Length];
            float edgeLength = Vector2.Distance(start, end);

            if (edgeLength <= 0.0001f)
            {
                continue;
            }

            int steps = Mathf.CeilToInt(edgeLength / spacing);

            // Кінцева вершина стане початком наступного ребра.
            for (int step = 0; step < steps; step++)
            {
                float progress = step / (float)steps;
                SpawnMember(Vector2.Lerp(start, end, progress));
            }
        }
    }

    private void SpawnRandomDisk()
    {
        GameplayRandom random = GameplayRandom.Instance;

        if (random == null)
        {
            Debug.LogError("Random Disk needs GameplayRandom in the scene.", this);
            return;
        }

        int count = Mathf.Max(1, bulletCount);
        float diskRadius = Mathf.Max(0.01f, radius);

        for (int index = 0; index < count; index++)
        {
            float angle = random.Range(0f, 2f * Mathf.PI);

            // Корінь дає рівномірний розподіл за площею диска.
            float distance = diskRadius * Mathf.Sqrt(random.Range(0f, 1f));

            Vector2 offset = new(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );

            SpawnMember(offset);
        }
    }

    private void SpawnMember(Vector2 offset)
    {
        Bullet bullet = bulletPool.Spawn(
            startCenter + offset,
            Vector2.zero,
            0f,
            transform.root,
            patternControlled: true,
            style: bulletStyle,
            tint: bulletTint
        );

        if (bullet != null)
        {
            members.Add(new Member(bullet, offset));
        }
    }

    private void ReturnFormation()
    {
        if (bulletPool != null)
        {
            foreach (Member member in members)
            {
                if (CanControl(member))
                {
                    bulletPool.Return(member.Bullet);
                }
            }
        }

        members.Clear();
    }

    private static bool CanControl(Member member)
    {
        return member.Bullet != null &&
            member.Body != null &&
            member.Bullet.gameObject.activeInHierarchy &&
            member.Bullet.SpawnGeneration == member.Generation &&
            member.Bullet.IsPatternControlled;
    }
}