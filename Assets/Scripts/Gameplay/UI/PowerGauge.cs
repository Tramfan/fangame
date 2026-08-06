using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PowerGauge : MonoBehaviour
{
    [SerializeField]
    private PlayerPower playerPower;

    [SerializeField]
    private Image fillImage;

    [SerializeField]
    private TMP_Text valueText;

    private void Awake()
    {
        if (playerPower == null)
        {
            Debug.LogError(
                "Power Gauge has no Player Power.",
                this
            );

            enabled = false;
            return;
        }

        if (fillImage == null)
        {
            Debug.LogError(
                "Power Gauge has no Fill Image.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (playerPower == null)
        {
            return;
        }

        playerPower.PowerChanged +=
            HandlePowerChanged;

        Refresh(
            playerPower.CurrentPower,
            playerPower.MaxPower
        );
    }

    private void Start()
    {
        if (playerPower != null)
        {
            Refresh(
                playerPower.CurrentPower,
                playerPower.MaxPower
            );
        }
    }

    private void OnDisable()
    {
        if (playerPower != null)
        {
            playerPower.PowerChanged -=
                HandlePowerChanged;
        }
    }

    private void HandlePowerChanged(
        int currentPower,
        int maxPower
    )
    {
        Refresh(currentPower, maxPower);
    }

    private void Refresh(
        int currentPower,
        int maxPower
    )
    {
        if (fillImage != null)
        {
            fillImage.fillAmount =
                maxPower > 0
                    ? Mathf.Clamp01(
                        (float)currentPower / maxPower
                    )
                    : 0f;
        }

        if (valueText != null)
        {
            valueText.text =
                $"{currentPower}/{maxPower}";
        }
    }
}