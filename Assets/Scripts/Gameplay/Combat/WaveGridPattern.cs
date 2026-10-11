using System.Collections.Generic;
using UnityEngine;

public sealed class WaveGridPattern : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform center;

    [SerializeField]
    private BulletPool bulletPool;

    [Header("Grid")]
    [SerializeField, Min(0)]
    private int halfWidth = 5;

    [SerializeField, Min(0)]
    private int halfHeight = 5;

    [SerializeField, Min(0.01f)]
    private float spacing = 0.7f;

    [Header("Cross Motion")]
    [SerializeField]
    private float baseOffset = 0.06f;

    [SerializeField]
    private float amplitude = 0.18f;

    [Header("Wave")]
    [SerializeField]
    private float spatialFrequency = 3.2f;

    [SerializeField]
    private float waveSpeed = 2.25f;

    [Header("Assembly")]
    [SerializeField, Min(1)]
    [Tooltip("How many grid centers appear each fixed tick.")]
    private int nodesPerTick = 3;

    [SerializeField, Min(0)]
    [Tooltip("Extra safe ticks after the grid is complete.")]
    private int safeTicksAfterBuild = 30;

    private sealed class PatternBullet
    {
        public Bullet bullet;
        public Rigidbody2D body;
        public CircleCollider2D collider;
    }

    private sealed class GridNode
    {
        public Vector2 localCenter;
        public PatternBullet right;
        public PatternBullet left;
        public PatternBullet up;
        public PatternBullet down;
    }

    private readonly List<GridNode> nodes = new();
    private int nextNodeIndex;
    private int elapsedSafeTicks;
    private float phaseTime;

    private void OnEnable()
    {
        if (bulletPool == null)
        {
            Debug.LogError(
                "Wave Grid Pattern needs a Bullet Pool.",
                this
            );

            enabled = false;
            return;
        }

        nextNodeIndex = 0;
        elapsedSafeTicks = 0;
        phaseTime = 0f;

        PrepareNodes();
    }

    private void OnDisable()
    {
        foreach (GridNode node in nodes)
        {
            ReturnBullet(node.right);
            ReturnBullet(node.left);
            ReturnBullet(node.up);
            ReturnBullet(node.down);
        }

        nodes.Clear();
    }

    private void FixedUpdate()
    {
        if (nextNodeIndex < nodes.Count)
        {
            int amount =
                Mathf.Max(1, nodesPerTick);

            for (int index = 0;
                 index < amount &&
                 nextNodeIndex < nodes.Count;
                 index++)
            {
                SpawnNode(nodes[nextNodeIndex]);
                nextNodeIndex++;
            }

            return;
        }

        if (elapsedSafeTicks <
            Mathf.Max(0, safeTicksAfterBuild))
        {
            elapsedSafeTicks++;
            return;
        }

        phaseTime += Time.fixedDeltaTime;

        UpdateGrid();
        ArmSafeBullets();
    }

    private void PrepareNodes()
    {
        nodes.Clear();

        for (int y = -halfHeight;
             y <= halfHeight;
             y++)
        {
            for (int x = -halfWidth;
                 x <= halfWidth;
                 x++)
            {
                nodes.Add(new GridNode
                {
                    localCenter = new Vector2(
                        x * spacing,
                        y * spacing
                    )
                });
            }
        }

        nodes.Sort(CompareNodesFromCenter);
    }

    private void SpawnNode(GridNode node)
    {
        Vector2 worldCenter =
            GetCenterPosition() +
            node.localCenter;

        float offset =
            GetOffset(node.localCenter);

        node.right = CreateBullet(
            worldCenter +
            Vector2.right * offset
        );

        node.left = CreateBullet(
            worldCenter +
            Vector2.left * offset
        );

        node.up = CreateBullet(
            worldCenter +
            Vector2.up * offset
        );

        node.down = CreateBullet(
            worldCenter +
            Vector2.down * offset
        );
    }

    private PatternBullet CreateBullet(
        Vector2 position
    )
    {
        Bullet bullet = bulletPool.Spawn(
            position,
            Vector2.zero,
            0f,
            transform.root,
            true
        );

        if (bullet == null)
        {
            return null;
        }

        CircleCollider2D bulletCollider =
            bullet.GetComponent<CircleCollider2D>();

        bulletCollider.enabled = false;

        return new PatternBullet
        {
            bullet = bullet,
            body = bullet.GetComponent<Rigidbody2D>(),
            collider = bulletCollider
        };
    }

    private void UpdateGrid()
    {
        Vector2 origin = GetCenterPosition();

        foreach (GridNode node in nodes)
        {
            Vector2 nodeWorld =
                origin + node.localCenter;

            float offset =
                GetOffset(node.localCenter);

            SetBulletPosition(
                node.right,
                nodeWorld +
                Vector2.right * offset
            );

            SetBulletPosition(
                node.left,
                nodeWorld +
                Vector2.left * offset
            );

            SetBulletPosition(
                node.up,
                nodeWorld +
                Vector2.up * offset
            );

            SetBulletPosition(
                node.down,
                nodeWorld +
                Vector2.down * offset
            );
        }
    }

    private float GetOffset(
        Vector2 localCenter
    )
    {
        float wave = Mathf.Sin(
            localCenter.magnitude *
            spatialFrequency -
            phaseTime * waveSpeed
        );

        return baseOffset +
            amplitude * wave;
    }

    private void ArmSafeBullets()
    {
        PlayerArea grazeArea =
            PlayerArea.GrazeArea;

        foreach (GridNode node in nodes)
        {
            ArmBullet(node.right, grazeArea);
            ArmBullet(node.left, grazeArea);
            ArmBullet(node.up, grazeArea);
            ArmBullet(node.down, grazeArea);
        }
    }

    private static void ArmBullet(
        PatternBullet patternBullet,
        PlayerArea grazeArea
    )
    {
        if (!CanControl(patternBullet) ||
            patternBullet.collider.enabled)
        {
            return;
        }

        if (grazeArea != null &&
            OverlapsPlayerArea(
                patternBullet.collider,
                grazeArea
            ))
        {
            return;
        }

        patternBullet.collider.enabled = true;
    }

    private static void SetBulletPosition(
        PatternBullet patternBullet,
        Vector2 position
    )
    {
        if (!CanControl(patternBullet))
        {
            return;
        }

        patternBullet.body.position =
            position;
    }

    private void ReturnBullet(
        PatternBullet patternBullet
    )
    {
        if (CanControl(patternBullet))
        {
            bulletPool.Return(
                patternBullet.bullet
            );
        }
    }

    private Vector2 GetCenterPosition()
    {
        return center != null
            ? center.position
            : transform.position;
    }

    private static bool CanControl(
        PatternBullet patternBullet
    )
    {
        return patternBullet != null &&
            patternBullet.bullet != null &&
            patternBullet.body != null &&
            patternBullet.collider != null &&
            patternBullet.bullet.gameObject.activeSelf &&
            patternBullet.bullet.IsPatternControlled;
    }

    private static bool OverlapsPlayerArea(
        CircleCollider2D bulletCollider,
        PlayerArea area
    )
    {
        Vector2 bulletCenter =
            bulletCollider.transform.TransformPoint(
                bulletCollider.offset
            );

        Vector3 scale =
            bulletCollider.transform.lossyScale;

        float bulletRadius =
            bulletCollider.radius *
            Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y)
            );

        return area.OverlapsCircle(
            bulletCenter,
            bulletRadius
        );
    }

    private static int CompareNodesFromCenter(
        GridNode left,
        GridNode right
    )
    {
        int distanceComparison =
            left.localCenter.sqrMagnitude
                .CompareTo(
                    right.localCenter.sqrMagnitude
                );

        if (distanceComparison != 0)
        {
            return distanceComparison;
        }

        int verticalComparison =
            left.localCenter.y.CompareTo(
                right.localCenter.y
            );

        return verticalComparison != 0
            ? verticalComparison
            : left.localCenter.x.CompareTo(
                right.localCenter.x
            );
    }
}