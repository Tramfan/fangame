using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class MenuButtonVisual :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler,
    ISubmitHandler
{
    [SerializeField]
    private TMP_Text label;

    [Header("Colors")]
    [SerializeField]
    private Color normalColor =
        new(0.91f, 0.93f, 0.95f, 1f);

    [SerializeField]
    private Color highlightedColor =
        new(0.5f, 0.91f, 1f, 1f);

    [SerializeField]
    private Color pressedColor =
        new(1f, 0.45f, 0.78f, 1f);

    [Header("Scale")]
    [SerializeField, Min(1f)]
    private float highlightedScale = 1.06f;

    [SerializeField, Range(0.5f, 1f)]
    private float pressedScale = 0.94f;

    [SerializeField, Min(0.01f)]
    private float submitFlashDuration = 0.08f;

    private Vector3 initialScale;

    private bool pointerInside;
    private bool selected;
    private bool pressed;

    private Coroutine submitRoutine;

    private void Awake()
    {
        if (label == null)
        {
            label =
                GetComponentInChildren<TMP_Text>();
        }

        if (label == null)
        {
            Debug.LogError(
                "Menu Button Visual has no Label.",
                this
            );

            enabled = false;
            return;
        }

        initialScale =
            label.rectTransform.localScale;
    }

    private void OnEnable()
    {
        pointerInside = false;
        pressed = false;

        UpdateVisual();
    }

    private void OnDisable()
    {
        if (submitRoutine != null)
        {
            StopCoroutine(submitRoutine);
            submitRoutine = null;
        }

        pointerInside = false;
        selected = false;
        pressed = false;

        if (label != null)
        {
            label.color = normalColor;

            label.rectTransform.localScale =
                initialScale;
        }
    }

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        pointerInside = true;
        UpdateVisual();
    }

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        pointerInside = false;
        pressed = false;

        UpdateVisual();
    }

    public void OnPointerDown(
        PointerEventData eventData
    )
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        pressed = true;
        UpdateVisual();
    }

    public void OnPointerUp(
        PointerEventData eventData
    )
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        pressed = false;
        UpdateVisual();
    }

    public void OnSelect(
        BaseEventData eventData
    )
    {
        selected = true;
        UpdateVisual();
    }

    public void OnDeselect(
        BaseEventData eventData
    )
    {
        selected = false;
        pressed = false;

        UpdateVisual();
    }

    public void OnSubmit(
        BaseEventData eventData
    )
    {
            if (!isActiveAndEnabled)
    {
        return;
    }
        if (submitRoutine != null)
        {
            StopCoroutine(submitRoutine);
        }

        submitRoutine =
            StartCoroutine(
                ShowSubmitFeedback()
            );
    }

    private IEnumerator ShowSubmitFeedback()
    {
        pressed = true;
        UpdateVisual();

        yield return new WaitForSecondsRealtime(
            submitFlashDuration
        );

        pressed = false;
        submitRoutine = null;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (label == null)
        {
            return;
        }

        Color targetColor;
        float targetScale;

        if (pressed)
        {
            targetColor = pressedColor;
            targetScale = pressedScale;
        }
        else if (pointerInside || selected)
        {
            targetColor = highlightedColor;
            targetScale = highlightedScale;
        }
        else
        {
            targetColor = normalColor;
            targetScale = 1f;
        }

        label.color = targetColor;

        label.rectTransform.localScale =
            initialScale * targetScale;
    }
}