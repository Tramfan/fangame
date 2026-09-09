using System;
using UnityEngine;

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

        [SerializeField, Min(1)]
        private int fireIntervalTicks = 6;

        [SerializeField, Min(0)]
        private int firstShotDelayTicks;

        [SerializeField, Min(0f)]
        private float projectileSpeed = 12f;

        [SerializeField, Min(1)]
        private int projectileDamage = 1;

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
    private Emitter[] emitters;

    public Emitter[] Emitters => emitters;
}