using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public sealed class GrazeDisplay : MonoBehaviour
{
    [SerializeField]
    private PlayerState playerState;

    [SerializeField]
    private LocalizedString labelFormat = new();

    private readonly object[] formatArguments =
        new object[1];

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
        int initialGraze =
            playerState != null
                ? playerState.GrazeCount
                : 0;

        formatArguments[0] = initialGraze;
        labelFormat.Arguments = formatArguments;

        labelFormat.StringChanged +=
            HandleLocalizedStringChanged;

        if (playerState != null)
        {
            playerState.GrazeChanged +=
                HandleGrazeChanged;
        }

        labelFormat.RefreshString();
    }

    private void OnDisable()
    {
        labelFormat.StringChanged -=
            HandleLocalizedStringChanged;

        if (playerState != null)
        {
            playerState.GrazeChanged -=
                HandleGrazeChanged;
        }
    }

    private void HandleGrazeChanged(int grazeCount)
    {
        formatArguments[0] = grazeCount;
        labelFormat.RefreshString();
    }

    private void HandleLocalizedStringChanged(
        string localizedText
    )
    {
        if (label != null)
        {
            label.text = localizedText;
        }
    }
}