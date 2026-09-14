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

    private CircleCollider2D circleCollider;

    public static PlayerArea GrazeArea
    {
        get;
        private set;
    }

    public PlayerAreaType AreaType => areaType;

    public Vector2 Center =>
        transform.TransformPoint(
            circleCollider.offset
        );

    public float WorldRadius
    {
        get
        {
            Vector3 scale =
                transform.lossyScale;

            float largestScale = Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y)
            );

            return
                circleCollider.radius *
                largestScale;
        }
    }

    private void Awake()
    {
        circleCollider =
            GetComponent<CircleCollider2D>();
    }

    private void OnEnable()
    {
        if (areaType == PlayerAreaType.Graze)
        {
            GrazeArea = this;
        }
    }

    private void OnDisable()
    {
        if (GrazeArea == this)
        {
            GrazeArea = null;
        }
    }

    public bool ContainsPoint(Vector2 worldPoint)
    {
        Vector2 offset =
            worldPoint - Center;

        float radius = WorldRadius;

        return
            offset.sqrMagnitude <=
            radius * radius;
    }
}