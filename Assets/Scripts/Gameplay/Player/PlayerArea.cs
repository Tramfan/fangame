using UnityEngine;

public enum PlayerAreaType
{
    Hitbox,
    Graze
}

[RequireComponent(typeof(CircleCollider2D))]
public sealed class PlayerArea : MonoBehaviour
{
    [SerializeField]
    private PlayerAreaType areaType;

    [Header("Reflection")]
    [SerializeField]
    private bool reflectionEnabled = true;

    private CircleCollider2D circleCollider;
    private float cachedWorldRadius;

    public static PlayerArea GrazeArea { get; private set; }
    public static PlayerArea HitboxArea { get; private set; }

    public PlayerShield OwnerShield { get; private set; }
    public PlayerState OwnerState { get; private set; }

    public bool ReflectionEnabled => reflectionEnabled;
    public PlayerAreaType AreaType => areaType;

    public Vector2 Center =>
        transform.TransformPoint(circleCollider.offset);

    public float WorldRadius => cachedWorldRadius;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        OwnerShield = GetComponentInParent<PlayerShield>();
        OwnerState = GetComponentInParent<PlayerState>();

        Vector3 scale = transform.lossyScale;
        float largestScale = Mathf.Max(
            Mathf.Abs(scale.x),
            Mathf.Abs(scale.y)
        );

        cachedWorldRadius = circleCollider.radius * largestScale;
    }

    private void OnEnable()
    {
        if (areaType == PlayerAreaType.Graze)
        {
            GrazeArea = this;
        }
        else if (areaType == PlayerAreaType.Hitbox)
        {
            HitboxArea = this;
        }
    }

    private void OnDisable()
    {
        if (GrazeArea == this)
        {
            GrazeArea = null;
        }
        else if (HitboxArea == this)
        {
            HitboxArea = null;
        }
    }

    public void SetWorldRadius(float newRadius)
    {
        cachedWorldRadius = Mathf.Max(0f, newRadius);
    }

    public bool OverlapsCircle(
        Vector2 worldCenter,
        float worldRadius
    )
    {
        return BulletHitboxMath.OverlapsCircle(
            Center,
            cachedWorldRadius,
            worldCenter,
            worldRadius
        );
    }
}