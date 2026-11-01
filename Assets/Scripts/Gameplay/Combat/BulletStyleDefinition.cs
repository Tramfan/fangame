
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

    public Sprite Sprite => sprite;
    public Material Material => material;
}
