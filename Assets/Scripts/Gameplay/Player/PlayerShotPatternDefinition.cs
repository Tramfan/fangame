using System;
using UnityEngine;

public enum PlayerShotOriginType : byte
{
    Player = 0,
    Option = 1
}
public enum PlayerShotFireMode : byte
{
    Repeated = 0,
    Continuous = 1
}
[CreateAssetMenu(
    fileName = "PlayerShotPattern",
    menuName = "Fangame/Player Shot Pattern"
)]
public sealed class PlayerShotPatternDefinition :
    ScriptableObject
{
    [Serializable]
    public sealed class Emitter
    {
        [Header("Origin")]
        [SerializeField]
        private PlayerShotOriginType originType =
            PlayerShotOriginType.Player;

        [Tooltip(
            "Zero-based option index. " +
            "Used only when Origin Type is Option."
        )]
        [SerializeField, Min(0)]
        private int optionIndex;
[Header("Firing")]
[SerializeField]

private PlayerShotFireMode fireMode =
    PlayerShotFireMode.Repeated;
        [Header("Projectile")]
        [SerializeField]
        private GameObject projectilePrefab;

        [SerializeField]
        private Vector2 localOffset;

        [Tooltip(
            "0 means straight up. " +
            "Positive values turn counter-clockwise."
        )]
        [SerializeField]
        private float angleOffsetDegrees;

        [SerializeField, Min(1)]
        private int projectileCount = 1;

        [SerializeField, Min(0f)]
        private float spreadDegrees;

        [Header("Timing")]
        [SerializeField, Min(1)]
        private int fireIntervalTicks = 6;

        [SerializeField, Min(0)]
        private int firstShotDelayTicks;

        [Header("Projectile Values")]
        [SerializeField, Min(0f)]
        private float projectileSpeed = 12f;

        [SerializeField, Min(1)]
        private int projectileDamage = 1;

        public PlayerShotOriginType OriginType =>
            originType;

        public int OptionIndex =>
            optionIndex;
public PlayerShotFireMode FireMode =>
    fireMode;
        public GameObject ProjectilePrefab =>
            projectilePrefab;

        public Vector2 LocalOffset =>
            localOffset;

        public float AngleOffsetDegrees =>
            angleOffsetDegrees;

        public int ProjectileCount =>
            projectileCount;

        public float SpreadDegrees =>
            spreadDegrees;

        public int FireIntervalTicks =>
            fireIntervalTicks;

        public int FirstShotDelayTicks =>
            firstShotDelayTicks;

        public float ProjectileSpeed =>
            projectileSpeed;

        public int ProjectileDamage =>
            projectileDamage;
    }

    [SerializeField]
    private Emitter[] emitters =
        Array.Empty<Emitter>();

    public Emitter[] Emitters =>
        emitters;
}