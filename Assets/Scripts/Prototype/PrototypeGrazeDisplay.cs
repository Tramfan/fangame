using TMPro;
using UnityEngine;

public sealed class PrototypeGrazeDisplay : MonoBehaviour
{
    [SerializeField]
    private PlayerState playerState;

    private TMP_Text label;
    private int displayedGraze = -1;

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

    private void Update()
    {
        if (label == null || playerState == null)
        {
            return;
        }

        if (displayedGraze == playerState.GrazeCount)
        {
            return;
        }

        displayedGraze = playerState.GrazeCount;
        label.text = $"Graze: {displayedGraze}";
    }
}