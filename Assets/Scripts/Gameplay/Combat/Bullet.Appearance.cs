using UnityEngine;

public sealed partial class Bullet
{
    [Header("Visual Juice")]
    [SerializeField, Min(0)]
    [Tooltip("Duration in fixed ticks of the spawn scale-in flash.")]
    private int spawnFlashTicks = 6;

    [SerializeField, Min(1f)]
    [Tooltip("Initial visual scale multiplier during spawn flash.")]
    private float spawnFlashStartScale = 1.75f;

    [SerializeField, Min(0)]
    [Tooltip("Duration in fixed ticks of the scale pulse when entering graze area.")]
    private int grazeCapturePulseTicks = 8;

    [SerializeField, Min(1f)]
    [Tooltip("Peak visual scale multiplier when captured by graze area.")]
    private float grazeCapturePeakScale = 1.22f;

    [SerializeField, Range(0f, 1f)]
    [Tooltip("White tint blend while bullet is inside an active reflection aura.")]
    private float grazeCaptureBrightness = 0.35f;

    private SpriteRenderer spriteRenderer;

    private Sprite originalSprite;
    private Material originalMaterial;
    private Color originalColor = Color.white;
    private Vector3 originalVisualScale = Vector3.one;
    private Quaternion originalVisualRotation;
    private bool originalVisualRotationSaved;
    private int originalSortingOrder;
    private bool originalSortingOrderSaved;

    private Vector3 baseVisualScale = Vector3.one;
    private Color baseTintColor = Color.white;

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

    private int spawnFlashTicksRemaining;
    private int grazePulseTicksRemaining;
    private bool isVisuallyCapturedInGraze;

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

        // Отключаем обычную вспышку спавна, если используется проявление по дистанции.
        spawnFlashTicksRemaining = 0;

        // Точка появи не рухається разом із босом.
        spawnAppearanceOrigin = position;

        spawnHiddenDistanceSquared =
            spawnHiddenDistance * spawnHiddenDistance;

        spawnVisibleDistanceSquared =
            spawnVisibleDistance * spawnVisibleDistance;

        spawnAppearanceColor = baseTintColor;

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

    internal void OnGrazeCaptureEntered(bool reflectionEnabled)
    {
        if (!reflectionEnabled || owner != BulletOwner.Enemy)
        {
            return;
        }

        isVisuallyCapturedInGraze = true;
        grazePulseTicksRemaining = Mathf.Max(0, grazeCapturePulseTicks);
        RefreshVisualJuiceState();
    }

    internal void OnGrazeCaptureExited()
    {
        if (!isVisuallyCapturedInGraze && grazePulseTicksRemaining <= 0)
        {
            return;
        }

        isVisuallyCapturedInGraze = false;
        RefreshVisualJuiceState();
    }

    internal void UpdateVisualJuice()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (isSpawnAppearancePending)
        {
            UpdateSpawnAppearance();
        }

        bool needsUpdate = false;

        if (spawnFlashTicksRemaining > 0)
        {
            spawnFlashTicksRemaining--;
            needsUpdate = true;
        }

        if (grazePulseTicksRemaining > 0)
        {
            grazePulseTicksRemaining--;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            RefreshVisualJuiceState();
        }
    }

    private void RefreshVisualJuiceState()
    {
        if (spriteRenderer == null || owner != BulletOwner.Enemy)
        {
            return;
        }

        float scaleMultiplier = 1f;

        if (spawnFlashTicksRemaining > 0 && spawnFlashTicks > 0)
        {
            float t = 1f - (spawnFlashTicksRemaining / (float)spawnFlashTicks);
            float eased = 1f - (1f - t) * (1f - t);
            scaleMultiplier *= Mathf.Lerp(spawnFlashStartScale, 1f, eased);
        }

        if (grazePulseTicksRemaining > 0 && grazeCapturePulseTicks > 0)
        {
            float pulseT = grazePulseTicksRemaining / (float)grazeCapturePulseTicks;
            scaleMultiplier *= Mathf.Lerp(1f, grazeCapturePeakScale, pulseT);
        }

        if (spriteRenderer.transform != transform)
        {
            spriteRenderer.transform.localScale = baseVisualScale * scaleMultiplier;
        }

        if (isSpawnAppearancePending)
        {
            return;
        }

        Color targetColor = baseTintColor;

        if (isVisuallyCapturedInGraze && grazeCaptureBrightness > 0f)
        {
            Color highlight = Color.Lerp(
                baseTintColor,
                Color.white,
                grazeCaptureBrightness
            );
            highlight.a = baseTintColor.a;
            targetColor = highlight;
        }

        if (spawnFlashTicksRemaining > 0 && spawnFlashTicks > 0)
        {
            float alphaT = 1f - (spawnFlashTicksRemaining / (float)spawnFlashTicks);
            targetColor.a *= Mathf.Lerp(0.35f, 1f, alphaT);
        }

        spriteRenderer.color = targetColor;
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

        float multiplier = style != null
            ? style.VisualScale
            : 1f;

        baseVisualScale = originalVisualScale * multiplier;

        if (spriteRenderer != null &&
            spriteRenderer.transform != transform)
        {
            spriteRenderer.transform.localScale = baseVisualScale;
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

        baseTintColor = originalColor * tint;
        spriteRenderer.color = baseTintColor;

        isSpawnAppearancePending = false;
        isVisuallyCapturedInGraze = false;
        grazePulseTicksRemaining = 0;
        spawnFlashTicksRemaining = Mathf.Max(0, spawnFlashTicks);

        alignVisualToMovement =
            style != null &&
            style.AlignToMovement &&
            visual != transform;

        visualRotationOffset = style != null
            ? style.RotationOffsetDegrees
            : 0f;

        visual.localRotation = originalVisualRotation;

        UpdateVisualRotation();
        RefreshVisualJuiceState();
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