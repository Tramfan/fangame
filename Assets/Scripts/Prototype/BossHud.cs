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
        }
    }

    private void OnEnable()
    {
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
            if (displayRoot != null)
            {
                displayRoot.SetActive(false);
            }

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

        timerText.text =
            boss.SecondsRemaining.ToString();
    }
}