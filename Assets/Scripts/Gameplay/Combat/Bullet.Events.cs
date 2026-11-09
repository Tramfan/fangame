using UnityEngine;

public sealed partial class Bullet
{
    private BulletVolleyEmitter timedVolleyEmitter;
    private int ticksUntilTimedVolley = -1;
    private bool returnAfterTimedVolley;

    internal void ArmTimedVolley(
        BulletVolleyEmitter emitter,
        int delayTicks,
        bool returnAfterVolley
    )
    {
        timedVolleyEmitter = emitter;
        ticksUntilTimedVolley = Mathf.Max(1, delayTicks);
        returnAfterTimedVolley = returnAfterVolley;
    }

    private bool TryReleaseTimedVolley()
    {
        if (ticksUntilTimedVolley < 0)
        {
            return false;
        }

        if (owner != BulletOwner.Enemy ||
            timedVolleyEmitter == null ||
            !timedVolleyEmitter.isActiveAndEnabled)
        {
            timedVolleyEmitter = null;
            ticksUntilTimedVolley = -1;
            return false;
        }

        if (ticksUntilTimedVolley > 0)
        {
            ticksUntilTimedVolley--;
            return false;
        }

        // Знімаємо подію перед пострілом, щоб вона спрацювала один раз.
        BulletVolleyEmitter emitter = timedVolleyEmitter;
        timedVolleyEmitter = null;
        ticksUntilTimedVolley = -1;

        if (emitter.FireAt(body.position, source) && returnAfterTimedVolley)
        {
            ReturnToPool();
            return true;
        }

        return false;
    }
}