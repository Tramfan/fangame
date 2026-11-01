using UnityEngine;

[DisallowMultipleComponent]
public sealed class BulletVolleyEmitter : MonoBehaviour
{
    public enum VolleyShape
    {
        Ring,
        Fan
    }

    [Header("References")]
    [SerializeField]
    private BulletPool bulletPool;

    [Header("Appearance")]
    [SerializeField]
    private BulletStyleDefinition bulletStyle;

    [SerializeField]
    private Color bulletTint = Color.white;

    [Header("Volley")]
    [SerializeField]
    private VolleyShape shape = VolleyShape.Ring;

    [SerializeField, Min(1)]
    private int bulletCount = 16;

    [SerializeField, Min(0f)]
    private float bulletSpeed = 2.5f;

    [SerializeField]
    [Tooltip("0 right, 90 up, 180 left, -90 down.")]
    private float baseAngleDegrees = -90f;

    [SerializeField, Range(0f, 360f)]
    [Tooltip("Total angle between the outer bullets of a fan.")]
    private float fanSpreadDegrees = 60f;
[Header("Groups")]
[SerializeField, Min(1)]
private int groupCount = 1;

[SerializeField]
private float groupAngleStepDegrees = 30f;
[Header("Aiming")]
[SerializeField]
private bool aimAtPlayer;

[SerializeField]
private Transform target;

public void SetTarget(Transform newTarget)
{
    target = newTarget;
}
    public void SetBulletPool(BulletPool newBulletPool)
    {
        bulletPool = newBulletPool;
    }

   public void Fire(float angleOffsetDegrees = 0f)
{
    if (!Application.isPlaying || !isActiveAndEnabled)
    {
        return;
    }

    if (bulletPool == null)
    {
        Debug.LogError(
            "Bullet Volley Emitter has no Bullet Pool assigned.",
            this
        );

        return;
    }

    int count = Mathf.Max(1, bulletCount);
    int groups = Mathf.Max(1, groupCount);

    float directionAngle = baseAngleDegrees;

if (aimAtPlayer)
{
    if (target == null)
    {
        Debug.LogError(
            "Bullet Volley Emitter has no target assigned.",
            this
        );

        return;
    }

    Vector2 toTarget =
        (Vector2)target.position - (Vector2)transform.position;

    if (toTarget.sqrMagnitude > 0.000001f)
    {
        directionAngle =
            Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
    }
}

float firstAngle = directionAngle + angleOffsetDegrees;
    float bulletAngleStep = 0f;

    if (shape == VolleyShape.Ring)
    {
        bulletAngleStep = 360f / count;
    }
    else if (count > 1)
    {
        float spread = Mathf.Clamp(fanSpreadDegrees, 0f, 360f);

        firstAngle -= spread * 0.5f;
        bulletAngleStep = spread / (count - 1);
    }

    for (int groupIndex = 0; groupIndex < groups; groupIndex++)
    {
        float groupAngleOffset =
            groupAngleStepDegrees * groupIndex;

        for (int bulletIndex = 0; bulletIndex < count; bulletIndex++)
        {
            float angle =
                firstAngle +
                groupAngleOffset +
                bulletAngleStep * bulletIndex;

            float radians = angle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            );

            bulletPool.Spawn(
                transform.position,
                direction,
                Mathf.Max(0f, bulletSpeed),
                transform.root,
                style: bulletStyle,
                tint: bulletTint
            );
        }
    }
}

    [ContextMenu("Fire Test Volley")]
    private void FireTestVolley()
    {
        Fire();
    }
}