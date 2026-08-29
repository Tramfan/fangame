using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[DisallowMultipleComponent]
public sealed class MainMenuController : MonoBehaviour
{
    private const string LanguagePreferenceKey =
        "selected_locale";

    [Header("Locales")]
    [SerializeField]
    private Locale englishLocale;

    [SerializeField]
    private Locale ukrainianLocale;

    private void Start()
    {
        ApplySavedLocale();
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
}