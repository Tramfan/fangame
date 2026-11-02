using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BulletCountDisplay : MonoBehaviour
{
    [SerializeField]
    private BulletPool bulletPool;

    [SerializeField]
    private TMP_Text counterText;

    [SerializeField, Min(0.05f)]
    private float refreshInterval = 0.2f;

    private float nextRefreshTime;
    private int lastCount = -1;

    private void OnEnable()
    {
        nextRefreshTime = 0f;
        lastCount = -1;
    }

    private void Update()
    {
        if (bulletPool == null || counterText == null)
        {
            return;
        }

        if (Time.unscaledTime < nextRefreshTime)
        {
            return;
        }

        nextRefreshTime =
            Time.unscaledTime + Mathf.Max(0.05f, refreshInterval);

        int count = bulletPool.ActiveCount;

        if (count == lastCount)
        {
            return;
        }

        lastCount = count;
        counterText.text = $"Bullets: {count}";
    }
}