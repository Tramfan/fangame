using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BossHud : MonoBehaviour
{
    [SerializeField]
    private BossPhaseController boss;

    [SerializeField]
    private GameObject displayRoot;

    [SerializeField]
    private Image healthFill;

    [SerializeField]
    private TMP_Text timerText;

    private EnemyHealth health;
    private int lastSecondsRemaining = -1;

    private void Awake()
    {
        if (boss == null)
        {
            Debug.LogError(
                "Boss HUD has no boss assigned.",
                this
            );

            enabled = false;
            return;
        }

        health = boss.GetComponent<EnemyHealth>();

        if (health == null)
        {
            Debug.LogError(
                "Assigned boss has no EnemyHealth.",
                boss
            );

            enabled = false;
            return;
        }

        if (healthFill == null)
        {
            Debug.LogError(
                "Boss HUD has no Health Fill assigned.",
                this
            );

            enabled = false;
            return;
        }

        if (timerText == null)
        {
            Debug.LogError(
                "Boss HUD has no Timer Text assigned.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        lastSecondsRemaining = -1;

        if (displayRoot != null)
        {
            displayRoot.SetActive(true);
        }

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (boss == null || !boss.IsPhaseActive)
        {
            if (displayRoot != null && displayRoot.activeSelf)
            {
                displayRoot.SetActive(false);
            }

            lastSecondsRemaining = -1;
            return;
        }

        if (displayRoot != null &&
            !displayRoot.activeSelf)
        {
            displayRoot.SetActive(true);
        }

        healthFill.fillAmount =
            health.MaxHealth > 0
                ? Mathf.Clamp01(
                    (float)health.CurrentHealth /
                    health.MaxHealth
                )
                : 0f;

        int secondsRemaining = boss.SecondsRemaining;

        if (secondsRemaining != lastSecondsRemaining)
        {
            lastSecondsRemaining = secondsRemaining;
            timerText.text = secondsRemaining.ToString();
        }
    }
}