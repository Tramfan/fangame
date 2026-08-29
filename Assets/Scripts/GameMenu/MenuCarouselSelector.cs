using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuCarouselSelector :
    MonoBehaviour,
    IMoveHandler,
    ISubmitHandler,
    IPointerClickHandler
{
    public void Previous()
    {
        ChangeSelection(-1);
        SelectSelf();
    }

    public void Next()
    {
        ChangeSelection(1);
        SelectSelf();
    }

    public void Up()
    {
        if (ChangeSecondarySelection(-1))
        {
            SelectSelf();
        }
    }

    public void Down()
    {
        if (ChangeSecondarySelection(1))
        {
            SelectSelf();
        }
    }

    public void Confirm()
    {
        ConfirmSelection();
    }

    public void OnMove(
        AxisEventData eventData
    )
    {
        switch (eventData.moveDir)
        {
            case MoveDirection.Left:
                Previous();
                eventData.Use();
                break;

            case MoveDirection.Right:
                Next();
                eventData.Use();
                break;

            case MoveDirection.Up:
                Up();
                eventData.Use();
                break;

            case MoveDirection.Down:
                Down();
                eventData.Use();
                break;
        }
    }

    public void OnSubmit(
        BaseEventData eventData
    )
    {
        Confirm();
        eventData.Use();
    }

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        Confirm();
        eventData.Use();
    }

    protected abstract void ChangeSelection(
        int direction
    );

    protected virtual bool
        ChangeSecondarySelection(
            int direction
        )
    {
        return false;
    }

    protected abstract void ConfirmSelection();

    private void SelectSelf()
    {
        if (!gameObject.activeInHierarchy ||
            EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(
            gameObject
        );
    }
}