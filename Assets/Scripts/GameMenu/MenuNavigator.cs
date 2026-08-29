using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class MenuNavigator : MonoBehaviour
{
    [SerializeField]
    private MenuScreen initialScreen;

    private readonly Stack<MenuScreen> history =
        new Stack<MenuScreen>();

    private MenuScreen[] screens;
    private MenuScreen currentScreen;

    private void Awake()
    {
        screens =
            GetComponentsInChildren<MenuScreen>(true);

        if (initialScreen == null)
        {
            Debug.LogError(
                "Menu Navigator has no Initial Screen.",
                this
            );

            enabled = false;
            return;
        }

        if (!IsRegistered(initialScreen))
        {
            Debug.LogError(
                "Initial Screen must be a child " +
                "of the Menu Navigator object.",
                this
            );

            enabled = false;
            return;
        }

        foreach (MenuScreen screen in screens)
        {
            screen.gameObject.SetActive(
                screen == initialScreen
            );
        }

        currentScreen = initialScreen;
    }

    private void Start()
    {
        SelectFirstObject();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) ||
    Input.GetKeyDown(KeyCode.Escape))
{
    Back();
}
    }

    public void Open(MenuScreen nextScreen)
    {
        if (nextScreen == null)
        {
            Debug.LogError(
                "Cannot open a null Menu Screen.",
                this
            );

            return;
        }

        if (!IsRegistered(nextScreen))
        {
            Debug.LogError(
                "Menu Screen must be a child " +
                "of the Menu Navigator object.",
                nextScreen
            );

            return;
        }

        if (nextScreen == currentScreen)
        {
            return;
        }

        if (currentScreen != null)
        {
            history.Push(currentScreen);
        }

        Show(nextScreen);
    }

    public void Back()
    {
        if (history.Count == 0)
        {
            return;
        }

        Show(history.Pop());
    }

    private void Show(MenuScreen nextScreen)
    {
        ClearSelection();

        if (currentScreen != null)
        {
            currentScreen.gameObject.SetActive(false);
        }

        nextScreen.gameObject.SetActive(true);
        currentScreen = nextScreen;

        SelectFirstObject();
    }

    private void SelectFirstObject()
    {
        if (EventSystem.current == null)
        {
            Debug.LogError(
                "Menu scene has no Event System.",
                this
            );

            return;
        }

        ClearSelection();

        GameObject firstSelected =
            currentScreen.FirstSelectedObject;

        if (firstSelected == null)
        {
            return;
        }

        if (!firstSelected.activeInHierarchy)
        {
            Debug.LogError(
                "First Selected Object is inactive.",
                firstSelected
            );

            return;
        }

        EventSystem.current.SetSelectedGameObject(
            firstSelected
        );
    }

    private void ClearSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                null
            );
        }
    }

    private bool IsRegistered(
        MenuScreen targetScreen
    )
    {
        foreach (MenuScreen screen in screens)
        {
            if (screen == targetScreen)
            {
                return true;
            }
        }

        return false;
    }
}