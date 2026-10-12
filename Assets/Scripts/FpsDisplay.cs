using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FpsDisplay : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float updateInterval = 0.5f;

    private TMP_Text label;
    private int frames;
    private float elapsedSeconds;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();

        if (label == null)
        {
            Debug.LogError(
                "FPS Display needs a TMP Text component.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        frames = 0;
        elapsedSeconds = 0f;

        if (label != null)
        {
            label.text = "FPS: ...";
        }
    }

    private void Update()
    {
        frames++;
        elapsedSeconds += Time.unscaledDeltaTime;

        if (elapsedSeconds <
            Mathf.Max(0.1f, updateInterval))
        {
            return;
        }

        int fps = Mathf.RoundToInt(
            frames / elapsedSeconds
        );

        label.text = $"FPS: {fps}";

        frames = 0;
        elapsedSeconds = 0f;
    }
}