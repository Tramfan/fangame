using UnityEngine;

public interface IPlayerProjectile
{
    void Initialize(
        Vector2 direction,
        float speed,
        int damage
    );
}