using UnityEngine;

public enum BulletColorPreset
{
    Custom = 0,
    Red = 1,
    Blue = 2,
    Green = 3,
    Yellow = 4,
    Purple = 5,
    Cyan = 6,
    Orange = 7,
    White = 8
}

public static class BulletColorUtility
{
    public static Color Resolve(BulletColorPreset preset, Color customColor)
    {
        return preset switch
        {
            BulletColorPreset.Red => new Color(1f, 0.22f, 0.28f, 1f),
            BulletColorPreset.Blue => new Color(0.24f, 0.55f, 1f, 1f),
            BulletColorPreset.Green => new Color(0.22f, 0.92f, 0.45f, 1f),
            BulletColorPreset.Yellow => new Color(1f, 0.88f, 0.2f, 1f),
            BulletColorPreset.Purple => new Color(0.78f, 0.32f, 1f, 1f),
            BulletColorPreset.Cyan => new Color(0.2f, 0.95f, 1f, 1f),
            BulletColorPreset.Orange => new Color(1f, 0.52f, 0.16f, 1f),
            BulletColorPreset.White => Color.white,
            _ => customColor
        };
    }
}