using TMPro;
using UnityEngine;

public sealed class GrazeDisplay : MonoBehaviour
{
    [SerializeField]
    private PlayerState playerState;

    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();

        if (label == null)
        {
            Debug.LogError(
                "TMP Text component is missing.",
                this
            );
        }

        if (playerState == null)
        {
            Debug.LogError(
                "Player State is not assigned.",
                this
            );
        }
    }

    private void OnEnable()
    {
        if (playerState == null)
        {
            return;
        }

        playerState.GrazeChanged += HandleGrazeChanged;
        UpdateLabel(playerState.GrazeCount);
    }

    private void OnDisable()
    {
        if (playerState != null)
        {
            playerState.GrazeChanged -= HandleGrazeChanged;
        }
    }

    private void HandleGrazeChanged(int grazeCount)
    {
        UpdateLabel(grazeCount);
    }

    private void UpdateLabel(int grazeCount)
    {
        if (label != null)
        {
            label.text = $"Graze: {grazeCount}";
        }
    }
}