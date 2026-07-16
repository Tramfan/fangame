using UnityEngine;

public enum PrototypePlayerAreaType
{
    Hitbox,
    Graze
}

[RequireComponent(typeof(Collider2D))]
public sealed class PrototypePlayerArea : MonoBehaviour
{
    [SerializeField] private PrototypePlayerAreaType areaType;

    public PrototypePlayerAreaType AreaType => areaType;
}