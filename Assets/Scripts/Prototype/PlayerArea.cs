using UnityEngine;

public enum PlayerAreaType
{
    Hitbox,
    Graze
}

[RequireComponent(typeof(Collider2D))]
public sealed class PlayerArea : MonoBehaviour
{
    [SerializeField]
    private PlayerAreaType areaType;

    public PlayerAreaType AreaType => areaType;
}