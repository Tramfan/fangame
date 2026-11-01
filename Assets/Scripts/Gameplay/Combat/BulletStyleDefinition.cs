using UnityEngine;

[CreateAssetMenu(
    fileName = "NewBulletStyle",
    menuName = "Fangame/Bullets/Bullet Style"
)]
public sealed class BulletStyleDefinition : ScriptableObject
{
    [Header("Appearance")]
    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private Material material;

    [SerializeField, Min(0.01f)]
    [Tooltip(
        "Visual size multiplier. Does not scale the hitbox."
    )]
    private float visualScale = 1f;
[Header("Orientation")]
[SerializeField]
[Tooltip("Point the visual along the movement direction.")]
private bool alignToMovement;

[SerializeField]
[Tooltip("Angle correction for the sprite's original orientation.")]
private float rotationOffsetDegrees;
    [Header("Hitbox")]
    [SerializeField]
    [Tooltip(
        "Use this style's circular hitbox instead of the prefab defaults."
    )]
    private bool overrideHitbox;

    [SerializeField, Min(0.001f)]
    [Tooltip(
        "Radius in the bullet root's local units, before Transform scaling."
    )]
    private float hitboxRadius = 0.1f;

    [SerializeField]
    [Tooltip(
        "Hitbox center relative to the bullet root."
    )]
    private Vector2 hitboxOffset = Vector2.zero;

    public Sprite Sprite => sprite;
    public Material Material => material;

    public float VisualScale =>
        Mathf.Max(0.01f, visualScale);

    public bool OverrideHitbox => overrideHitbox;

    public float HitboxRadius =>
        Mathf.Max(0.001f, hitboxRadius);

    public Vector2 HitboxOffset => hitboxOffset;
    public bool AlignToMovement => alignToMovement;

public float RotationOffsetDegrees =>
    rotationOffsetDegrees;
}