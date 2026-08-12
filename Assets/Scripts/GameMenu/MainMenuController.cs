using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class MainMenuController : MonoBehaviour
{
    private const string LanguagePreferenceKey =
        "selected_locale";

    [Header("Scene")]
    [SerializeField]
    private string gameplaySceneName = "TestStage";

    [Header("Locales")]
    [SerializeField]
    private Locale englishLocale;

    [SerializeField]
    private Locale ukrainianLocale;

    [Header("Navigation")]
    [SerializeField]
    private GameObject firstSelectedObject;

    private bool sceneLoading;

    private void Start()
    {
        ApplySavedLocale();
        SelectInitialButton();
    }

    public void StartGame()
    {
        if (sceneLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
                gameplaySceneName))
        {
            Debug.LogError(
                "Gameplay Scene Name is empty.",
                this
            );

            return;
        }

        sceneLoading = true;

        SceneManager.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single
        );
    }

    public void SelectEnglish()
    {
        SelectLocale(englishLocale);
    }

    public void SelectUkrainian()
    {
        SelectLocale(ukrainianLocale);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying =
            false;
#else
        Application.Quit();
#endif
    }

    private void ApplySavedLocale()
    {
        string savedLocaleCode =
            PlayerPrefs.GetString(
                LanguagePreferenceKey,
                string.Empty
            );

        if (englishLocale != null &&
            savedLocaleCode ==
            englishLocale.Identifier.Code)
        {
            ApplyLocale(
                englishLocale,
                false
            );

            return;
        }

        if (ukrainianLocale != null &&
            savedLocaleCode ==
            ukrainianLocale.Identifier.Code)
        {
            ApplyLocale(
                ukrainianLocale,
                false
            );
        }
    }

    private void SelectLocale(Locale locale)
    {
        ApplyLocale(locale, true);
    }

    private void ApplyLocale(
        Locale locale,
        bool savePreference
    )
    {
        if (locale == null)
        {
            Debug.LogError(
                "Menu locale is not assigned.",
                this
            );

            return;
        }

        LocalizationSettings.SelectedLocale =
            locale;

        if (!savePreference)
        {
            return;
        }

        PlayerPrefs.SetString(
            LanguagePreferenceKey,
            locale.Identifier.Code
        );

        PlayerPrefs.Save();
    }

    private void SelectInitialButton()
    {
        if (EventSystem.current == null)
        {
            Debug.LogError(
                "Main Menu has no Event System.",
                this
            );

            return;
        }

        if (firstSelectedObject != null)
        {
            EventSystem.current
                .SetSelectedGameObject(
                    firstSelectedObject
                );
        }
    }
}