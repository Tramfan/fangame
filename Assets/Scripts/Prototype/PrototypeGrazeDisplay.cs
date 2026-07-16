using TMPro;
using UnityEngine;

public sealed class PrototypeGrazeDisplay : MonoBehaviour
{
    [SerializeField] private PrototypePlayerController player;

    private TMP_Text label;
    private int displayedGraze = -1;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();

        if (label == null)
        {
            Debug.LogError("TMP Text component is missing.", this);
        }

        if (player == null)
        {
            Debug.LogError("Player is not assigned.", this);
        }
    }

    private void Update()
    {
        if (label == null || player == null)
        {
            return;
        }

        if (displayedGraze == player.GrazeCount)
        {
            return;
        }

        displayedGraze = player.GrazeCount;
        label.text = $"Graze: {displayedGraze}";
    }
}