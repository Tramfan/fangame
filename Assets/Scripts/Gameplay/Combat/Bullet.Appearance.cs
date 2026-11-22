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
    private int originalSortingOrder;
    private bool originalSortingOrderSaved;

    private bool alignVisualToMovement;
    private float visualRotationOffset;

    private bool isSpawnAppearancePending;
    private Vector2 spawnAppearanceOrigin;
    private Color spawnAppearanceColor;
    private float spawnHiddenDistance;
    private float spawnVisibleDistance;
    private float spawnHiddenDistanceSquared;
    private float spawnVisibleDistanceSquared;
    private float spawnOpacity;

    internal void ConfigureSpawnAppearance(
        float hiddenDistance,
        float fadeDistance
    )
    {
        spawnHiddenDistance = Mathf.Max(0f, hiddenDistance);
        spawnVisibleDistance =
            spawnHiddenDistance + Mathf.Max(0f, fadeDistance);

        if (spawnVisibleDistance <= 0f)
        {
            return;
        }

        // Точка появи не рухається разом із босом.
        spawnAppearanceOrigin = position;

        spawnHiddenDistanceSquared =
            spawnHiddenDistance * spawnHiddenDistance;

        spawnVisibleDistanceSquared =
            spawnVisibleDistance * spawnVisibleDistance;

        spawnAppearanceColor = spriteRenderer != null
            ? spriteRenderer.color
            : Color.white;

        isSpawnAppearancePending = true;
        spawnOpacity = 0f;
        SetSpawnOpacity(0f);

        // Невидимий рух не повинен створювати вихід із зони грейзу.
        wasInsideGrazeArea = false;
        ignoreGrazeUntilExit = false;
    }

    private bool UpdateSpawnAppearance()
    {
        if (!isSpawnAppearancePending)
        {
            return true;
        }

        float distanceSquared =
            (position - spawnAppearanceOrigin).sqrMagnitude;

        if (distanceSquared >= spawnVisibleDistanceSquared)
        {
            SetSpawnOpacity(1f);
            isSpawnAppearancePending = false;
            return true;
        }

        if (distanceSquared <= spawnHiddenDistanceSquared)
        {
            return false;
        }

        float opacity = Mathf.InverseLerp(
            spawnHiddenDistance,
            spawnVisibleDistance,
            Mathf.Sqrt(distanceSquared)
        );

        // Прозорість змінюється лише в бік повної видимості.
        if (opacity > spawnOpacity)
        {
            spawnOpacity = opacity;
            SetSpawnOpacity(opacity);
        }

        return false;
    }

    private void SetSpawnOpacity(float opacity)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = spawnAppearanceColor;
        color.a *= opacity;
        spriteRenderer.color = color;
    }

    internal void ApplyStyleGeometry(BulletStyleDefinition style)
    {
        bool useStyleHitbox = style != null && style.OverrideHitbox;

        Vector3 scale = transform.lossyScale;

        float largestScale = Mathf.Max(
            Mathf.Abs(scale.x),
            Mathf.Abs(scale.y)
        );

        float rawRadius = useStyleHitbox
            ? style.HitboxRadius
            : defaultHitboxRadius;

        Vector2 rawOffset = useStyleHitbox
            ? style.HitboxOffset
            : defaultHitboxOffset;

        cachedHitboxRadius = rawRadius * largestScale;

        cachedHitboxOffset = new Vector2(
            rawOffset.x * scale.x,
            rawOffset.y * scale.y
        );

        if (spriteRenderer != null &&
            spriteRenderer.transform != transform)
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

        if (!originalSortingOrderSaved)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            originalSortingOrderSaved = true;
        }

        spriteRenderer.sprite = style != null && style.Sprite != null
            ? style.Sprite
            : originalSprite;

        spriteRenderer.sharedMaterial =
            style != null && style.Material != null
                ? style.Material
                : originalMaterial;

        spriteRenderer.sortingOrder = style != null
            ? style.SortingOrder
            : originalSortingOrder;

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