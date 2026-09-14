using TMPro;
using UnityEngine;
using System.Text;

[DisallowMultipleComponent]
public sealed class ScoreDisplay : MonoBehaviour
{
    [SerializeField, Range(1, 19)]
    private int minimumDigits = 10;
private readonly StringBuilder scoreBuilder =
    new();
    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();

        if (label == null)
        {
            Debug.LogError(
                "Score Display has no TMP Text component.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        GameRunContext.ScoreChanged +=
            HandleScoreChanged;

        Refresh(
            GameRunContext.CurrentScore
        );
    }

    private void OnDisable()
    {
        GameRunContext.ScoreChanged -=
            HandleScoreChanged;
    }

    private void HandleScoreChanged(
        long newScore
    )
    {
        Refresh(newScore);
    }

    private void Refresh(
        long score
    )
    {
        if (label == null)
        {
            return;
        }
label.text =
    FormatScore(score);
    }
private string FormatScore(
    long score
)
{
    string digits =
        score.ToString(
            $"D{minimumDigits}"
        );

    scoreBuilder.Clear();

    int firstGroupLength =
        digits.Length % 3;

    if (firstGroupLength == 0)
    {
        firstGroupLength = 3;
    }

    scoreBuilder.Append(
        digits,
        0,
        firstGroupLength
    );

    for (int index = firstGroupLength;
         index < digits.Length;
         index += 3)
    {
        scoreBuilder.Append(' ');

        scoreBuilder.Append(
            digits,
            index,
            3
        );
    }

    return scoreBuilder.ToString();
}
    private void OnValidate()
    {
        minimumDigits =
            Mathf.Clamp(
                minimumDigits,
                1,
                19
            );
    }
}