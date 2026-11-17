using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Bullet))]
public sealed class FigureScatterBullet : MonoBehaviour
{
    private Bullet bullet;
    private BulletPool pool;

    public bool IsAvailable =>
        isActiveAndEnabled &&
        bullet != null &&
        bullet.gameObject.activeSelf &&
        bullet.IsPatternControlled;

    public void Prepare(
        Bullet newBullet,
        BulletPool newPool
    )
    {
        bullet = newBullet;
        pool = newPool;

        // Фигура опасна уже во время рисования.
        bullet.CollisionEnabled = true;
        enabled = true;
    }

    public void Release(
        Vector2 velocity
    )
    {
        if (!IsAvailable)
        {
            return;
        }

        bullet.ReleasePatternControlWithVelocity(
            velocity
        );

        // Дальше движение и границы экрана
        // обрабатывает обычный Bullet.
        enabled = false;
    }

    public void ReturnIfWaiting()
    {
        if (IsAvailable)
        {
            pool.Return(bullet);
        }
    }

    private void OnDisable()
    {
        bullet = null;
        pool = null;

        // Prepare включит компонент при
        // следующем использовании этой пули.
        enabled = false;
    }
}