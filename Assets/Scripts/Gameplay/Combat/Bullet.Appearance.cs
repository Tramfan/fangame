using UnityEngine;

public sealed partial class Bullet
{
    private SpriteRenderer spriteRenderer;

    private Sprite originalSprite;
    private Material originalMaterial;
    private Color originalColor = Color.white;
    private Vector3 originalVisualScale = Vector3.one;
    private Quaternion originalVisualRotation;
    private bool originalVisualRotationSaved;

    private bool alignVisualToMovement;
    private float visualRotationOffset;

    private float originalHitboxRadius;
    private Vector2 originalHitboxOffset;

    internal void ApplyStyleGeometry(BulletStyleDefinition style)
    {
        bool useStyleHitbox = style != null && style.OverrideHitbox;

        bulletCollider.radius = useStyleHitbox
            ? style.HitboxRadius
            : originalHitboxRadius;

        bulletCollider.offset = useStyleHitbox
            ? style.HitboxOffset
            : originalHitboxOffset;

        if (spriteRenderer == null)
        {
            return;
        }

        if (spriteRenderer.transform != transform)
        {
            float multiplier = style != null
                ? style.VisualScale
                : 1f;

            spriteRenderer.transform.localScale =
                originalVisualScale * multiplier;
        }
    }

    internal void ApplyAppearance(
        BulletStyleDefinition style,
        Color tint
    )
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Transform visual = spriteRenderer.transform;

        if (!originalVisualRotationSaved)
        {
            originalVisualRotation = visual.localRotation;
            originalVisualRotationSaved = true;
        }

        spriteRenderer.sprite = style != null && style.Sprite != null
            ? style.Sprite
            : originalSprite;

        spriteRenderer.sharedMaterial =
            style != null && style.Material != null
                ? style.Material
                : originalMaterial;

        spriteRenderer.color = originalColor * tint;

        alignVisualToMovement =
            style != null &&
            style.AlignToMovement &&
            visual != transform;

        visualRotationOffset = style != null
            ? style.RotationOffsetDegrees
            : 0f;

        visual.localRotation = originalVisualRotation;

        UpdateVisualRotation();
    }

    private void UpdateVisualRotation()
    {
        if (!alignVisualToMovement ||
            spriteRenderer == null ||
            direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        spriteRenderer.transform.rotation =
            Quaternion.Euler(0f, 0f, angle + visualRotationOffset);
    }
}