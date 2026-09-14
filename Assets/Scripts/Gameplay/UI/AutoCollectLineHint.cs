using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public sealed class AutoCollectLineHint :
    MonoBehaviour
{
    [SerializeField, Min(1)]
    private int visibleTicks = 120;

    [SerializeField, Min(0)]
    private int fadeTicks = 30;

    private CanvasGroup canvasGroup;
    private int elapsedTicks;

    private void Awake()
    {
        canvasGroup =
            GetComponent<CanvasGroup>();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        elapsedTicks = 0;
        canvasGroup.alpha = 1f;
    }

    private void FixedUpdate()
    {
        elapsedTicks++;

        int remainingTicks =
            visibleTicks - elapsedTicks;

        if (remainingTicks <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        if (fadeTicks > 0 &&
            remainingTicks <= fadeTicks)
        {
            canvasGroup.alpha =
                remainingTicks /
                (float)fadeTicks;
        }
    }

    private void OnValidate()
    {
        visibleTicks =
            Mathf.Max(
                1,
                visibleTicks
            );

        fadeTicks =
            Mathf.Clamp(
                fadeTicks,
                0,
                visibleTicks
            );
    }
}