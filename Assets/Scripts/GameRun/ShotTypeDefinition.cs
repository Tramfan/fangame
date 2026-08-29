using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(
    fileName = "ShotType",
    menuName = "Fangame/Shot Type Definition"
)]
public sealed class ShotTypeDefinition :
    ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private LocalizedString displayName;

    [Header("Gameplay")]
    [SerializeField]
    private PlayerBullet bulletPrefab;

    [SerializeField, Min(1)]
    private int fireIntervalTicks = 6;

    [SerializeField]
    private bool shootingEnabled = true;

    public string Id => id;

    public LocalizedString DisplayName =>
        displayName;

    public PlayerBullet BulletPrefab =>
        bulletPrefab;

    public int FireIntervalTicks =>
        fireIntervalTicks;

    public bool ShootingEnabled =>
        shootingEnabled;
}